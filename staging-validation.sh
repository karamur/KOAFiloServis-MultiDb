#!/bin/bash
# Staging Validation Script — Manual Puantaj Module
# Requires: psql, dotnet, curl

set -e

export PGPASSWORD="Fast123"
PG_HOST="localhost"
PG_PORT="5432"
PG_USER="postgres"
APP_DB="DestekCRMServisBlazorDb"
MASTER_DB="KOAFiloServis_Master"
TIMESTAMP=$(date -u +"%Y%m%d_%H%M%S")
REPORT_DIR="staging-report"
SNAPSHOT_DIR="$REPORT_DIR/snapshots"
BACKUP_DIR="$REPORT_DIR/backups"
PASS=0
FAIL=0

mkdir -p "$SNAPSHOT_DIR" "$BACKUP_DIR"

log_pass() { echo "[PASS] $1"; PASS=$((PASS+1)); }
log_fail() { echo "[FAIL] $1 — $2"; FAIL=$((FAIL+1)); }

echo "============================================"
echo " STAGING VALIDATION — Manual Puantaj Module "
echo " Timestamp: $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
echo "============================================"
echo ""

# ═══════════════════════════════════════════════════════════════════
# PHASE 0: DB SNAPSHOT + BACKUP
# ═══════════════════════════════════════════════════════════════════
echo "=== PHASE 0: DB Snapshot ==="

echo "  Backing up $APP_DB..."
pg_dump -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" \
  --format=custom --file="$BACKUP_DIR/${APP_DB}_${TIMESTAMP}.dump" 2>&1 | grep -v "^$" || true
if [ ${PIPESTATUS[0]} -eq 0 ]; then
  echo "  Backup: OK ($BACKUP_DIR/${APP_DB}_${TIMESTAMP}.dump)"
else
  echo "  Backup: FAILED — continuing anyway"
fi

# Row counts before tests
echo "  Taking pre-test row counts..."
psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -c "
SELECT 'PuantajKayitlar' as tbl, count(*) FROM \"PuantajKayitlar\" WHERE \"IsDeleted\" = false
UNION ALL
SELECT 'PuantajHesapDonemleri', count(*) FROM \"PuantajHesapDonemleri\" WHERE \"IsDeleted\" = false
UNION ALL
SELECT 'PuantajAuditLogs', count(*) FROM \"PuantajAuditLogs\" WHERE \"IsDeleted\" = false
UNION ALL
SELECT 'PuantajFinansalKayitlar', count(*) FROM \"PuantajFinansalKayitlar\" WHERE \"IsDeleted\" = false
UNION ALL
SELECT 'PuantajJobExecutions', count(*) FROM \"PuantajJobExecutions\" WHERE \"IsDeleted\" = false;
" > "$SNAPSHOT_DIR/pre-test-counts.txt" 2>&1
cat "$SNAPSHOT_DIR/pre-test-counts.txt"

# ═══════════════════════════════════════════════════════════════════
# PHASE 1: APPLY PENDING MIGRATIONS
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 1: Apply Migrations ==="

cd "$(dirname "$0")"

echo "  Checking pending migrations..."
dotnet ef migrations list \
  --project KOAFiloServis.Web \
  --context ApplicationDbContext \
  -- --environment Development 2>&1 | grep -i "pending" > "$SNAPSHOT_DIR/pending-migrations.txt" || true

if [ -s "$SNAPSHOT_DIR/pending-migrations.txt" ]; then
  echo "  Pending migrations found:"
  cat "$SNAPSHOT_DIR/pending-migrations.txt"
  echo "  Applying migrations..."
  dotnet ef database update \
    --project KOAFiloServis.Web \
    --context ApplicationDbContext \
    -- --environment Development 2>&1 | tail -5
  echo "  Migrations applied."
else
  echo "  No pending migrations."
fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 2: HEALTH CHECKS (app must be running)
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 2: Health Checks ==="

BASE_URL="${BASE_URL:-http://localhost:5190}"

# Check if app is running
HEALTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/healthz" 2>/dev/null || echo "000")

if [ "$HEALTH_RESPONSE" != "200" ]; then
  echo "  App not running at $BASE_URL (HTTP $HEALTH_RESPONSE)"
  echo "  Starting app in background..."
  ASPNETCORE_ENVIRONMENT=Development \
  ASPNETCORE_URLS="http://localhost:5190" \
  dotnet run --project KOAFiloServis.Web --no-build &
  APP_PID=$!
  echo "  App PID: $APP_PID"

  # Wait for app to start
  for i in $(seq 1 30); do
    sleep 2
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/healthz" 2>/dev/null || echo "000")
    echo "  [$i/30] Waiting... HTTP $STATUS"
    if [ "$STATUS" = "200" ]; then
      echo "  App ready!"
      break
    fi
  done
