using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public interface ITasimaTedarikciService
{
    // Tedarikçi CRUD
    Task<List<TasimaTedarikci>> GetAllAsync(bool sadeceAktif = false);
    Task<TasimaTedarikci?> GetAsync(int id);
    Task<TasimaTedarikci> CreateAsync(TasimaTedarikci tedarikci);
    Task<TasimaTedarikci> UpdateAsync(TasimaTedarikci tedarikci);
    Task<TasimaTedarikci> CreateFromCariAsync(int cariId, bool updateIfExists = true);
    Task<TasimaTedarikci> CreateFromPersonelAsync(int personelId, bool updateIfExists = true);
    Task DeleteAsync(int id);
    Task<string> GenerateTedarikciKoduAsync();

    // İş (Tedarikçi-Güzergah eşleşmesi) CRUD
    Task<List<TasimaTedarikciIs>> GetIslerAsync(int? tedarikciId = null);
    Task<TasimaTedarikciIs?> GetIsAsync(int id);
    Task<TasimaTedarikciIs> CreateIsAsync(TasimaTedarikciIs tedarikciIs);
    Task<TasimaTedarikciIs> UpdateIsAsync(TasimaTedarikciIs tedarikciIs);
    Task DeleteIsAsync(int id);

    // Tedarikçiye bağlı personel/araç (mevcut Sofor/Arac kayıtları üzerinden)
    Task<List<Sofor>> GetTedarikciPersonelleriAsync(int tedarikciId);
    Task<List<Arac>> GetTedarikciAraclariAsync(int tedarikciId);

    // Tedarikçi firma evrak CRUD
    Task<List<TedarikciEvrak>> GetTedarikciEvraklariAsync(int tedarikciId);
    Task<TedarikciEvrak> CreateTedarikciEvrakAsync(TedarikciEvrak evrak);
    Task<TedarikciEvrak> UpdateTedarikciEvrakAsync(TedarikciEvrak evrak);
    Task DeleteTedarikciEvrakAsync(int evrakId);

    // Tedarikçi evrak dosya işlemleri
    Task<TedarikciEvrakDosya> UploadTedarikciEvrakDosyaAsync(int evrakId, IBrowserFile file);
    Task<byte[]> GetTedarikciEvrakDosyaAsync(int dosyaId);
    Task DeleteTedarikciEvrakDosyaAsync(int dosyaId);
}

