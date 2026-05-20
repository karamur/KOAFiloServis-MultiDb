using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace KOAFiloServis.Web.Services;

public sealed class TenantDatabaseService : ITenantDatabaseService
{
    private readonly ITenantConnectionStringProvider _connProvider;
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly IDbContextFactory<ApplicationDbContext> _tenantFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantDatabaseService> _logger;

    public TenantDatabaseService(
        ITenantConnectionStringProvider connProvider,
        IDbContextFactory<MasterDbContext> masterFactory,
        IDbContextFactory<ApplicationDbContext> tenantFactory,
        IConfiguration configuration,
        ILogger<TenantDatabaseService> logger)
    {
        _connProvider = connProvider;
        _masterFactory = masterFactory;
        _tenantFactory = tenantFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CreateTenantDatabaseAsync(int firmaId)
    {
        var prefix = _configuration.GetValue<string>("TenantDatabase:NamingPrefix") ?? "kofa_firma_";
        var databaseName = $"{prefix}{firmaId:D3}";

        if (await TenantDatabaseExistsAsync(databaseName))
        {
            _logger.LogInformation("Tenant DB zaten mevcut: {DbName}", databaseName);
        }
        else
        {
            // PostgreSQL sunucusuna baglan, yeni DB olustur
            var masterConnStr = _connProvider.GetMasterConnectionString();
            var masterBuilder = new NpgsqlConnectionStringBuilder(masterConnStr);
            var serverConnStr = new NpgsqlConnectionStringBuilder(masterConnStr)
            {
                Database = "postgres"
            }.ConnectionString;

            await using var serverConn = new NpgsqlConnection(serverConnStr);
            await serverConn.OpenAsync();
            await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", serverConn);
            await createCmd.ExecuteNonQueryAsync();
            _logger.LogInformation("Tenant DB olusturuldu: {DbName}", databaseName);
        }

        // Tenant DB'ye EF Core migration'larini uygula
        var tenantConnStr = _connProvider.GetConnectionStringForFirma(firmaId, databaseName);
        if (tenantConnStr == null)
            throw new InvalidOperationException($"Tenant connection string alinamadi: FirmaId={firmaId}");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(tenantConnStr);
        await using var context = new ApplicationDbContext(optionsBuilder.Options);

        await context.Database.MigrateAsync();
        _logger.LogInformation("Tenant DB migration uygulandi: {DbName}", databaseName);

        // Master DB'de Firma.DatabaseName guncelle
        await using var masterCtx = await _masterFactory.CreateDbContextAsync();
        var firma = await masterCtx.Firmalar.FindAsync(firmaId);
        if (firma != null)
        {
            firma.DatabaseName = databaseName;
            await masterCtx.SaveChangesAsync();
            _logger.LogInformation("Firma {FirmaId} DatabaseName = {DbName}", firmaId, databaseName);
        }
    }

    public async Task<bool> TenantDatabaseExistsAsync(string databaseName)
    {
        var masterConnStr = _connProvider.GetMasterConnectionString();
        var serverConnStr = new NpgsqlConnectionStringBuilder(masterConnStr)
        {
            Database = "postgres"
        }.ConnectionString;

        await using var conn = new NpgsqlConnection(serverConnStr);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @name", conn);
        cmd.Parameters.AddWithValue("@name", databaseName);
        return await cmd.ExecuteScalarAsync() != null;
    }
}
