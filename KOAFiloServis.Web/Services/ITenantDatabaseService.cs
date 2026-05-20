namespace KOAFiloServis.Web.Services;

public interface ITenantDatabaseService
{
    Task CreateTenantDatabaseAsync(int firmaId);
    Task<bool> TenantDatabaseExistsAsync(string databaseName);
}