public class TasimaTedarikciService : ITasimaTedarikciService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ISecureFileService _secureFileService;

    public TasimaTedarikciService(IDbContextFactory<ApplicationDbContext> contextFactory, ISecureFileService secureFileService)
    {
        _contextFactory = contextFactory;
        _secureFileService = secureFileService;
    }

    public async Task<List<TasimaTedarikci>> GetAllAsync(bool sadeceAktif = false)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.TasimaTedarikciler
            .Include(t => t.Cari)
            .AsQueryable();

        if (sadeceAktif)
            query = query.Where(t => t.Aktif);

        return await query.OrderBy(t => t.Unvan).ToListAsync();
    }

    public async Task<TasimaTedarikci?> GetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TasimaTedarikciler
            .Include(t => t.Cari)
            .Include(t => t.Personeller)
            .Include(t => t.Araclar)
            .Include(t => t.Isler)
                .ThenInclude(i => i.Guzergah)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TasimaTedarikci> CreateAsync(TasimaTedarikci tedarikci)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (string.IsNullOrWhiteSpace(tedarikci.TedarikciKodu))
            tedarikci.TedarikciKodu = await GenerateTedarikciKoduAsync();

        tedarikci.CreatedAt = DateTime.UtcNow;
        context.TasimaTedarikciler.Add(tedarikci);
        await context.SaveChangesAsync();
        return tedarikci;
    }

    public async Task<TasimaTedarikci> UpdateAsync(TasimaTedarikci tedarikci)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        tedarikci.UpdatedAt = DateTime.UtcNow;
        context.TasimaTedarikciler.Update(tedarikci);
        await context.SaveChangesAsync();
        return tedarikci;
    }

    public async Task<TasimaTedarikci> CreateFromCariAsync(int cariId, bool updateIfExists = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var cari = await context.Cariler
            .FirstOrDefaultAsync(c => c.Id == cariId && !c.IsDeleted);

        if (cari is null)
            throw new InvalidOperationException("Kopyalanacak cari bulunamadı.");

        var existing = await context.TasimaTedarikciler
            .FirstOrDefaultAsync(t => t.CariId == cariId && !t.IsDeleted);

        if (existing is not null)
        {
            if (updateIfExists)
            {
                MapFromCari(existing, cari);
                existing.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return existing;
        }

        var yeniTedarikci = new TasimaTedarikci
        {
            TedarikciKodu = await GenerateTedarikciKoduAsync(),
            Aktif = cari.Aktif,
            CariId = cari.Id,
            CreatedAt = DateTime.UtcNow
        };

        MapFromCari(yeniTedarikci, cari);
        context.TasimaTedarikciler.Add(yeniTedarikci);
        await context.SaveChangesAsync();

        return yeniTedarikci;
    }

    public async Task<TasimaTedarikci> CreateFromPersonelAsync(int personelId, bool updateIfExists = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var personel = await context.Soforler
            .FirstOrDefaultAsync(s => s.Id == personelId && !s.IsDeleted);

        if (personel is null)
            throw new InvalidOperationException("Kopyalanacak personel bulunamadı.");

        var existing = await context.TasimaTedarikciler
            .FirstOrDefaultAsync(t => t.Notlar != null && t.Notlar.Contains($"PersonelId:{personelId}") && !t.IsDeleted);

        if (existing is not null)
        {
            if (updateIfExists)
            {
                MapFromPersonel(existing, personel);
                existing.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
            return existing;
        }

        var yeniTedarikci = new TasimaTedarikci
        {
            TedarikciKodu = await GenerateTedarikciKoduAsync(),
            Aktif = personel.Aktif,
            CreatedAt = DateTime.UtcNow
        };
        MapFromPersonel(yeniTedarikci, personel);
        yeniTedarikci.Notlar = (yeniTedarikci.Notlar ?? "") + $" [PersonelId:{personelId}]";
        context.TasimaTedarikciler.Add(yeniTedarikci);
        await context.SaveChangesAsync();
        return yeniTedarikci;
    }

    private static void MapFromPersonel(TasimaTedarikci hedef, Sofor kaynak)
    {
        hedef.Unvan = $"{kaynak.Ad} {kaynak.Soyad}".Trim();
        hedef.YetkiliKisi = $"{kaynak.Ad} {kaynak.Soyad}".Trim();
        hedef.Telefon = kaynak.Telefon;
        hedef.Email = kaynak.Email;
        hedef.Adres = kaynak.Adres;
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.TasimaTedarikciler.FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<string> GenerateTedarikciKoduAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sayi = await context.TasimaTedarikciler.IgnoreQueryFilters().CountAsync();
        return $"TT{(sayi + 1):D4}";
    }

    public async Task<List<TasimaTedarikciIs>> GetIslerAsync(int? tedarikciId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.TasimaTedarikciIsler
            .Include(i => i.TasimaTedarikci)
            .Include(i => i.Guzergah)
            .Include(i => i.Arac)
            .Include(i => i.Sofor)
            .AsQueryable();

        if (tedarikciId.HasValue)
            query = query.Where(i => i.TasimaTedarikciId == tedarikciId.Value);

        return await query.OrderByDescending(i => i.BaslangicTarihi).ToListAsync();
    }

    public async Task<TasimaTedarikciIs?> GetIsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TasimaTedarikciIsler
            .Include(i => i.TasimaTedarikci)
            .Include(i => i.Guzergah)
            .Include(i => i.Arac)
            .Include(i => i.Sofor)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<TasimaTedarikciIs> CreateIsAsync(TasimaTedarikciIs tedarikciIs)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        tedarikciIs.CreatedAt = DateTime.UtcNow;
        context.TasimaTedarikciIsler.Add(tedarikciIs);
        await context.SaveChangesAsync();
        return tedarikciIs;
    }

    public async Task<TasimaTedarikciIs> UpdateIsAsync(TasimaTedarikciIs tedarikciIs)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        tedarikciIs.UpdatedAt = DateTime.UtcNow;
        context.TasimaTedarikciIsler.Update(tedarikciIs);
        await context.SaveChangesAsync();
        return tedarikciIs;
    }

    public async Task DeleteIsAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.TasimaTedarikciIsler.FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<List<Sofor>> GetTedarikciPersonelleriAsync(int tedarikciId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Soforler
            .Where(s => s.TasimaTedarikciId == tedarikciId)
            .OrderBy(s => s.Ad).ThenBy(s => s.Soyad)
            .ToListAsync();
    }

    public async Task<List<Arac>> GetTedarikciAraclariAsync(int tedarikciId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Araclar
            .Where(a => a.TasimaTedarikciId == tedarikciId)
            .OrderBy(a => a.AktifPlaka)
            .ToListAsync();
    }

    private static void MapFromCari(TasimaTedarikci hedef, Cari kaynak)
    {
        hedef.Unvan = kaynak.Unvan;
        hedef.YetkiliKisi = kaynak.YetkiliKisi;
        hedef.Telefon = kaynak.Telefon;
        hedef.Telefon2 = kaynak.Telefon2;
        hedef.Email = kaynak.Email;
        hedef.Adres = kaynak.Adres;
        hedef.Il = kaynak.Il;
        hedef.Ilce = kaynak.Ilce;
        hedef.VergiDairesi = kaynak.VergiDairesi;
        hedef.VergiNo = kaynak.VergiNo;
        hedef.Notlar = kaynak.Notlar;
        hedef.SozlesmeNo = kaynak.SozlesmeNo;
        hedef.SozlesmeBaslangicTarihi = kaynak.SozlesmeBaslangicTarihi;
        hedef.SozlesmeBitisTarihi = kaynak.SozlesmeBitisTarihi;
    }
}
