using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

/// <summary>
/// Dry-run puantaj hesaplama motoru.
/// DB'ye KESİNLİKLE yazmaz — sadece Okuma yapar, sonucu memory'de üretir.
/// </summary>
public sealed class PreviewEngineService : IPreviewEngineService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public PreviewEngineService(IDbContextFactory<ApplicationDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task<PreviewResult> PreviewAsync(int yil, int ay, int? kurumId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var uyarilar = new List<string>();

        var baslangic = new DateTime(yil, ay, 1);
        var bitis = baslangic.AddMonths(1).AddDays(-1);

        // 1. Operasyonları yükle (read-only)
        var query = db.OperasyonKayitlari
            .Include(o => o.Guzergah)
            .Include(o => o.Arac)
            .Include(o => o.Sofor)
            .Where(o => !o.IsDeleted && o.Tarih >= baslangic && o.Tarih <= bitis)
            .AsNoTracking();

        if (kurumId.HasValue && kurumId.Value > 0)
        {
            var guzergahIds = await db.Guzergahlar
                .Where(g => !g.IsDeleted && g.KurumId == kurumId.Value)
                .Select(g => g.Id)
                .ToListAsync();
            query = query.Where(o => guzergahIds.Contains(o.GuzergahId));
        }

        var operasyonlar = await query.OrderBy(o => o.Tarih).ToListAsync();

        if (!operasyonlar.Any())
        {
            uyarilar.Add("Bu dönemde işlenecek operasyon kaydı bulunamadı.");
            return new PreviewResult { UyariMesajlari = uyarilar };
        }

        // 2. Önceki aktif hesap dönemini kontrol et
        var oncekiAktif = await db.PuantajHesapDonemleri
            .Where(h => !h.IsDeleted && h.Yil == yil && h.Ay == ay
                        && h.KurumId == kurumId && h.Durum == PuantajHesapDurum.Aktif)
            .OrderByDescending(h => h.Versiyon)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        int yeniVersiyon = (oncekiAktif?.Versiyon ?? 0) + 1;

        if (oncekiAktif != null)
            uyarilar.Add($"Bu dönem için Versiyon {oncekiAktif.Versiyon} aktif hesap mevcut. Yeni hesaplama revizyon olacak (V{yeniVersiyon}).");

        // 3. Fiyat referanslarını yükle (read-only)
        var guzergahIds2 = operasyonlar.Select(o => o.GuzergahId).Distinct().ToList();
        var guzergahlar = await db.Guzergahlar
            .Where(g => guzergahIds2.Contains(g.Id))
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id);

        var eslestirmeler = await db.FiloGuzergahEslestirmeleri
            .Where(e => guzergahIds2.Contains(e.GuzergahId) && e.IsActive)
            .AsNoTracking()
            .ToListAsync();

        // 4. Grupla ve hesapla (tamamen memory'de)
        var gruplar = operasyonlar
            .GroupBy(o => new { o.GuzergahId, o.AracId, o.Slot })
            .Select(g =>
            {
                var ilk = g.First();
                var toplamSefer = g.Where(o => o.OperasyonDurumu == OperasyonDurumu.Gitti)
                                   .Sum(o => (int)(o.SeferSayisi * o.PuantajCarpani));

                // Fiyat belirleme (snapshot)
                decimal birimGelir = 0, birimGider = 0;
                var eslestirme = eslestirmeler.FirstOrDefault(e => e.GuzergahId == g.Key.GuzergahId && e.AracId == g.Key.AracId);
                if (eslestirme != null)
                {
                    birimGelir = eslestirme.KurumaKesilecekUcret;
                    birimGider = eslestirme.TaseronaOdenenUcret;
                }
                else if (guzergahlar.TryGetValue(g.Key.GuzergahId, out var guz))
                {
                    birimGelir = guz.GelirFiyat;
                    birimGider = guz.GiderFiyat;
                }

                return new PreviewGrupDetay
                {
                    GuzergahId = g.Key.GuzergahId,
                    GuzergahAdi = ilk.Guzergah?.GuzergahAdi ?? "",
                    AracId = g.Key.AracId,
                    Plaka = ilk.Arac?.AktifPlaka ?? ilk.Arac?.Plaka ?? "",
                    SoforAdi = ilk.Sofor != null ? $"{ilk.Sofor.Ad} {ilk.Sofor.Soyad}" : null,
                    Slot = ilk.Slot.ToString(),
                    SeferGunuToplami = toplamSefer,
                    BirimGelir = birimGelir,
                    BirimGider = birimGider,
                    ToplamGelir = birimGelir * toplamSefer,
                    ToplamGider = birimGider * toplamSefer,
                    OperasyonSayisi = g.Count()
                };
            })
            .OrderBy(g => g.GuzergahAdi).ThenBy(g => g.Plaka).ThenBy(g => g.Slot)
            .ToList();

        // 5. Uyarı kontrolleri
        var sifirSeferli = gruplar.Where(g => g.SeferGunuToplami == 0).ToList();
        if (sifirSeferli.Any())
            uyarilar.Add($"{sifirSeferli.Count} grupta sefer günü toplamı 0 (tüm operasyonlar iptal/durmuş).");

        return new PreviewResult
        {
            OperasyonSayisi = operasyonlar.Count,
            GrupSayisi = gruplar.Count,
            UretilecekPuantajKayit = gruplar.Count,
            OncekiVersiyon = oncekiAktif?.Versiyon ?? 0,
            YeniVersiyon = yeniVersiyon,
            OncekiHesapDonemiId = oncekiAktif?.Id,
            OncekiHesaplayan = oncekiAktif?.HesaplayanKullanici,
            OncekiHesaplamaTarihi = oncekiAktif?.HesaplamaTarihi,
            ToplamGelir = gruplar.Sum(g => g.ToplamGelir),
            ToplamGider = gruplar.Sum(g => g.ToplamGider),
            ToplamSeferGunu = gruplar.Sum(g => g.SeferGunuToplami),
            OrtalamaBirimGelir = gruplar.Any(g => g.SeferGunuToplami > 0)
                ? gruplar.Where(g => g.SeferGunuToplami > 0).Average(g => g.BirimGelir)
                : 0,
            Gruplar = gruplar,
            UyariMesajlari = uyarilar
        };
    }
}
