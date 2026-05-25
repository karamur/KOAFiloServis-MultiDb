using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public sealed class PuantajFinansService : IPuantajFinansService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IFaturaService _faturaService;

    public PuantajFinansService(IDbContextFactory<ApplicationDbContext> dbFactory, IFaturaService faturaService)
    {
        _dbFactory = dbFactory;
        _faturaService = faturaService;
    }

    public async Task<bool> FinansalKayitOlusturulabilirMiAsync(int hesapDonemiId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var h = await db.PuantajHesapDonemleri.FindAsync(hesapDonemiId);
        return h is { Durum: PuantajHesapDurum.Aktif, OnayDurum: PuantajDonemOnayDurum.Kilitli };
    }

    public async Task FinansalKayitOlusturAsync(int hesapDonemiId, CancellationToken ct = default)
    {
        if (!await FinansalKayitOlusturulabilirMiAsync(hesapDonemiId, ct))
            throw new InvalidOperationException("Finansal kayıt için hesap dönemi Kilitli olmalıdır.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var mevcut = await db.PuantajFinansalKayitlar
            .Where(f => f.HesapDonemiId == hesapDonemiId && !f.IsDeleted)
            .Select(f => f.PuantajKayitId)
            .ToListAsync(ct);

        var pkList = await db.PuantajKayitlar
            .Where(p => p.HesapDonemiId == hesapDonemiId && !p.IsDeleted && !mevcut.Contains(p.Id))
            .ToListAsync(ct);

        var simdi = DateTime.UtcNow;
        foreach (var pk in pkList)
        {
            db.PuantajFinansalKayitlar.Add(new PuantajFinansalKayit
            {
                FirmaId = null,
                PuantajKayitId = pk.Id,
                HesapDonemiId = hesapDonemiId,
                BirimGelir = pk.BirimGelir,
                BirimGider = pk.BirimGider,
                ToplamGelir = pk.ToplamGelir,
                ToplamGider = pk.ToplamGider,
                KdvTutar = pk.GelirKdvTutari,
                GenelToplam = pk.Alinacak,
                SeferGunu = (int)pk.Gun,
                GelirCariId = pk.FaturaKesiciCariId ?? pk.KurumCariId,
                GiderCariId = pk.OdemeYapilacakCariId,
                KayitTarihi = simdi,
                CreatedAt = simdi
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<List<PuantajFinansalKayit>> FinansalKayitlariGetirAsync(int hesapDonemiId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PuantajFinansalKayitlar
            .Include(f => f.PuantajKayit).ThenInclude(p => p!.Guzergah)
            .Include(f => f.PuantajKayit).ThenInclude(p => p!.Arac)
            .Where(f => f.HesapDonemiId == hesapDonemiId && !f.IsDeleted)
            .OrderBy(f => f.PuantajKayit!.Guzergah!.GuzergahAdi)
            .AsNoTracking().ToListAsync(ct);
    }

    public async Task<bool> FaturaUretilebilirMiAsync(int hesapDonemiId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var kayitVar = await db.PuantajFinansalKayitlar
            .AnyAsync(f => f.HesapDonemiId == hesapDonemiId && !f.IsDeleted, ct);
        return kayitVar;
    }

    public async Task<Fatura> GelirFaturasiUretAsync(int finansalKayitId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var fk = await db.PuantajFinansalKayitlar
            .Include(f => f.PuantajKayit).ThenInclude(p => p!.Guzergah)
            .Include(f => f.PuantajKayit).ThenInclude(p => p!.Arac)
            .FirstOrDefaultAsync(f => f.Id == finansalKayitId && !f.IsDeleted, ct)
            ?? throw new InvalidOperationException("Finansal kayıt bulunamadı.");

        if (fk.GelirCariId == null)
            throw new InvalidOperationException("Gelir CariId tanımlı değil.");
        if (fk.GelirFaturaId != null)
            throw new InvalidOperationException("Bu kayıt için gelir faturası zaten üretilmiş.");

        var pk = fk.PuantajKayit!;
        var faturaNo = await _faturaService.GenerateNextFaturaNoAsync(FaturaTipi.SatisFaturasi, FaturaYonu.Giden);

        var fatura = new Fatura
        {
            FaturaNo = faturaNo,
            FaturaTarihi = DateTime.Today,
            FaturaTipi = FaturaTipi.SatisFaturasi,
            FaturaYonu = FaturaYonu.Giden,
            Durum = FaturaDurum.Beklemede,
            EFaturaTipi = EFaturaTipi.EArsiv,
            CariId = fk.GelirCariId.Value,
            AraToplam = fk.ToplamGelir,
            KdvOrani = pk.GelirKdvOrani,
            KdvTutar = fk.KdvTutar,
            GenelToplam = fk.GenelToplam,
            ImportKaynak = "Puantaj",
            Aciklama = $"{pk.Yil}/{pk.Ay:D2} {pk.GuzergahAdi} / {pk.Arac?.AktifPlaka ?? pk.Arac?.Plaka} Puantaj Gelir Faturası",
            CreatedAt = DateTime.UtcNow
        };

        fatura = await _faturaService.CreateAsync(fatura);

        // FaturaKalem ekle
        await using var db2 = await _dbFactory.CreateDbContextAsync();
        db2.FaturaKalemleri.Add(new FaturaKalem
        {
            FaturaId = fatura.Id,
            SiraNo = 1,
            Aciklama = $"{pk.GuzergahAdi} / {(pk.Arac?.AktifPlaka ?? pk.Arac?.Plaka)} / {pk.Slot}",
            Miktar = fk.SeferGunu,
            Birim = "Sefer Günü",
            BirimFiyat = fk.BirimGelir,
            KdvOrani = pk.GelirKdvOrani,
            KdvTutar = fk.KdvTutar,
            ToplamTutar = fk.GenelToplam,
            KalemTipi = FaturaKalemTipi.Servis,
            CreatedAt = DateTime.UtcNow
        });
        await db2.SaveChangesAsync(ct);

        // Finansal kaydı güncelle
        fk.GelirFaturaId = fatura.Id;
        fk.Durum = fk.GiderFaturaId != null ? PuantajFinansalDurum.TumFaturalarUretildi : PuantajFinansalDurum.GelirFaturasiUretildi;
        fk.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return fatura;
    }

    public async Task<Fatura> GiderFaturasiUretAsync(int finansalKayitId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var fk = await db.PuantajFinansalKayitlar
            .Include(f => f.PuantajKayit).ThenInclude(p => p!.Guzergah)
            .Include(f => f.PuantajKayit).ThenInclude(p => p!.Arac)
            .FirstOrDefaultAsync(f => f.Id == finansalKayitId && !f.IsDeleted, ct)
            ?? throw new InvalidOperationException("Finansal kayıt bulunamadı.");

        if (fk.GiderCariId == null)
            throw new InvalidOperationException("Gider CariId tanımlı değil.");
        if (fk.GiderFaturaId != null)
            throw new InvalidOperationException("Bu kayıt için gider faturası zaten üretilmiş.");

        var pk = fk.PuantajKayit!;
        var faturaNo = await _faturaService.GenerateNextFaturaNoAsync(FaturaTipi.AlisFaturasi, FaturaYonu.Gelen);

        var fatura = new Fatura
        {
            FaturaNo = faturaNo,
            FaturaTarihi = DateTime.Today,
            FaturaTipi = FaturaTipi.AlisFaturasi,
            FaturaYonu = FaturaYonu.Gelen,
            Durum = FaturaDurum.Beklemede,
            EFaturaTipi = EFaturaTipi.EArsiv,
            CariId = fk.GiderCariId.Value,
            AraToplam = fk.ToplamGider,
            KdvOrani = pk.GiderKdvOrani20 + pk.GiderKdvOrani10,
            KdvTutar = pk.GiderKdv20Tutari + pk.GiderKdv10Tutari,
            GenelToplam = fk.ToplamGider + pk.GiderKdv20Tutari + pk.GiderKdv10Tutari,
            ImportKaynak = "Puantaj",
            Aciklama = $"{pk.Yil}/{pk.Ay:D2} {pk.GuzergahAdi} / {pk.Arac?.AktifPlaka ?? pk.Arac?.Plaka} Puantaj Gider Faturası",
            CreatedAt = DateTime.UtcNow
        };

        fatura = await _faturaService.CreateAsync(fatura);

        await using var db2 = await _dbFactory.CreateDbContextAsync();
        db2.FaturaKalemleri.Add(new FaturaKalem
        {
            FaturaId = fatura.Id,
            SiraNo = 1,
            Aciklama = $"Tedarikçi Ödemesi: {pk.GuzergahAdi} / {(pk.Arac?.AktifPlaka ?? pk.Arac?.Plaka)}",
            Miktar = fk.SeferGunu,
            Birim = "Sefer Günü",
            BirimFiyat = fk.BirimGider,
            KdvOrani = pk.GiderKdvOrani20 + pk.GiderKdvOrani10,
            KdvTutar = pk.GiderKdv20Tutari + pk.GiderKdv10Tutari,
            ToplamTutar = fk.ToplamGider,
            KalemTipi = FaturaKalemTipi.Servis,
            CreatedAt = DateTime.UtcNow
        });
        await db2.SaveChangesAsync(ct);

        fk.GiderFaturaId = fatura.Id;
        fk.Durum = fk.GelirFaturaId != null ? PuantajFinansalDurum.TumFaturalarUretildi : PuantajFinansalDurum.GiderFaturasiUretildi;
        fk.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return fatura;
    }

    public async Task<int> TopluFaturaUretAsync(int hesapDonemiId, CancellationToken ct = default)
    {
        var kayitlar = await FinansalKayitlariGetirAsync(hesapDonemiId, ct);
        int uretilen = 0;

        foreach (var fk in kayitlar.Where(f => f.Durum == PuantajFinansalDurum.Bekliyor))
        {
            try
            {
                if (fk.GelirCariId != null && fk.GelirFaturaId == null)
                    await GelirFaturasiUretAsync(fk.Id, ct);
                if (fk.GiderCariId != null && fk.GiderFaturaId == null)
                    await GiderFaturasiUretAsync(fk.Id, ct);
                uretilen++;
            }
            catch { /* Tekil hata, devam et */ }
        }

        return uretilen;
    }
}
