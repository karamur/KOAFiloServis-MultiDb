using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services.Interfaces;

public interface IPuantajJobService
{
    Task<PuantajJobExecution> ProcessAllTenantsAsync(int yil, int ay, string tetikleyen, CancellationToken ct = default);
    Task ProcessTenantAsync(int firmaId, string? databaseName, int yil, int ay, string tetikleyen, CancellationToken ct = default);
}
