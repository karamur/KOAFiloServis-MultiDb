namespace KOAFiloServis.Web.Services;

public interface ITenantDatabaseService
{
    Task CreateTenantDatabaseAsync(int firmaId, bool migrateData = true);
    Task<bool> TenantDatabaseExistsAsync(string databaseName);
    Task MigrateFirmaDataAsync(int firmaId);
}
