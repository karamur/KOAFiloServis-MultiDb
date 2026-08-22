using MKFiloServis.Shared.Entities;
using MKFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace MKFiloServis.Web.Services;

/// <summary>
/// Firma değişikliğinde entity ve bağlı verilerin taşınması/kopyalanması.
/// </summary>
public class FirmaTransferService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly Interfaces.ICacheService _cache;

    public FirmaTransferService(IDbContextFactory<ApplicationDbContext> dbFactory, Interfaces.ICacheService cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    // ── Güzergah Transfer ──────────────────────────────────────────────────

    public async Task<List<TransferableItem>> GetGuzergahTransferItemsAsync(int guzergahId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var items = new List<TransferableItem>();

        var seferSayisi = await db.GuzergahSeferleri.CountAsync(s => s.GuzergahId == guzergahId);
        if (seferSayisi > 0)
            items.Add(new TransferableItem { Key = "GuzergahSefer", Label = $"Seferler ({seferSayisi} adet)", Count = seferSayisi, Selected = true });

        var puantajSayisi = await db.PuantajKayitlar.CountAsync(p => p.GuzergahId == guzergahId && !p.IsDeleted);
        if (puantajSayisi > 0)
            items.Add(new TransferableItem { Key = "PuantajKayit", Label = $"Puantaj Kayıtları ({puantajSayisi} adet)", Count = puantajSayisi, Selected = true });

        return items;
    }

    public async Task<(int moved, List<string> errors)> MoveGuzergahToFirmaAsync(int guzergahId, int targetFirmaId, List<string> selectedKeys)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        int moved = 0;
        var errors = new List<string>();

        if (selectedKeys.Contains("GuzergahSefer"))
        {
            try
            {
                var seferler = await db.GuzergahSeferleri.Where(s => s.GuzergahId == guzergahId).ToListAsync();
                foreach (var s in seferler) s.FirmaId = targetFirmaId;
                moved += seferler.Count;
            }
            catch (Exception ex) { errors.Add($"Seferler: {ex.Message}"); }
        }

        if (selectedKeys.Contains("PuantajKayit"))
        {
            try
            {
                var puantajlar = await db.PuantajKayitlar.Where(p => p.GuzergahId == guzergahId && !p.IsDeleted).ToListAsync();
                foreach (var p in puantajlar) p.IsverenFirmaId = targetFirmaId;
                moved += puantajlar.Count;
            }
            catch (Exception ex) { errors.Add($"Puantaj: {ex.Message}"); }
        }

        var guzergah = await db.Guzergahlar.FindAsync(guzergahId);
        if (guzergah != null) guzergah.FirmaId = targetFirmaId;

        await db.SaveChangesAsync();
        return (moved, errors);
    }

    // ── Araç Transfer ──────────────────────────────────────────────────────

    public async Task<List<TransferableItem>> GetAracTransferItemsAsync(int aracId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var items = new List<TransferableItem>();

        // Id-bazli islem: arac baska firmada olabilir, tenant filtresi atlanir.
        var evrakSayisi = await db.AracEvraklari.IgnoreQueryFilters().CountAsync(e => e.AracId == aracId && !e.IsDeleted);
        if (evrakSayisi > 0)
            items.Add(new TransferableItem { Key = "AracEvrak", Label = $"Evraklar ({evrakSayisi} adet)", Count = evrakSayisi, Selected = true });

        var plakaSayisi = await db.AracPlakalar.IgnoreQueryFilters().CountAsync(p => p.AracId == aracId && !p.IsDeleted);
        if (plakaSayisi > 0)
            items.Add(new TransferableItem { Key = "AracPlaka", Label = $"Plaka Geçmişi ({plakaSayisi} adet) - bilgi amaçlı", Count = plakaSayisi, Selected = false });

        var puantajSayisi = await db.PuantajKayitlar.IgnoreQueryFilters().CountAsync(p => p.AracId == aracId && !p.IsDeleted);
        if (puantajSayisi > 0)
            items.Add(new TransferableItem { Key = "PuantajKayit", Label = $"Puantaj Kayıtları ({puantajSayisi} adet)", Count = puantajSayisi, Selected = true });

        var servisSayisi = await db.ServisCalismalari.IgnoreQueryFilters().CountAsync(s => s.AracId == aracId && !s.IsDeleted);
        if (servisSayisi > 0)
            items.Add(new TransferableItem { Key = "ServisCalisma", Label = $"Bakım/Onarım ({servisSayisi} adet)", Count = servisSayisi, Selected = true });

        var plakaTakipSayisi = await db.KiralikPlakaTakipler.IgnoreQueryFilters().CountAsync(k => k.AracId == aracId && !k.IsDeleted);
        if (plakaTakipSayisi > 0)
            items.Add(new TransferableItem { Key = "KiralikPlakaTakip", Label = $"Plaka Takip ({plakaTakipSayisi} adet)", Count = plakaTakipSayisi, Selected = false });

        return items;
    }

    public async Task<(int moved, List<string> errors)> MoveAracToFirmaAsync(int aracId, int targetFirmaId, List<string> selectedKeys)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        int moved = 0;
        var errors = new List<string>();

        if (selectedKeys.Contains("AracEvrak"))
        {
            try
            {
                var evraklar = await db.AracEvraklari.IgnoreQueryFilters().Where(e => e.AracId == aracId && !e.IsDeleted).ToListAsync();
                moved += evraklar.Count;
                // AracEvrak'ta FirmaId yok, evraklar araca bagli olarak kalir
            }
            catch (Exception ex) { errors.Add($"Evraklar: {ex.Message}"); }
        }

        if (selectedKeys.Contains("PuantajKayit"))
        {
            try
            {
                var puantajlar = await db.PuantajKayitlar.IgnoreQueryFilters().Where(p => p.AracId == aracId && !p.IsDeleted).ToListAsync();
                foreach (var p in puantajlar) p.IsverenFirmaId = targetFirmaId;
                moved += puantajlar.Count;
            }
            catch (Exception ex) { errors.Add($"Puantaj: {ex.Message}"); }
        }

        if (selectedKeys.Contains("ServisCalisma"))
        {
            try
            {
                var servisler = await db.ServisCalismalari.IgnoreQueryFilters().Where(s => s.AracId == aracId && !s.IsDeleted).ToListAsync();
                foreach (var s in servisler) s.FirmaId = targetFirmaId;
                moved += servisler.Count;
            }
            catch (Exception ex) { errors.Add($"Bakım/Onarım: {ex.Message}"); }
        }

        if (selectedKeys.Contains("KiralikPlakaTakip"))
        {
            try
            {
                var takipler = await db.KiralikPlakaTakipler.IgnoreQueryFilters().Where(k => k.AracId == aracId && !k.IsDeleted).ToListAsync();
                foreach (var t in takipler) { /* KiralikPlakaTakip'te FirmaId yok, skip */ }
            }
            catch (Exception ex) { errors.Add($"Plaka Takip: {ex.Message}"); }
        }

        // FindAsync tenant filtresine takilir: arac daha once baska firmaya tasindiysa
        // null doner ve firma degisikligi sessizce yapilmaz. IgnoreQueryFilters sart.
        var arac = await db.Araclar.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == aracId && !a.IsDeleted);
        if (arac != null)
        {
            // Tasima izi: hangi firmadan tasindigi gorunsun
            arac.KaynakFirmaId = arac.FirmaId;
            arac.KaynakKayitId = arac.Id;
            arac.FirmaId = targetFirmaId;
            arac.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            errors.Add("Araç kaydı bulunamadı; firma değişikliği uygulanamadı.");
        }

        await db.SaveChangesAsync();

        // Arac listeleri firma bazli cache'leniyor; tasima sonrasi eski firmada
        // gorunmemesi icin tum arac cache'leri temizlenir.
        await _cache.RemoveByPrefixAsync(Interfaces.CacheKeys.AracPrefix);

        return (moved, errors);
    }
}

public class TransferableItem
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public int Count { get; set; }
    public bool Selected { get; set; }
}


