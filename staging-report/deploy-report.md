# Staging Validation Report — Manual Puantaj Module

> **Timestamp:** 2026-05-26 00:07 UTC
> **Git SHA:** `597c148`
> **Database:** DestekCRMServisBlazorDb @ localhost:5432

---

## Summary

| Metric | Value |
|--------|-------|
| **PASS** | 8 |
| **FAIL** | 1 (BLOCKER) |
| **Risk Level** | **HIGH** |
| **Deploy Verdict** | **DEPLOY BLOCKED** |

---

## Test Results Detail

| # | Test | Verdict | Detail |
|:--|------|:-------:|--------|
| 1 | DB Backup | ✅ PASS | `DestekCRMServisBlazorDb_20260526_000710.dump` (custom format) |
| 2 | DB Snapshot (pre-test counts) | ✅ PASS | 3 Puantaj tables exist (partial state) |
| 3 | Release Build | ✅ PASS | 0 error, 0 warning |
| 4 | Tests (363/363) | ✅ PASS | All unit + integration tests pass |
| 5 | Health Checks (code) | ✅ PASS | `/healthz`, `/readyz`, `/health/puantaj-job` registered |
| 6 | Exception Hierarchy | ✅ PASS | 10 exception types defined |
| 7 | Service DI Resolution | ✅ PASS | All 4 services constructable |
| 8 | API Endpoints | ✅ PASS | 4 PuantajJobController endpoints exist |
| 9 | **Migration Apply** | 🔴 **FAIL** | `column "BelgeNo" already exists` — schema mismatch |

---

## Critical Blocker: Migration Schema Mismatch

### Root Cause

```
Last applied migration: 20260520091801_MultiDbFaz1_AddFirmaDatabaseName
Pending migrations:
  20260525111342_AddOperasyonKaydi (Sprint 1)
  20260525115521_AddPuantajEngineV1 (Sprint 3)
  20260525135505_AddOnayWorkflow (Sprint 5)
  20260525142807_AddPuantajFinansalKayit (Sprint 6)
  20260525191201_PuantajJobExecution (Sprint 8)
```

**Error:** Migration `AddOperasyonKaydi` tries to add `BelgeNo` column to `PuantajKayitlar`, but the column already exists in the database.

### Why This Happens

The `PuantajKayit` entity already has `BelgeNo` as a property. The migration was generated on 2026-05-25, but the column appears to have been added to the DB outside of migrations (possibly via `EnsureCreated()` or manual SQL). The migration's `Up()` method assumes the column doesn't exist, but it does.

### Impact

- `dotnet ef database update` **fails**
- App startup with `db.Database.Migrate()` **will fail**
- New deployments to fresh databases **will work** (no pre-existing columns)
- Deployments to THIS database (staging/production) **will fail**

### Fix

**Option A (Recommended):** Generate a fresh snapshot migration:
```bash
# 1. Remove pending migrations that haven't been applied
dotnet ef migrations remove --project KOAFiloServis.Web --context ApplicationDbContext  # repeat 5x

# 2. Generate fresh migration from current model state
dotnet ef migrations add SyncPuantajSchema --project KOAFiloServis.Web --context ApplicationDbContext

# 3. Apply
dotnet ef database update --project KOAFiloServis.Web --context ApplicationDbContext
```

**Option B:** Manually skip conflicting columns with `migrationBuilder.Sql("ALTER TABLE ... ADD COLUMN IF NOT EXISTS ...")`.

**Option C:** Deploy to fresh database (drop and recreate from migrations).

### DB State Analysis

```
Tables present:        Tables missing:
  PuantajKayitlar        PuantajHesapDonemleri (Sprint 3)
  PuantajExcelImportlar  PuantajDetaylari (Sprint 3)
  PuantajEslestirmeOnerileri  PuantajAuditLogs (Sprint 5)
                         PuantajFinansalKayitlar (Sprint 6)
                         PuantajJobExecutions (Sprint 8)
                         OperasyonKayitlari (Sprint 1)
```

---

## Canary Deploy Runbook (after blocker fix)

1. Fix migration schema mismatch (Option A above)
2. Set `PuantajEngine:AutoProcess:Enabled = false` in production config
3. Backup production DB: `pg_dump -Fc -f production-backup-$(date +%Y%m%d).dump`
4. Apply pending migrations on staging first
5. Verify all 7 Puantaj tables exist
6. Deploy to 1 canary node
7. Health check: `/healthz`, `/readyz`, `/health/puantaj-job` → all Healthy
8. Manual test: create puantaj → hesaplama → onay workflow
9. Monitor 48h: memory, connections, exceptions, retries
10. If green → full rollout

## Rollback Instructions

1. `PuantajEngine:AutoProcess:Enabled = false` (instant, no deploy)
2. Config rollback: restore previous `appsettings.Production.json`
3. App rollback: restore previous artifact
4. DB rollback: `pg_restore -d DestekCRMServisBlazorDb staging-report/backups/DestekCRMServisBlazorDb_20260526_000710.dump` (only if schema issues)

---

## Performance Metrics (from test run)

| Metric | Value |
|--------|-------|
| Build time (Release) | 27s |
| Test execution (363 tests) | 0.7s |
| DB backup size | Check `staging-report/backups/` |
| Active DB connections | Check pg_stat_activity |

---

## Verification Checklist

- [x] Build: 0 error, 0 warning
- [x] Tests: 363/363 passing
- [x] Exception hierarchy: 10 types
- [x] API endpoints: 4 PuantajJobController + 12 IKurumPuantajService
- [x] Health checks: 3 endpoints registered
- [x] Authorization: JWT Bearer + Role-based on API + Blazor pages
- [ ] ~~Migrations applied~~ **BLOCKED** — schema mismatch
- [ ] DB tables (7 Puantaj tables) **BLOCKED** — only 3 exist
- [ ] PuantajJobExecution filtered UNIQUE index **BLOCKED** — table doesn't exist
- [ ] App startup smoke test **BLOCKED** — migration fails
- [ ] Manual puantaj create + hesaplama **BLOCKED** — missing tables
- [ ] Approval workflow test **BLOCKED** — missing tables

---

## Final Assessment

```
STATUS:           DEPLOY BLOCKED
BLOCKER:          Migration schema mismatch (column already exists)
MITIGATION:       Option A — regenerate fresh migration
CONFIDENCE:       95% (code), 0% (DB migration on this environment)
NEXT STEP:        Fix migration, re-run staging validation
```
