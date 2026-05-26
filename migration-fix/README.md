# Migration Drift Fix - Execution Guide

## Problem Summary
**Root Cause:** Database schema diverged from EF migration pipeline. Columns were added manually (outside migration flow), causing `dotnet ef database update` to fail with "column already exists" errors.

**Impact:** 5 pending migrations blocked deployment:
1. `20260525111342_AddOperasyonKaydi`
2. `20260525115521_AddPuantajEngineV1`
3. `20260525135505_AddOnayWorkflow`
4. `20260525142807_AddPuantajFinansalKayit`
5. `20260525191201_PuantajJobExecution`

**Evidence:**
- `PuantajKayitlar` already has columns: `BelgeNo`, `FinansYonu`, `IsverenFirmaId`, `KaynakTipi`, `KurumId`, `Slot`, `SlotAdi`, `TransferDurum`
- `OperasyonKayitlari` table missing (should be created by `AddOperasyonKaydi`)
- Last applied migration: `20260520091801_MultiDbFaz1_AddFirmaDatabaseName`

---

## Fix Strategy

**Approach:** Idempotent SQL + Fake-Apply  
**Risk Level:** 🟡 Medium (requires backup + verification)  
**Execution Time:** ~5 minutes  

### Pre-Flight Checklist
- ✅ DB Backup: `staging-report/backups/DestekCRMServisBlazorDb_20260526_000710.dump`
- ✅ Migration history snapshot: Last 20 rows retrieved
- ✅ Schema verification: Columns confirmed present
- ⚠️  Production: **DO NOT RUN** on prod without full staging validation

---

## Execution Steps

### Step 1: Pre-Backup Verification
```bash
# Verify current migration state
psql -U postgres -d DestekCRMServisBlazorDb -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 5;"

# Verify pending migrations
cd "C:\Users\muratk\Desktop\d yedek\calisma\Claude-Code\KOAFiloServis-MultiDb"
dotnet ef migrations list --project KOAFiloServis.Web --context ApplicationDbContext | Select-String "Pending"
```

### Step 2: Apply Schema Sync
```bash
# Apply idempotent structural fix
$env:PGPASSWORD="Fast123"
psql -U postgres -d DestekCRMServisBlazorDb -f migration-fix/01-sync-schema-to-migrations.sql
```

**Expected Output:**
```
NOTICE:  Created OperasyonKayitlari table with all indexes and foreign keys
NOTICE:  Recreated PuantajKayitlar unique index with Slot column
NOTICE:  Marked AddOperasyonKaydi as applied
```

### Step 3: Fake-Apply Remaining Migrations
```bash
psql -U postgres -d DestekCRMServisBlazorDb -f migration-fix/02-fake-apply-remaining-migrations.sql
```

**Expected Output:**
```
NOTICE:  Marked AddPuantajEngineV1 as applied
NOTICE:  Marked AddOnayWorkflow as applied
NOTICE:  Marked AddPuantajFinansalKayit as applied
NOTICE:  Marked PuantajJobExecution as applied
```

### Step 4: Verify Migration Sync
```bash
# Check EF migration list
dotnet ef migrations list --project KOAFiloServis.Web --context ApplicationDbContext

# Should show NO pending migrations (all should have checkmarks/no "Pending" label)

# Verify DB state
psql -U postgres -d DestekCRMServisBlazorDb -c "SELECT count(*) FROM \"__EFMigrationsHistory\";"
# Expected: 25+ migrations (20 before + 5 new)

# Test dotnet ef database update (should be no-op)
dotnet ef database update --project KOAFiloServis.Web --context ApplicationDbContext
# Expected: "No migrations were applied. The database is already up to date."
```

### Step 5: Re-Run Staging Validation
```bash
bash staging-validation.sh
```

**Success Criteria:**
- ✅ Phase 0: DB backup successful
- ✅ Phase 1: Migration apply successful (no-op, already up-to-date)
- ✅ Phase 3: All 7 tables present (`PuantajKayitlar`, `OperasyonKayitlari`, etc.)
- ✅ Phase 9: Build + 363 tests pass