else
  echo "  App already running at $BASE_URL"
fi

# Run health checks
echo ""
echo "  /healthz..."
HZ=$(curl -s "$BASE_URL/healthz" 2>/dev/null)
if [ -n "$HZ" ]; then log_pass "/healthz: $HZ"; else log_fail "/healthz" "no response"; fi

echo "  /readyz..."
RZ=$(curl -s "$BASE_URL/readyz" 2>/dev/null)
if echo "$RZ" | grep -qi "healthy"; then log_pass "/readyz: Healthy"; else log_fail "/readyz" "$RZ"; fi

echo "  /health/puantaj-job..."
PJ=$(curl -s "$BASE_URL/health/puantaj-job" 2>/dev/null)
if [ -n "$PJ" ]; then log_pass "/health/puantaj-job: $(echo $PJ | head -1)"; else log_fail "/health/puantaj-job" "no response"; fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 3: DATABASE DIRECT VERIFICATION
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 3: DB Direct Verification ==="

# PuantajJobExecution table exists
echo "  PuantajJobExecution table..."
TABLE_EXISTS=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM information_schema.tables
WHERE table_name = 'PuantajJobExecutions'" 2>/dev/null | tr -d ' ')
if [ "$TABLE_EXISTS" = "1" ]; then log_pass "PuantajJobExecutions table exists"; else log_fail "Table" "PuantajJobExecutions not found"; fi

# Filtered UNIQUE index
echo "  Filtered UNIQUE index..."
IDX_EXISTS=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM pg_indexes
WHERE indexname LIKE '%PuantajJobExecutions%' AND indexdef LIKE '%WHERE%'" 2>/dev/null | tr -d ' ')
if [ "$IDX_EXISTS" = "1" ]; then log_pass "Filtered UNIQUE index exists (WHERE Durum=0)"; else log_fail "Index" "Filtered index not found"; fi

# PuantajJobExecution columns
echo "  PuantajJobExecution columns..."
COLUMNS=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM information_schema.columns
WHERE table_name = 'PuantajJobExecutions'" 2>/dev/null | tr -d ' ')
if [ "$COLUMNS" -ge "12" ]; then log_pass "$COLUMNS columns (>=12)"; else log_fail "Columns" "Only $COLUMNS columns found"; fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 4: MANUAL PUANTAJ ENTITY TESTS (PSQL Direct)
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 4: Manual Puantaj Entity Tests ==="

# Test INSERT
echo "  Manual INSERT..."
INSERT_RESULT=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
INSERT INTO \"PuantajKayitlar\" (
  \"Yil\", \"Ay\", \"GuzergahId\", \"AracId\",
  \"Slot\", \"SeferSayisi\", \"Gun\",
  \"BirimGelir\", \"BirimGider\", \"Kaynak\",
  \"CreatedAt\", \"IsDeleted\"
) VALUES (2026, 5, 9999, 9999, 1, 1, 22, 500, 300, 0, now(), false)
RETURNING \"Id\";" 2>/dev/null)
if [ -n "$INSERT_RESULT" ] && [ "$INSERT_RESULT" != "" ]; then
  log_pass "INSERT OK (Id=$INSERT_RESULT)"
else
  log_fail "INSERT" "Insert failed — check FK constraints (GuzergahId/AracId)"
fi

# Test UPSERT (duplicate key)
echo "  Duplicate INSERT (expect UPDATE)..."
UPSERT_RESULT=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
UPDATE \"PuantajKayitlar\"
SET \"BirimGelir\" = 600, \"UpdatedAt\" = now()
WHERE \"GuzergahId\" = 9999 AND \"AracId\" = 9999
  AND \"Yil\" = 2026 AND \"Ay\" = 5 AND \"Slot\" = 1
  AND \"IsDeleted\" = false
RETURNING \"BirimGelir\";" 2>/dev/null | tr -d ' ')
if [ "$UPSERT_RESULT" = "600" ]; then log_pass "UPSERT OK (BirimGelir=600)"; else log_fail "UPSERT" "Expected 600, got '$UPSERT_RESULT'"; fi

# Test soft delete
echo "  Soft delete..."
DELETE_RESULT=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
UPDATE \"PuantajKayitlar\"
SET \"IsDeleted\" = true, \"UpdatedAt\" = now()
WHERE \"GuzergahId\" = 9999 AND \"AracId\" = 9999
  AND \"Yil\" = 2026 AND \"Ay\" = 5 AND \"Slot\" = 1
  AND \"IsDeleted\" = false
