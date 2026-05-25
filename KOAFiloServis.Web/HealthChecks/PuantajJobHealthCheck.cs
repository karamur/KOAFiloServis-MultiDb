using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KOAFiloServis.Web.HealthChecks;

public sealed class PuantajJobHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public PuantajJobHealthCheck(IDbContextFactory<ApplicationDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var lastRun = await db.PuantajJobExecutions
            .Where(j => !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new
            {
                Durum = (int)j.Durum,
                j.Baslangic,
                j.Tetikleyen,
                j.IslenenOperasyon,
                j.UretilenPuantaj
            })
            .FirstOrDefaultAsync(ct);

        if (lastRun == null)
            return HealthCheckResult.Healthy("Henüz çalışmadı");

        var elapsed = DateTime.UtcNow - (lastRun.Baslangic ?? DateTime.MinValue);
        var status = (PuantajJobExecutionDurum)lastRun.Durum;

        var data = new Dictionary<string, object>
        {
            ["LastRun"] = lastRun.Baslangic?.ToString("O") ?? "N/A",
            ["LastStatus"] = status.ToString(),
            ["Trigger"] = lastRun.Tetikleyen,
            ["HoursSinceLastRun"] = elapsed.TotalHours.ToString("F1"),
            ["LastOperations"] = lastRun.IslenenOperasyon,
            ["LastPuantajRecords"] = lastRun.UretilenPuantaj
        };

        if (elapsed.TotalHours > 36 && status != PuantajJobExecutionDurum.Running)
            return HealthCheckResult.Degraded(
                $"Son çalışma {elapsed.TotalHours:F0} saat önce", data: data);

        if (status == PuantajJobExecutionDurum.Failed || status == PuantajJobExecutionDurum.PartialSuccess)
            return HealthCheckResult.Degraded($"Son çalışma {status}", data: data);

        return HealthCheckResult.Healthy("OK", data);
    }
}
