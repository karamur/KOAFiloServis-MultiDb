using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public sealed class HoldingService : IHoldingService
{
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly IDbContextFactory<HoldingDbContext> _holdingFactory;
    private readonly ITenantConnectionStringProvider _connProvider;
    private readonly ILogger<HoldingService> _logger;

    public HoldingService(
        IDbContextFactory<MasterDbContext> masterFactory,
        IDbContextFactory<HoldingDbContext> holdingFactory,
        ITenantConnectionStringProvider connProvider,
        ILogger<HoldingService> logger)
    {
        _masterFactory = masterFactory;
        _holdingFactory = holdingFactory;
        _connProvider = connProvider;
        _logger = logger;
    }

    public async Task ToplaVeKaydetAsync(int yil, int ay)
    {
        using var masterCtx = await _masterFactory.CreateDbContextAsync();
        var firmalar = await masterCtx.Firmalar
            .Where(f => f.Aktif && !f.IsDeleted && f.DatabaseName != null)
            .ToListAsync();

        if (firmalar.Count == 0)
        {
            _logger.LogInformation("ToplaVeKaydet: Tenant DB'si olan aktif firma bulunamadi.");
            return;
        }

        _logger.LogInformation("ToplaVeKaydet: {Count} firma icin {Yil}-{Ay} verisi toplaniyor...",
            firmalar.Count, yil, ay);

        var tasks = firmalar.Select(async firma =>
        {
            try
            {
                var connStr = _connProvider.GetConnectionStringForFirma(firma.Id, firma.DatabaseName);
                if (connStr == null) return null;

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseNpgsql(connStr).Options;
                using var tenantCtx = new ApplicationDbContext(options);

                var start = new DateTime(yil, ay, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);

                var gelir = await tenantCtx.Faturalar
                    .Where(f => !f.IsDeleted && f.FaturaTipi == FaturaTipi.SatisFaturasi
                        && f.FaturaTarihi >= start && f.FaturaTarihi < end)
                    .SumAsync(f => f.GenelToplam);

                var gider = await tenantCtx.Faturalar
                    .Where(f => !f.IsDeleted && f.FaturaTipi == FaturaTipi.AlisFaturasi
                        && f.FaturaTarihi >= start && f.FaturaTarihi < end)
                    .SumAsync(f => f.GenelToplam);

                var aracMaliyet = await tenantCtx.AracMasraflari
                    .Where(m => !m.IsDeleted && m.CreatedAt >= start && m.CreatedAt < end)
                    .SumAsync(m => m.Tutar);

                var personelMaliyet = await tenantCtx.PersonelMaaslari
                    .Where(p => !p.IsDeleted && p.CreatedAt >= start && p.CreatedAt < end)
                    .SumAsync(p => p.NetMaas);

                var hakedisToplam = await tenantCtx.Hakedisler
                    .Where(h => !h.IsDeleted && h.CreatedAt >= start && h.CreatedAt < end)
                    .SumAsync(h => h.GenelToplam);

                var aktifAracSayisi = await tenantCtx.Araclar
                    .CountAsync(a => !a.IsDeleted && a.Aktif);

                var personelSayisi = await tenantCtx.Soforler
                    .CountAsync(s => !s.IsDeleted && s.Aktif);

                return new HoldingVeri
                {
                    FirmaId = firma.Id,
                    FirmaKodu = firma.FirmaKodu,
                    FirmaAdi = firma.FirmaAdi,
                    Yil = yil,
                    Ay = ay,
                    Kategori = "KARZARAR",
                    ToplamGelir = gelir,
                    ToplamGider = gider,
                    Kar = gelir - gider,
                    AraclarMaliyet = aracMaliyet,
                    PersonelMaliyet = personelMaliyet,
                    HakedisToplam = hakedisToplam,
                    AktifAracSayisi = aktifAracSayisi,
                    PersonelSayisi = personelSayisi,
                    OlusturmaTarihi = DateTime.UtcNow,
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firma {FirmaId} ({FirmaAdi}) veri toplama hatasi.", firma.Id, firma.FirmaAdi);
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);

        using var holdingCtx = await _holdingFactory.CreateDbContextAsync();
        foreach (var veri in results.Where(v => v != null).Cast<HoldingVeri>())
        {
            var existing = await holdingCtx.HoldingVeriler
                .FirstOrDefaultAsync(v => v.FirmaId == veri.FirmaId
                    && v.Yil == veri.Yil && v.Ay == veri.Ay
                    && v.Kategori == veri.Kategori);

            if (existing != null)
            {
                existing.ToplamGelir = veri.ToplamGelir;
                existing.ToplamGider = veri.ToplamGider;
                existing.Kar = veri.Kar;
                existing.AraclarMaliyet = veri.AraclarMaliyet;
                existing.PersonelMaliyet = veri.PersonelMaliyet;
                existing.HakedisToplam = veri.HakedisToplam;
                existing.AktifAracSayisi = veri.AktifAracSayisi;
                existing.PersonelSayisi = veri.PersonelSayisi;
                existing.OlusturmaTarihi = DateTime.UtcNow;
            }
            else
            {
                holdingCtx.HoldingVeriler.Add(veri);
            }
        }
        await holdingCtx.SaveChangesAsync();

        _logger.LogInformation("ToplaVeKaydet: {Count} firma verisi kaydedildi.",
            results.Count(v => v != null));
    }

    public async Task<List<HoldingVeri>> GetFirmaKarsilastirmaAsync(int yil, int? ay = null)
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        var query = ctx.HoldingVeriler.Where(v => v.Yil == yil && v.Kategori == "KARZARAR");
        if (ay.HasValue)
            query = query.Where(v => v.Ay == ay.Value);
        return await query.OrderBy(v => v.FirmaId).ThenBy(v => v.Ay).ToListAsync();
    }

    public async Task<List<HoldingVeri>> GetButceKonsolidasyonAsync(int yil)
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        return await ctx.HoldingVeriler
            .Where(v => v.Yil == yil && v.Kategori == "BUTCE")
            .OrderBy(v => v.FirmaId).ThenBy(v => v.Ay)
            .ToListAsync();
    }

    public async Task<List<HoldingVeri>> GetAracMaliyetOzetiAsync(int yil, int? ay = null)
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        var query = ctx.HoldingVeriler.Where(v => v.Yil == yil && v.Kategori == "KARZARAR");
        if (ay.HasValue)
            query = query.Where(v => v.Ay == ay.Value);
        return await query.OrderBy(v => v.FirmaId).ToListAsync();
    }

    public async Task<List<HoldingVeri>> GetPersonelGiderOzetiAsync(int yil, int? ay = null)
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        var query = ctx.HoldingVeriler.Where(v => v.Yil == yil && v.Kategori == "KARZARAR");
        if (ay.HasValue)
            query = query.Where(v => v.Ay == ay.Value);
        return await query.OrderBy(v => v.FirmaId).ToListAsync();
    }

    public async Task<List<HoldingVeri>> GetHakedisOzetiAsync(int yil, int? ay = null)
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        var query = ctx.HoldingVeriler.Where(v => v.Yil == yil && v.Kategori == "KARZARAR");
        if (ay.HasValue)
            query = query.Where(v => v.Ay == ay.Value);
        return await query.OrderBy(v => v.FirmaId).ToListAsync();
    }

    public async Task<List<HoldingRapor>> GetKayitliRaporlarAsync()
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        return await ctx.HoldingRaporlar
            .OrderByDescending(r => r.OlusturmaTarihi)
            .Take(50)
            .ToListAsync();
    }

    public async Task<HoldingRapor> RaporKaydetAsync(HoldingRapor rapor)
    {
        using var ctx = await _holdingFactory.CreateDbContextAsync();
        if (rapor.Id == 0)
            ctx.HoldingRaporlar.Add(rapor);
        else
            ctx.HoldingRaporlar.Update(rapor);
        await ctx.SaveChangesAsync();
        return rapor;
    }
}
