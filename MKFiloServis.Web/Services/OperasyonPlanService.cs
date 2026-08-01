using Microsoft.EntityFrameworkCore;
using MKFiloServis.Shared.Entities;
using MKFiloServis.Web.Data;
using MKFiloServis.Web.Services.Interfaces;

namespace MKFiloServis.Web.Services;

/// <summary>
/// İzole operasyonel puantaj çekirdeği servisi.
/// Sadece yeni puantaj tablolarına yazar; araç/personel/muhasebe/bütçe modüllerine dokunmaz.
/// Mevcut FiloGuzergahEslestirme ve FiloGunlukPuantaj yalnızca okuma/teyit amaçlı kullanılır.
/// </summary>
public class OperasyonPlanService : IOperasyonPlanService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<OperasyonPlanService> _logger;

    public OperasyonPlanService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogger<OperasyonPlanService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<(int Olusan, int Atlanan)> PlanUretAsync(DateTime tarih)
    {
        var gun = tarih.Date;
        await using var db = await _dbFactory.CreateDbContextAsync();

        var takvim = await db.OperasyonTakvimGunleri
            .FirstOrDefaultAsync(t => t.Tarih == gun);
        var carpan = takvim?.PuantajCarpani ?? 1.0m;

        if (takvim?.GunTipi == OperasyonGunTipi.Tatil)
        {
            _logger.LogInformation("PLAN_URET: {Tarih} tatil günü, plan üretilmedi.", gun);
            return (0, 0);
        }

        var eslestirmeler = await db.Set<FiloGuzergahEslestirme>()
            .Where(e => e.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var mevcutEslestirmeIdleri = await db.OperasyonPlanSatirlari
            .Where(p => p.Tarih == gun)
            .Select(p => p.FiloGuzergahEslestirmeId)
            .ToListAsync();
        var mevcutSet = mevcutEslestirmeIdleri.ToHashSet();

        int olusan = 0, atlanan = 0;
        foreach (var e in eslestirmeler)
        {
            if (mevcutSet.Contains(e.Id)) { atlanan++; continue; }

            db.OperasyonPlanSatirlari.Add(new OperasyonPlanSatiri
            {
                Tarih = gun,
                FiloGuzergahEslestirmeId = e.Id,
                KurumFirmaId = e.KurumFirmaId,
                GuzergahId = e.GuzergahId,
                AracId = e.AracId,
                SoforId = e.SoforId,
                ServisTuru = e.ServisTuru,
                PlanlananSefer = 1m,
                PuantajCarpani = carpan,
                KurumSeferUcretiSnapshot = e.KurumaKesilecekUcret,
                TaseronSeferUcretiSnapshot = e.TaseronaOdenenUcret,
                Durum = OperasyonPlanDurumu.Planlandi
            });
            olusan++;
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("PLAN_URET: {Tarih} için {Olusan} plan oluştu, {Atlanan} atlandı.", gun, olusan, atlanan);
        return (olusan, atlanan);
    }

    public async Task<List<OperasyonPlanSatiri>> GetPlanlarAsync(DateTime tarih)
    {
        var gun = tarih.Date;
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.OperasyonPlanSatirlari
            .Where(p => p.Tarih == gun)
            .OrderBy(p => p.GuzergahId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<OperasyonTakvimGunu?> GetTakvimGunuAsync(DateTime tarih)
    {
        var gun = tarih.Date;
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.OperasyonTakvimGunleri
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Tarih == gun);
    }

    public async Task<int> PlanTeyitEtAsync(List<int> planIdleri)
    {
        if (planIdleri.Count == 0) return 0;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var planlar = await db.OperasyonPlanSatirlari
            .Where(p => planIdleri.Contains(p.Id) && p.Durum == OperasyonPlanDurumu.Planlandi)
            .ToListAsync();

        int teyit = 0;
        foreach (var plan in planlar)
        {
            var puantaj = new FiloGunlukPuantaj
            {
                Tarih = plan.Tarih,
                FiloGuzergahEslestirmeId = plan.FiloGuzergahEslestirmeId,
                KurumFirmaId = plan.KurumFirmaId,
                GuzergahId = plan.GuzergahId,
                AracId = plan.AracId,
                SoforId = plan.SoforId,
                Durum = OperasyonDurumu.Gitti,
                ServisTuru = plan.ServisTuru,
                SeferSayisi = plan.PlanlananSefer,
                PuantajCarpani = plan.PuantajCarpani,
                TahakkukEdenKurumUcreti = plan.PlanlananSefer * plan.PuantajCarpani * plan.KurumSeferUcretiSnapshot,
                TahakkukEdenTaseronUcreti = plan.PlanlananSefer * plan.PuantajCarpani * plan.TaseronSeferUcretiSnapshot,
                FirmaId = plan.FirmaId
            };
            db.Set<FiloGunlukPuantaj>().Add(puantaj);
            await db.SaveChangesAsync();

            plan.Durum = OperasyonPlanDurumu.TeyitEdildi;
            plan.FiloGunlukPuantajId = puantaj.Id;
            plan.TeyitTarihi = DateTime.UtcNow;
            teyit++;
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("PLAN_TEYIT: {Adet} plan teyit edilip günlük puantaja aktarıldı.", teyit);
        return teyit;
    }

    public async Task<List<OperasyonKontrat>> GetKontratlarAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.OperasyonKontratlar
            .Include(k => k.Fiyatlar)
            .AsNoTracking()
            .OrderByDescending(k => k.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task KontratKaydetAsync(OperasyonKontrat kontrat)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        if (kontrat.Id == 0) db.OperasyonKontratlar.Add(kontrat);
        else { kontrat.UpdatedAt = DateTime.UtcNow; db.OperasyonKontratlar.Update(kontrat); }
        await db.SaveChangesAsync();
    }

    public async Task TakvimGunuKaydetAsync(OperasyonTakvimGunu gun)
    {
        gun.Tarih = gun.Tarih.Date;
        await using var db = await _dbFactory.CreateDbContextAsync();
        var mevcut = await db.OperasyonTakvimGunleri.FirstOrDefaultAsync(t => t.Tarih == gun.Tarih);
        if (mevcut is null) db.OperasyonTakvimGunleri.Add(gun);
        else
        {
            mevcut.GunTipi = gun.GunTipi;
            mevcut.PuantajCarpani = gun.PuantajCarpani;
            mevcut.Aciklama = gun.Aciklama;
            mevcut.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }
}
