using System.IO.Compression;
using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class BelgeUyariService : IBelgeUyariService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IPersonelOzlukService _ozlukService;
    private readonly ISecureFileService _secureFileService;

    public BelgeUyariService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IPersonelOzlukService ozlukService,
        ISecureFileService secureFileService)
    {
        _contextFactory = contextFactory;
        _ozlukService = ozlukService;
        _secureFileService = secureFileService;
    }

    public async Task<BelgeUyariOzet> GetBelgeUyarilarAsync(int yaklasanGunSayisi = 30)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var ozet = new BelgeUyariOzet();
        var bugun = DateTime.Today;
        var limitTarih = bugun.AddDays(yaklasanGunSayisi);

        // Aktif tüm personeli al
        var soforler = await context.Soforler
            .Where(s => s.Aktif && !s.IsDeleted)
            .ToListAsync();

        // Tüm personel özlük evraklarını tek sorguda al (GecerlilikBitisTarihi olan ve yaklaşan/geçmiş)
        var tumOzlukEvraklar = await context.PersonelOzlukEvraklar
            .AsNoTracking()
            .Include(e => e.Sofor)
                .ThenInclude(s => s!.TasimaTedarikci)
            .Include(e => e.EvrakTanim)
            .Where(e => !e.IsDeleted
                && e.Sofor != null && e.Sofor.Aktif && !e.Sofor.IsDeleted
                && e.GecerlilikBitisTarihi.HasValue
                && e.GecerlilikBitisTarihi.Value <= limitTarih)
            .OrderBy(e => e.GecerlilikBitisTarihi)
            .ToListAsync();

        // Özlük evraklarından GecerlilikBitisTarihi olan tüm uyarıları kategoriye göre dağıt
        // Sofor entity alanlarına sahip olanlar için özlük evrak kaydı varsa onu kullan, yoksa fallback
        var soforIdEhliyetEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        e.EvrakTanim.EvrakAdi.Contains("Ehliyet", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.SoforId).ToHashSet();
        var soforIdSrcEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        e.EvrakTanim.EvrakAdi.Contains("SRC", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.SoforId).ToHashSet();
        var soforIdPsikoteknikEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        e.EvrakTanim.EvrakAdi.Contains("Psikoteknik", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.SoforId).ToHashSet();
        var soforIdSaglikEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        (e.EvrakTanim.EvrakAdi.Contains("Sağlık", StringComparison.OrdinalIgnoreCase) ||
                         e.EvrakTanim.EvrakAdi.Contains("Saglik", StringComparison.OrdinalIgnoreCase)))
            .Select(e => e.SoforId).ToHashSet();

        // Özlük evraklarını uyarı listelerine dağıt
        foreach (var evrak in tumOzlukEvraklar)
        {
            var evrakAdi = evrak.EvrakTanim?.EvrakAdi ?? string.Empty;
            var uyari = new BelgeUyari
            {
                Id = evrak.Id,
                Kaynak = "Personel",
                Baslik = evrak.Sofor?.TamAd ?? "Personel",
                BelgeTuru = evrakAdi,
                BitisTarihi = evrak.GecerlilikBitisTarihi!.Value,
                DetayUrl = $"/personel/{evrak.SoforId}",
                TasimaTedarikciId = evrak.Sofor?.TasimaTedarikciId,
                TasimaTedarikciUnvan = evrak.Sofor?.TasimaTedarikci?.Unvan
            };

            if (evrakAdi.Contains("Ehliyet", StringComparison.OrdinalIgnoreCase))
                ozet.EhliyetUyarilari.Add(uyari);
            else if (evrakAdi.Contains("SRC", StringComparison.OrdinalIgnoreCase))
                ozet.SrcUyarilari.Add(uyari);
            else if (evrakAdi.Contains("Psikoteknik", StringComparison.OrdinalIgnoreCase))
                ozet.PsikoteknikUyarilari.Add(uyari);
            else if (evrakAdi.Contains("Sağlık", StringComparison.OrdinalIgnoreCase) ||
                     evrakAdi.Contains("Saglik", StringComparison.OrdinalIgnoreCase))
                ozet.SaglikRaporuUyarilari.Add(uyari);
            else
                ozet.DigerPersonelEvrakUyarilari.Add(uyari);
        }

        // TEKİL KAYNAK: Araç uyarıları yalnızca AracEvrak (Filo > Araçlar > Evraklar) tablosundan gelir.
        var tumAracEvraklari = await context.AracEvraklari
            .AsNoTracking()
            .Include(x => x.Arac)
                .ThenInclude(a => a!.TasimaTedarikci)
            .Where(x => !x.IsDeleted
                && x.Arac != null
                && !x.Arac.IsDeleted
                && x.Arac.Aktif
                && x.Durum != EvrakDurum.Pasif
                && x.BitisTarihi.HasValue
                && x.BitisTarihi.Value <= limitTarih)
            .OrderBy(x => x.BitisTarihi)
            .ToListAsync();

        // AracEvrak tablosundan gelen tüm evrakleri kategoriye göre dağıt
        foreach (var evrak in tumAracEvraklari)
        {
            var baslik = evrak.Arac?.AktifPlaka ?? evrak.Arac?.SaseNo ?? "Araç";
            var belgeTuru = string.IsNullOrWhiteSpace(evrak.EvrakAdi) ? evrak.EvrakKategorisi : evrak.EvrakAdi!;
            var detayUrl = $"/araclar/{evrak.AracId}/evraklar";

            BelgeUyari uyari = new()
            {
                Id = evrak.Id,
                Kaynak = "Araç",
                Baslik = baslik,
                BelgeTuru = belgeTuru,
                BitisTarihi = evrak.BitisTarihi!.Value,
                DetayUrl = detayUrl,
                TasimaTedarikciId = evrak.Arac?.TasimaTedarikciId,
                TasimaTedarikciUnvan = evrak.Arac?.TasimaTedarikci?.Unvan
            };

            if (evrak.EvrakKategorisi == EvrakKategorileri.Muayene)
                ozet.MuayeneUyarilari.Add(uyari);
            else if (evrak.EvrakKategorisi == EvrakKategorileri.Kasko)
                ozet.KaskoUyarilari.Add(uyari);
            else if (evrak.EvrakKategorisi == EvrakKategorileri.TrafikSigortasi)
                ozet.TrafikSigortasiUyarilari.Add(uyari);
            else
                ozet.DigerAracEvrakUyarilari.Add(uyari);
        }


        // Tum personeller icin "Diger" kategorisindeki evrak durumlarini cek (uyari filtresi yok - tam liste)
        var digerEvrakTanimlari = await context.OzlukEvrakTanimlari
            .AsNoTracking()
            .Where(t => t.Aktif && t.Kategori == OzlukEvrakKategori.Diger)
            .OrderBy(t => t.SiraNo)
            .ThenBy(t => t.EvrakAdi)
            .ToListAsync();

        if (digerEvrakTanimlari.Count > 0)
        {
            var digerEvrakTanimIds = digerEvrakTanimlari.Select(t => t.Id).ToHashSet();

            var mevcutDigerEvraklar = await context.PersonelOzlukEvraklar
                .AsNoTracking()
                .Include(e => e.Sofor)
                .Where(e => !e.IsDeleted
                    && e.Sofor != null && e.Sofor.Aktif && !e.Sofor.IsDeleted
                    && digerEvrakTanimIds.Contains(e.EvrakTanimId))
                .ToListAsync();

            foreach (var sofor in soforler.OrderBy(s => s.Ad).ThenBy(s => s.Soyad))
            {
                foreach (var tanim in digerEvrakTanimlari)
                {
                    var kayit = mevcutDigerEvraklar
                        .FirstOrDefault(e => e.SoforId == sofor.Id && e.EvrakTanimId == tanim.Id);

                    ozet.DigerTumPersonelBelgeler.Add(new PersonelBelgeDetay
                    {
                        EvrakId = kayit?.Id ?? 0,
                        SoforId = sofor.Id,
                        PersonelAdi = sofor.TamAd,
                        PersonelKodu = sofor.SoforKodu ?? sofor.Id.ToString(),
                        EvrakAdi = tanim.EvrakAdi,
                        Kategori = tanim.Kategori,
                        Tamamlandi = kayit?.Tamamlandi ?? false,
                        TamamlanmaTarihi = kayit?.TamamlanmaTarihi,
                        GecerlilikBitisTarihi = kayit?.GecerlilikBitisTarihi,
                        Zorunlu = tanim.Zorunlu,
                        DosyaYolu = kayit?.DosyaYolu,
                        DetayUrl = $"/personel/ozluk-evrak"
                    });
                }
            }
        }
        // Özet sayıları hesapla
        ozet.ToplamKritikUyari = ozet.TumUyarilar.Count(u => u.Seviye == BelgeUyariSeviye.Kritik || u.Seviye == BelgeUyariSeviye.Acil);
        ozet.ToplamUyari = ozet.TumUyarilar.Count;

        return ozet;
    }

    public async Task<List<PersonelBelgeTabloKalemi>> GetPersonelBelgeTablosuAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Tüm aktif personelleri çek
        var soforler = await context.Soforler
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.SiralamaNo == 0 ? int.MaxValue : s.SiralamaNo)
            .ThenBy(s => s.Ad)
            .ToListAsync();

        if (!soforler.Any()) return new List<PersonelBelgeTabloKalemi>();

        var soforIdler = soforler.Select(s => s.Id).ToList();

        // 2. Tüm aktif evrak tanımlarını tek sorguda çek
        var tumTanimlar = await context.OzlukEvrakTanimlari
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.Aktif)
            .OrderBy(t => t.Kategori)
            .ThenBy(t => t.SiraNo)
            .ToListAsync();

        // 3. Tüm personellerin evraklarını tek sorguda çek
        var tumEvraklar = await context.PersonelOzlukEvraklar
            .AsNoTracking()
            .Where(e => soforIdler.Contains(e.SoforId) && !e.IsDeleted)
            .ToListAsync();

        // Grup: soforId → evraklar
        var evraklarByPersonel = tumEvraklar.GroupBy(e => e.SoforId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<PersonelBelgeTabloKalemi>();
        foreach (var s in soforler)
        {
            var gorevStr = ((int)s.Gorev).ToString();
            var gecerliTanimlar = tumTanimlar
                .Where(t => string.IsNullOrEmpty(t.GecerliGorevler)
                    || t.GecerliGorevler.Split(',').Contains(gorevStr))
                .ToList();

            var personelEvraklar = evraklarByPersonel.TryGetValue(s.Id, out var pe) ? pe : new();

            var dosyalar = gecerliTanimlar.Select(tanim =>
            {
                var evrak = personelEvraklar.FirstOrDefault(e => e.EvrakTanimId == tanim.Id);
                return new OzlukEvrakDosyaBilgisi
                {
                    EvrakTanimId = tanim.Id,
                    EvrakAdi = tanim.EvrakAdi,
                    DosyaYolu = evrak?.DosyaYolu,
                    DosyaAdi = BuildIndirmeDosyaAdi(tanim.EvrakAdi, evrak?.DosyaYolu)
                };
            }).ToList();

            result.Add(new PersonelBelgeTabloKalemi
            {
                SoforId = s.Id,
                PersonelAdi = s.TamAd,
                PersonelKodu = s.SoforKodu,
                Gorev = s.Gorev.ToString(),
                Aktif = s.Aktif,
                ToplamEvrakSayisi = gecerliTanimlar.Count,
                YuklenmisEvrakSayisi = personelEvraklar.Count(e =>
                    gecerliTanimlar.Any(t => t.Id == e.EvrakTanimId) && !string.IsNullOrEmpty(e.DosyaYolu)),
                EvrakDosyalari = dosyalar,
                EhliyetGecerlilik = s.EhliyetGecerlilikTarihi,
                KimlikGecerlilik = s.KimlikGecerlilikTarihi,
                SrcGecerlilik = s.SrcBelgesiGecerlilikTarihi,
                PsikoteknikGecerlilik = s.PsikoteknikGecerlilikTarihi,
                AdliSicilGecerlilik = s.AdliSicilGecerlilikTarihi,
                SaglikRaporuGecerlilik = s.SaglikRaporuGecerlilikTarihi,
                SuruculCezaBarkodGecerlilik = s.SuruculCezaBarkodluBelgeTarihi
            });
        }
        return result;
    }

    public async Task<PersonelBelgeTabloKalemi?> GetTekPersonelBelgeAsync(int soforId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var s = await context.Soforler
            .AsNoTracking()
            .Where(x => x.Id == soforId && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (s == null) return null;

        var evrakDurum = await _ozlukService.GetPersonelEvrakDurumuAsync(s.Id);
        var dosyalar = evrakDurum.Evraklar
            .Select(e => new OzlukEvrakDosyaBilgisi
            {
                EvrakTanimId = e.EvrakTanimId,
                EvrakAdi = e.EvrakAdi,
                DosyaYolu = e.DosyaYolu,
                DosyaAdi = BuildIndirmeDosyaAdi(e.EvrakAdi, e.DosyaYolu)
            }).ToList();

        return new PersonelBelgeTabloKalemi
        {
            SoforId = s.Id,
            PersonelAdi = s.TamAd,
            PersonelKodu = s.SoforKodu,
            Gorev = s.Gorev.ToString(),
            Aktif = s.Aktif,
            ToplamEvrakSayisi = evrakDurum.ToplamEvrak,
            YuklenmisEvrakSayisi = evrakDurum.TamamlananEvrak,
            EvrakDosyalari = dosyalar,
            EhliyetGecerlilik = s.EhliyetGecerlilikTarihi,
            KimlikGecerlilik = s.KimlikGecerlilikTarihi,
            SrcGecerlilik = s.SrcBelgesiGecerlilikTarihi,
            PsikoteknikGecerlilik = s.PsikoteknikGecerlilikTarihi,
            AdliSicilGecerlilik = s.AdliSicilGecerlilikTarihi,
            SaglikRaporuGecerlilik = s.SaglikRaporuGecerlilikTarihi,
            SuruculCezaBarkodGecerlilik = s.SuruculCezaBarkodluBelgeTarihi
        };
    }

    public async Task<bool> PersonelBelgeTarihGuncelleAsync(int soforId, string belgeAlani, DateTime? tarih)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sofor = await context.Soforler.FindAsync(soforId);
        if (sofor == null) return false;

        switch (belgeAlani)
        {
            case "Ehliyet": sofor.EhliyetGecerlilikTarihi = tarih; break;
            case "Kimlik": sofor.KimlikGecerlilikTarihi = tarih; break;
            case "Src": sofor.SrcBelgesiGecerlilikTarihi = tarih; break;
            case "Psikoteknik": sofor.PsikoteknikGecerlilikTarihi = tarih; break;
            case "AdliSicil": sofor.AdliSicilGecerlilikTarihi = tarih; break;
            case "SaglikRaporu": sofor.SaglikRaporuGecerlilikTarihi = tarih; break;
            case "SuruculCezaBarkod": sofor.SuruculCezaBarkodluBelgeTarihi = tarih; break;
            default: return false;
        }

        sofor.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> PersonelBelgePdfAsync(int soforId)
    {
        return await _ozlukService.ExportPersonelDosyaPdfAsync(soforId);
    }

    public async Task<byte[]> SeciliPersonelBelgelerZipAsync(List<int> soforIdler, List<string>? seciliDosyaYollari = null)
    {
        using var zipMs = new MemoryStream();
        using (var archive = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var soforId in soforIdler)
            {
                var evrakDurum = await _ozlukService.GetPersonelEvrakDurumuAsync(soforId);
                var personelKlasoru = string.Join("_",
                    (evrakDurum.PersonelAdi?.Length > 0 ? evrakDurum.PersonelAdi : evrakDurum.PersonelKodu ?? soforId.ToString())
                    .Split(Path.GetInvalidFileNameChars()));

                // Seçili dosya yolu filtresi varsa uygula
                var evraklar = evrakDurum.Evraklar
                    .Where(e => !string.IsNullOrEmpty(e.DosyaYolu))
                    .Where(e => seciliDosyaYollari == null || seciliDosyaYollari.Contains(e.DosyaYolu!))
                    .ToList();

                foreach (var evrak in evraklar)
                {
                    var icerik = await _secureFileService.ReadDecryptedAsync(evrak.DosyaYolu);
                    if (icerik == null || icerik.Length == 0) continue;

                    var uzanti = GetGercekUzanti(evrak.DosyaYolu);
                    var guvenliEvrakAd = string.Join("_", evrak.EvrakAdi.Split(Path.GetInvalidFileNameChars()));
                    var zipYolu = soforIdler.Count > 1
                        ? $"{personelKlasoru}/{guvenliEvrakAd}{uzanti}"
                        : $"{guvenliEvrakAd}{uzanti}";

                    var entry = archive.CreateEntry(zipYolu, CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    await entryStream.WriteAsync(icerik);
                }
            }
        }
        zipMs.Position = 0;
        return zipMs.ToArray();
    }

    /// <summary>
    /// Saklanan dosya yolundaki '.enc' uzantısını kaldırır, gerçek (ör. .pdf, .jpg) uzantıyı döndürür.
    /// </summary>
    private static string GetGercekUzanti(string? dosyaYolu)
    {
        if (string.IsNullOrWhiteSpace(dosyaYolu)) return string.Empty;
        var ad = Path.GetFileName(dosyaYolu);
        if (ad.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
            ad = ad.Substring(0, ad.Length - 4);
        return Path.GetExtension(ad);
    }

    /// <summary>
    /// İndirme için kullanıcı dostu, .enc içermeyen dosya adı üretir.
    /// </summary>
    private static string? BuildIndirmeDosyaAdi(string evrakAdi, string? dosyaYolu)
    {
        if (string.IsNullOrWhiteSpace(dosyaYolu)) return null;
        var uzanti = GetGercekUzanti(dosyaYolu);
        var guvenliAd = string.Join("_", (evrakAdi ?? "belge").Split(Path.GetInvalidFileNameChars()));
        return string.IsNullOrEmpty(uzanti) ? guvenliAd : $"{guvenliAd}{uzanti}";
    }

    // ─────────────────────────────────────────────────────────────────
    // Araç Belge Tablosu
    // ─────────────────────────────────────────────────────────────────

    // Sütun anahtarı → AracEvrak.EvrakKategorisi eşlemesi
    private static string KategoriEslestir(string belgeAlani) => belgeAlani switch
    {
        "Ruhsat" => EvrakKategorileri.Ruhsat,
        "Sigorta" => EvrakKategorileri.TrafikSigortasi,
        "Muayene" => EvrakKategorileri.Muayene,
        "Uygunluk" => EvrakKategorileri.UygunlukBelgesi,
        "KoltukSigortasi" => EvrakKategorileri.KoltukSigortasi,
        "Kasko" => EvrakKategorileri.Kasko,
        _ => belgeAlani
    };

    public async Task<List<AracBelgeTabloKalemi>> GetAracBelgeTablosuAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var araclar = await context.Araclar
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.AktifPlaka ?? a.SaseNo)
            .ToListAsync();

        if (!araclar.Any()) return new List<AracBelgeTabloKalemi>();

        var aracIdler = araclar.Select(a => a.Id).ToList();

        var tumEvraklar = await context.AracEvraklari
            .AsNoTracking()
            .Include(e => e.Dosyalar.Where(d => !d.IsDeleted))
            .Where(e => aracIdler.Contains(e.AracId) && !e.IsDeleted && e.Durum != EvrakDurum.Pasif)
            .ToListAsync();

        var evraklarByArac = tumEvraklar.GroupBy(e => e.AracId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var sutunlar = new[] { "Ruhsat", "Sigorta", "Muayene", "Uygunluk", "KoltukSigortasi", "Kasko" };
        var result = new List<AracBelgeTabloKalemi>();

        foreach (var a in araclar)
        {
            var aracEvraklari = evraklarByArac.TryGetValue(a.Id, out var ae) ? ae : new();

            // Her sütun için en güncel (en geç bitiş tarihli) evrak kaydını bul
            AracEvrak? Bul(string alan)
            {
                var kategori = KategoriEslestir(alan);
                return aracEvraklari
                    .Where(e => string.Equals(e.EvrakKategorisi, kategori, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.BitisTarihi ?? DateTime.MinValue)
                    .FirstOrDefault();
            }

            var ruhsatEv = Bul("Ruhsat");
            var sigortaEv = Bul("Sigorta");
            var muayeneEv = Bul("Muayene");
            var uygunlukEv = Bul("Uygunluk");
            var koltukEv = Bul("KoltukSigortasi");
            var kaskoEv = Bul("Kasko");

            var dosyalar = new List<AracEvrakDosyaBilgisi>();
            void DosyaEkle(AracEvrak? evrak, string varsayilanKategori)
            {
                if (evrak == null) return;
                var dosya = evrak.Dosyalar?.OrderByDescending(d => d.CreatedAt).FirstOrDefault(d => !d.IsDeleted);
                dosyalar.Add(new AracEvrakDosyaBilgisi
                {
                    AracEvrakId = evrak.Id,
                    EvrakKategorisi = string.IsNullOrEmpty(evrak.EvrakKategorisi) ? varsayilanKategori : evrak.EvrakKategorisi,
                    EvrakAdi = evrak.EvrakAdi,
                    DosyaYolu = dosya?.DosyaYolu,
                    DosyaAdi = BuildIndirmeDosyaAdi(evrak.EvrakAdi ?? evrak.EvrakKategorisi, dosya?.DosyaYolu)
                });
            }

            DosyaEkle(ruhsatEv, KategoriEslestir("Ruhsat"));
            DosyaEkle(sigortaEv, KategoriEslestir("Sigorta"));
            DosyaEkle(muayeneEv, KategoriEslestir("Muayene"));
            DosyaEkle(uygunlukEv, KategoriEslestir("Uygunluk"));
            DosyaEkle(koltukEv, KategoriEslestir("KoltukSigortasi"));
            DosyaEkle(kaskoEv, KategoriEslestir("Kasko"));

            // Sigorta tarihi öncelik: Arac entity > AracEvrak
            // Muayene/Kasko aynı şekilde fallback
            DateTime? sigortaTarihi = a.TrafikSigortaBitisTarihi ?? sigortaEv?.BitisTarihi;
            DateTime? muayeneTarihi = a.MuayeneBitisTarihi ?? muayeneEv?.BitisTarihi;
            DateTime? kaskoTarihi = a.KaskoBitisTarihi ?? kaskoEv?.BitisTarihi;
            DateTime? koltukTarihi = a.KoltukSigortasiBitisTarihi ?? koltukEv?.BitisTarihi;

            result.Add(new AracBelgeTabloKalemi
            {
                AracId = a.Id,
                Plaka = a.AktifPlaka ?? string.Empty,
                SaseNo = a.SaseNo,
                MarkaModel = $"{a.Marka} {a.Model}".Trim(),
                AracTipi = a.AracTipi,
                Aktif = a.Aktif,
                ToplamEvrakSayisi = sutunlar.Length,
                YuklenmisEvrakSayisi = dosyalar.Count(d => d.DosyaVar),
                EvrakDosyalari = dosyalar,
                RuhsatGecerlilik = ruhsatEv?.BitisTarihi,
                SigortaGecerlilik = sigortaTarihi,
                MuayeneGecerlilik = muayeneTarihi,
                UygunlukGecerlilik = uygunlukEv?.BitisTarihi,
                KoltukSigortasiGecerlilik = koltukTarihi,
                KaskoGecerlilik = kaskoTarihi
            });
        }
        return result;
    }

    public async Task<AracBelgeTabloKalemi?> GetTekAracBelgeAsync(int aracId)
    {
        var liste = await GetAracBelgeTablosuAsync();
        return liste.FirstOrDefault(x => x.AracId == aracId);
    }

    public async Task<bool> AracBelgeTarihGuncelleAsync(int aracId, string belgeAlani, DateTime? bitisTarihi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var arac = await context.Araclar.FindAsync(aracId);
        if (arac == null) return false;

        // Doğrudan Arac entity'de tutulan tarihler
        switch (belgeAlani)
        {
            case "Sigorta":
                arac.TrafikSigortaBitisTarihi = bitisTarihi.HasValue
                    ? DateTime.SpecifyKind(bitisTarihi.Value, DateTimeKind.Utc) : null;
                break;
            case "Muayene":
                arac.MuayeneBitisTarihi = bitisTarihi.HasValue
                    ? DateTime.SpecifyKind(bitisTarihi.Value, DateTimeKind.Utc) : null;
                break;
            case "Kasko":
                arac.KaskoBitisTarihi = bitisTarihi.HasValue
                    ? DateTime.SpecifyKind(bitisTarihi.Value, DateTimeKind.Utc) : null;
                break;
            case "KoltukSigortasi":
                arac.KoltukSigortasiBitisTarihi = bitisTarihi.HasValue
                    ? DateTime.SpecifyKind(bitisTarihi.Value, DateTimeKind.Utc) : null;
                break;
            case "Ruhsat":
            case "Uygunluk":
                // Bu tarihler Arac entity'de yok → AracEvrak üzerine kaydedilir/güncellenir
                break;
            default: return false;
        }

        // AracEvrak güncelle/ekle (her durumda iz olarak; uyarı sayfası için)
        var kategori = KategoriEslestir(belgeAlani);
        var evrak = await context.AracEvraklari
            .Where(e => e.AracId == aracId && !e.IsDeleted && e.EvrakKategorisi == kategori)
            .OrderByDescending(e => e.BitisTarihi)
            .FirstOrDefaultAsync();

        if (evrak == null && bitisTarihi.HasValue)
        {
            evrak = new AracEvrak
            {
                AracId = aracId,
                EvrakKategorisi = kategori,
                EvrakAdi = kategori,
                Durum = EvrakDurum.Aktif,
                BitisTarihi = DateTime.SpecifyKind(bitisTarihi.Value, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };
            context.AracEvraklari.Add(evrak);
        }
        else if (evrak != null)
        {
            evrak.BitisTarihi = bitisTarihi.HasValue
                ? DateTime.SpecifyKind(bitisTarihi.Value, DateTimeKind.Utc) : null;
            evrak.UpdatedAt = DateTime.UtcNow;
        }

        arac.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AracBelgeDosyaYukleAsync(int aracId, string belgeAlani, string dosyaAdi, byte[] icerik)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var arac = await context.Araclar.FindAsync(aracId);
        if (arac == null) return false;

        var kategori = KategoriEslestir(belgeAlani);

        // Aynı kategoride aktif evrak var mı?
        var evrak = await context.AracEvraklari
            .Where(e => e.AracId == aracId && !e.IsDeleted && e.EvrakKategorisi == kategori)
            .OrderByDescending(e => e.BitisTarihi)
            .FirstOrDefaultAsync();

        if (evrak == null)
        {
            evrak = new AracEvrak
            {
                AracId = aracId,
                EvrakKategorisi = kategori,
                EvrakAdi = kategori,
                Durum = EvrakDurum.Aktif,
                CreatedAt = DateTime.UtcNow
            };
            context.AracEvraklari.Add(evrak);
            await context.SaveChangesAsync();
        }

        var storedPath = await _secureFileService.SaveEncryptedAsync(
            $"arac-evrak/{arac.Id}",
            dosyaAdi,
            icerik);

        var evrakDosya = new AracEvrakDosya
        {
            AracEvrakId = evrak.Id,
            DosyaAdi = dosyaAdi,
            DosyaYolu = storedPath,
            DosyaTipi = Path.GetExtension(dosyaAdi).TrimStart('.').ToLowerInvariant(),
            DosyaBoyutu = icerik.LongLength,
            CreatedAt = DateTime.UtcNow
        };
        context.AracEvrakDosyalari.Add(evrakDosya);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> SeciliAracBelgelerZipAsync(List<int> aracIdler, List<string>? seciliDosyaYollari = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        using var zipMs = new MemoryStream();
        using (var archive = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var aracId in aracIdler)
            {
                var arac = await context.Araclar.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == aracId);
                if (arac == null) continue;

                var aracKlasoru = string.Join("_",
                    (arac.AktifPlaka ?? arac.SaseNo ?? aracId.ToString())
                    .Split(Path.GetInvalidFileNameChars()));

                var evraklar = await context.AracEvraklari
                    .AsNoTracking()
                    .Include(e => e.Dosyalar.Where(d => !d.IsDeleted))
                    .Where(e => e.AracId == aracId && !e.IsDeleted)
                    .ToListAsync();

                foreach (var evrak in evraklar)
                {
                    foreach (var dosya in evrak.Dosyalar)
                    {
                        if (string.IsNullOrEmpty(dosya.DosyaYolu)) continue;
                        if (seciliDosyaYollari != null && !seciliDosyaYollari.Contains(dosya.DosyaYolu)) continue;

                        var icerik = await _secureFileService.ReadDecryptedAsync(dosya.DosyaYolu);
                        if (icerik == null || icerik.Length == 0) continue;

                        var uzanti = GetGercekUzanti(dosya.DosyaYolu);
                        var temelAd = !string.IsNullOrWhiteSpace(evrak.EvrakAdi) ? evrak.EvrakAdi : evrak.EvrakKategorisi;
                        var guvenliAd = string.Join("_", (temelAd ?? "belge").Split(Path.GetInvalidFileNameChars()));
                        var zipYolu = aracIdler.Count > 1
                            ? $"{aracKlasoru}/{guvenliAd}{uzanti}"
                            : $"{guvenliAd}{uzanti}";

                        // Aynı isimde dosya çakışmasın
                        var entryYolu = zipYolu;
                        var sayac = 1;
                        while (archive.GetEntry(entryYolu) != null)
                        {
                            entryYolu = aracIdler.Count > 1
                                ? $"{aracKlasoru}/{guvenliAd}_{sayac}{uzanti}"
                                : $"{guvenliAd}_{sayac}{uzanti}";
                            sayac++;
                        }

                        var entry = archive.CreateEntry(entryYolu, CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();
                        await entryStream.WriteAsync(icerik);
                    }
                }
            }
        }
        zipMs.Position = 0;
        return zipMs.ToArray();
    }
}


