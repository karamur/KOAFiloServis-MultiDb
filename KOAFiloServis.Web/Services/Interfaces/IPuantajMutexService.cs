using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services.Interfaces;

public interface IPuantajMutexService
{
    Task<MutexAcquireResult> TryAcquireAsync(
        int firmaId, int yil, int ay, string tetikleyen,
        CancellationToken ct = default);

    Task UpdateToCompletedAsync(
        PuantajJobExecution mutex, PuantajEngineSonucV1 engineResult,
        CancellationToken ct = default);

    Task UpdateToFailedAsync(
        PuantajJobExecution mutex, string errorMessage,
        CancellationToken ct = default);

    Task UpdateToSkippedAsync(
        PuantajJobExecution mutex, string reason, int? hesapDonemiId = null,
        CancellationToken ct = default);

    Task CleanupStaleAsync(CancellationToken ct = default);
}

public sealed class MutexAcquireResult
{
    public bool Acquired { get; set; }
    public PuantajJobExecution? Record { get; set; }
    public string? FailureReason { get; set; }
}