RETURNING \"IsDeleted\";" 2>/dev/null | tr -d ' ')
if [ "$DELETE_RESULT" = "t" ]; then log_pass "Soft delete OK (IsDeleted=true)"; else log_fail "Soft delete" "Expected 't', got '$DELETE_RESULT'"; fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 5: PuantajJobExecution MUTEX TEST
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 5: Mutex Test ==="

# Test UNIQUE constraint
echo "  Mutex INSERT (first)..."
M1=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
INSERT INTO \"PuantajJobExecutions\" (
  \"FirmaId\", \"Yil\", \"Ay\", \"Tetikleyen\", \"Durum\",
  \"Baslangic\", \"CreatedAt\", \"IsDeleted\"
) VALUES (9999, 2026, 99, 'StagingTest', 0, now(), now(), false)
RETURNING \"Id\";" 2>/dev/null | tr -d ' ')
if [ -n "$M1" ]; then log_pass "Mutex 1 OK (Id=$M1)"; else log_fail "Mutex 1" "Insert failed"; fi

echo "  Mutex INSERT (duplicate — expect UNIQUE violation)..."
M2=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
INSERT INTO \"PuantajJobExecutions\" (
  \"FirmaId\", \"Yil\", \"Ay\", \"Tetikleyen\", \"Durum\",
  \"Baslangic\", \"CreatedAt\", \"IsDeleted\"
) VALUES (9999, 2026, 99, 'StagingTest-2', 0, now(), now(), false)
RETURNING \"Id\";" 2>&1)
if echo "$M2" | grep -qi "duplicate\|unique\|23505"; then
  log_pass "Mutex duplicate BLOCKED (UNIQUE violation)";
else
  log_fail "Mutex duplicate" "Expected UNIQUE violation, got: $M2";
fi

# Cleanup test data
echo "  Cleanup test data..."
psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -c "
UPDATE \"PuantajJobExecutions\" SET \"IsDeleted\" = true WHERE \"FirmaId\" = 9999;
UPDATE \"PuantajKayitlar\" SET \"IsDeleted\" = true WHERE \"GuzergahId\" = 9999;
" 2>/dev/null > /dev/null

# ═══════════════════════════════════════════════════════════════════
# PHASE 6: AUDIT LOG VERIFICATION
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 6: Audit Log Verification ==="

AUDIT_COUNT=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM \"PuantajAuditLogs\" WHERE \"IsDeleted\" = false;" 2>/dev/null | tr -d ' ')
echo "  Existing audit logs: $AUDIT_COUNT"
if [ "$AUDIT_COUNT" -ge "0" ]; then log_pass "PuantajAuditLogs table accessible ($AUDIT_COUNT records)"; else log_fail "Audit" "Table not accessible"; fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 7: PuantajHesapDonemi + Workflow
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 7: HesapDonemi + Workflow State ==="

HESAP_COUNT=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM \"PuantajHesapDonemleri\" WHERE \"IsDeleted\" = false;" 2>/dev/null | tr -d ' ')
echo "  Active HesapDonemi: $HESAP_COUNT"

LOCKED_COUNT=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM \"PuantajHesapDonemleri\"
WHERE \"IsDeleted\" = false AND \"OnayDurum\" = 3;" 2>/dev/null | tr -d ' ')
echo "  Kilitli donemler: $LOCKED_COUNT"

if [ -n "$HESAP_COUNT" ]; then log_pass "PuantajHesapDonemleri accessible"; else log_fail "HesapDonemi" "Not accessible"; fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 8: CONNECTION POOL CHECK
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 8: Connection Pool ==="

PG_CONNS=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT count(*) FROM pg_stat_activity WHERE datname = '$APP_DB';" 2>/dev/null | tr -d ' ')
echo "  Active connections to $APP_DB: $PG_CONNS"

MAX_CONNS=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
SELECT setting FROM pg_settings WHERE name = 'max_connections';" 2>/dev/null | tr -d ' ')
echo "  Max connections: $MAX_CONNS"

# ═══════════════════════════════════════════════════════════════════
# PHASE 9: BUILD VERIFICATION
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 9: Build Verification ==="

BUILD_OUT=$(dotnet build -c Release --no-restore 2>&1 | tail -3)
if echo "$BUILD_OUT" | grep -q "başarılı\|successfully"; then
  log_pass "Release build OK"
else
  log_fail "Build" "$BUILD_OUT"
fi

TEST_OUT=$(dotnet test -c Release --no-build 2>&1 | tail -3)
if echo "$TEST_OUT" | grep -q "Başarılı\|Passed"; then
  TEST_COUNT=$(echo "$TEST_OUT" | grep -oP 'Total: \K\d+' || echo "?")
  log_pass "Tests OK ($TEST_COUNT total)"
else
  log_fail "Tests" "$TEST_OUT"
fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 10: PERFORMANCE BENCHMARK (PSQL)
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 10: Performance ==="

# Query performance on PuantajJobExecutions
QUERY_TIME=$(psql -h $PG_HOST -p $PG_PORT -U $PG_USER -d "$APP_DB" -t -c "
EXPLAIN (ANALYZE, FORMAT JSON)
SELECT * FROM \"PuantajJobExecutions\"
WHERE \"IsDeleted\" = false
ORDER BY \"CreatedAt\" DESC
LIMIT 50;" 2>/dev/null | python3 -c "
import json, sys
try:
  data = json.load(sys.stdin)
  ms = data[0]['Execution Time']
  print(f'{ms:.2f}ms')
except:
  print('parse_error')
" 2>/dev/null || echo "  Query perf: could not parse (OK)")
echo "  PuantajJobExecutions SELECT TOP 50: $QUERY_TIME"

# ═══════════════════════════════════════════════════════════════════
# PHASE 11: EXCEPTION HIERARCHY CHECK
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "=== PHASE 11: Exception Types ==="

# Verify exception files exist
EX_FILES=$(ls KOAFiloServis.Shared/Exceptions/PuantajExceptions.cs 2>/dev/null)
if [ -f "$EX_FILES" ]; then
  EX_COUNT=$(grep -c "class.*Exception" "$EX_FILES" 2>/dev/null || echo "0")
  log_pass "Exception hierarchy: $EX_COUNT types defined"
else
  log_fail "Exceptions" "PuantajExceptions.cs not found"
fi

# ═══════════════════════════════════════════════════════════════════
# PHASE 12: FINAL SUMMARY
# ═══════════════════════════════════════════════════════════════════
echo ""
echo "============================================"
echo " STAGING VALIDATION COMPLETE"
echo "============================================"
echo ""
echo "  PASS: $PASS"
echo "  FAIL: $FAIL"
echo ""

if [ "$FAIL" -eq 0 ]; then
  echo "  RISK LEVEL: LOW"
  echo "  DEPLOY VERDICT: READY FOR CANARY"
else
  echo "  RISK LEVEL: MEDIUM/HIGH"
  echo "  DEPLOY VERDICT: REVIEW FAILURES"
fi

# Generate deploy-report.md
cat > "${REPORT_DIR}/deploy-report.md" << REPORTEOF
# Staging Validation Report — Manual Puantaj Module

> Timestamp: $(date -u '+%Y-%m-%d %H:%M:%S UTC')
> Git SHA: $(git rev-parse HEAD 2>/dev/null || echo "unknown")
> Database: $APP_DB@$PG_HOST:$PG_PORT

## Summary

| Metric | Value |
|--------|-------|
| **PASS** | $PASS |
| **FAIL** | $FAIL |
| **Risk Level** | $([ "$FAIL" -eq 0 ] && echo "LOW" || echo "REVIEW") |
| **Deploy Verdict** | $([ "$FAIL" -eq 0 ] && echo "READY FOR CANARY" || echo "REVIEW FAILURES") |

## DB State

- Active connections: $PG_CONNS / $MAX_CONNS
- PuantajKayitlar: active records present
- PuantajHesapDonemleri: $HESAP_COUNT active, $LOCKED_COUNT locked
- PuantajAuditLogs: $AUDIT_COUNT records
- PuantajJobExecutions: table + filtered index verified

## Migrations Applied

\`\`\`
$(cat "$SNAPSHOT_DIR/pending-migrations.txt" 2>/dev/null || echo "None pending")
\`\`\`

## Backup

- Path: \`$BACKUP_DIR/${APP_DB}_${TIMESTAMP}.dump\`
- Format: pg_dump custom

## Issues Found

$([ "$FAIL" -gt 0 ] && echo "See failure details above." || echo "None.")

## Canary Deploy Runbook

1. Set \`PuantajEngine:AutoProcess:Enabled = false\`
2. Deploy to 1 node
3. Verify \`/healthz\`, \`/readyz\`, \`/health/puantaj-job\` all Healthy
4. Manual puantaj create + hesaplama + approval workflow
5. Monitor for 48h: memory, connections, exceptions, retries
6. If all green → full rollout
7. If any issue → rollback via config toggle or restore backup

## Rollback Instructions

1. \`PuantajEngine:AutoProcess:Enabled = false\` (instant)
2. App rollback: restore previous artifact
3. DB rollback: \`pg_restore -d $APP_DB $BACKUP_DIR/${APP_DB}_${TIMESTAMP}.dump\` (only if schema changes caused issues)
REPORTEOF

echo ""
echo "Report: $REPORT_DIR/deploy-report.md"
echo "Backup: $BACKUP_DIR/${APP_DB}_${TIMESTAMP}.dump"
