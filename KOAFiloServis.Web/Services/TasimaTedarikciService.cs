using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public interface ITasimaTedarikciService
{
    // Tedarikçi CRUD
    Task<List<TasimaTedarikci>> GetAllAsync(bool sadeceAktif = false);
    Task<TasimaTedarikci?> GetAsync(int id);
    Task<TasimaTedarikci> CreateAsync(TasimaTedarikci tedarikci);
    Task<TasimaTedarikci> UpdateAsync(TasimaTedarikci tedarikci);
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
}

public class TasimaTedarikciService : ITasimaTedarikciService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public TasimaTedarikciService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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
}