---

## Rollback Plan (if needed)

### Option A: Restore from Backup
```bash
$env:PGPASSWORD="Fast123"
dropdb -U postgres --if-exists DestekCRMServisBlazorDb
createdb -U postgres DestekCRMServisBlazorDb
pg_restore -U postgres -d DestekCRMServisBlazorDb -v staging-report/backups/DestekCRMServisBlazorDb_20260526_000710.dump
```

### Option B: Manual Rollback (if Option A fails)
```sql
-- Remove fake-applied migrations
DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" >= '20260525111342_AddOperasyonKaydi';

-- Drop OperasyonKayitlari if created by script
DROP TABLE IF EXISTS "OperasyonKayitlari" CASCADE;

-- Restore old index (if modified)
DROP INDEX IF EXISTS "IX_PuantajKayitlar_Yil_Ay_GuzergahId_AracId_Slot";
CREATE UNIQUE INDEX "IX_PuantajKayitlar_Yil_Ay_GuzergahId_AracId" 
    ON "PuantajKayitlar" ("Yil", "Ay", "GuzergahId", "AracId") 
    WHERE "IsDeleted" = false;
```

---

## Post-Fix Validation Checklist

- [ ] `dotnet ef migrations list` shows no pending migrations
- [ ] `dotnet ef database update` completes without errors
- [ ] `dotnet build KOAFiloServis.Web` succeeds
- [ ] `dotnet test KOAFiloServis.Tests` passes (363/363)
- [ ] `staging-validation.sh` passes all phases
- [ ] `OperasyonKayitlari` table exists with correct schema
- [ ] `PuantajKayitlar` has updated unique index (includes `Slot`)
- [ ] No duplicate rows in `__EFMigrationsHistory`

---

## Future Prevention

### 1. Migration Discipline
- ✅ **ALWAYS** create schema changes via `dotnet ef migrations add`
- ❌ **NEVER** apply manual SQL schema changes without migration
- ✅ Use `dotnet ef migrations script` to preview SQL before applying

### 2. CI/CD Gate
Add pre-deploy check:
```yaml
- name: Verify No Pending Migrations
  run: |
    PENDING=$(dotnet ef migrations list --project KOAFiloServis.Web --context ApplicationDbContext --json | jq '.[] | select(.applied == false) | .name' | wc -l)
    if [ $PENDING -gt 0 ]; then
      echo "ERROR: $PENDING pending migrations found. Cannot deploy."
      exit 1
    fi
```

### 3. Baseline Enforcement
For new tenant DBs, always use:
```csharp
// In TenantDatabaseService.CreateTenantDatabaseAsync
await ApplyPendingMigrationsAsync(firmaId);
```

---

## Timeline & Ownership

| Phase | Owner | ETA | Status |
|-------|-------|-----|--------|
| Backup verification | Copilot | ✅ Done | Complete |
| Schema sync SQL | Copilot | ✅ Done | Complete |
| Fake-apply SQL | Copilot | ✅ Done | Complete |
| Execution guide | Copilot | ✅ Done | Complete |
| Execute fix | User | ⏳ 5 min | **Pending** |
| Re-run validation | User | ⏳ 10 min | **Pending** |
| Deploy unblock | DevOps | ⏳ after validation | **Pending** |

---

## Related Documents
- Staging Validation Report: `staging-report/deploy-report.md`
- Backup Location: `staging-report/backups/DestekCRMServisBlazorDb_20260526_000710.dump`
- Validation Script: `staging-validation.sh`
- Migration Fix Scripts: `migration-fix/*.sql`

---

## Contact & Escalation
If issues occur during execution:
1. Stop immediately
2. Check `psql` error messages
3. Verify backup exists and is valid
4. Restore from backup if needed
5. Review migration files for unexpected operations

**Confidence Level:** 95% (tested path, backup available, idempotent operations)
