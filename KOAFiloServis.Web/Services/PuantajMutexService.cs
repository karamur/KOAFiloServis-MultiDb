using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace KOAFiloServis.Web.Services;

public sealed class PuantajMutexService : IPuantajMutexService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<PuantajMutexService> _logger;

    public PuantajMutexService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogger<PuantajMutexService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<MutexAcquireResult> TryAcquireAsync(
        int firmaId, int yil, int ay, string tetikleyen,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var mutex = new PuantajJobExecution
        {
            FirmaId = firmaId,
            Yil = yil, Ay = ay,
            Tetikleyen = tetikleyen,
            Durum = PuantajJobExecutionDurum.Running,
            Baslangic = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            db.PuantajJobExecutions.Add(mutex);
            await db.SaveChangesAsync(ct);
            return new MutexAcquireResult { Acquired = true, Record = mutex };
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _logger.LogWarning(
                "Mutex alınamadı — Firma {FirmaId} {Yil}/{Ay} zaten işleniyor",
                firmaId, yil, ay);
            return new MutexAcquireResult
            {
                Acquired = false,
                FailureReason = "Aynı dönem zaten işleniyor veya tamamlanmış"
            };
        }
    }

    public async Task UpdateToCompletedAsync(
        PuantajJobExecution mutex, PuantajEngineSonucV1 engineResult,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.PuantajJobExecutions.Attach(mutex);

        mutex.Durum = PuantajJobExecutionDurum.Completed;
        mutex.HesapDonemiId = engineResult.HesapDonemiId;
        mutex.Versiyon = engineResult.Versiyon;
        mutex.IslenenOperasyon = engineResult.IslenenOperasyonSayisi;
        mutex.UretilenPuantaj = engineResult.UretilenPuantajKayit;
        mutex.Bitis = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateToFailedAsync(
        PuantajJobExecution mutex, string errorMessage,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.PuantajJobExecutions.Attach(mutex);

        mutex.Durum = PuantajJobExecutionDurum.Failed;
        mutex.HataMesaji = Truncate(errorMessage, 990);
        mutex.Bitis = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateToSkippedAsync(
        PuantajJobExecution mutex, string reason, int? hesapDonemiId = null,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.PuantajJobExecutions.Attach(mutex);

        mutex.Durum = PuantajJobExecutionDurum.Skipped;
        mutex.HataMesaji = Truncate(reason, 990);
        mutex.HesapDonemiId = hesapDonemiId;
        mutex.Bitis = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task CleanupStaleAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var threshold = DateTime.UtcNow.AddMinutes(-30);

        var stale = await db.PuantajJobExecutions
            .Where(j => j.Durum == PuantajJobExecutionDurum.Running
                        && j.Baslangic < threshold
                        && !j.IsDeleted)
            .ToListAsync(ct);

        if (stale.Count == 0) return;

        foreach (var r in stale)
        {
            r.Durum = PuantajJobExecutionDurum.Failed;
            r.HataMesaji = "Stale — 30dk timeout (crash recovery)";
            r.Bitis = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        _logger.LogWarning("Stale cleanup: {Count} Running kaydı Failed yapıldı", stale.Count);
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pgEx
        && pgEx.SqlState == PostgresErrorCodes.UniqueViolation;

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
