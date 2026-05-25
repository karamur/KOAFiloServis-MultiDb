namespace KOAFiloServis.Web.Services.Interfaces;

/// <summary>
/// Dry-run puantaj hesaplama motoru. DB'ye yazmaz, sadece önizleme sonucu üretir.
/// </summary>
public interface IPreviewEngineService
{
    /// <summary>Dönem + kurum için kuru çalıştırma önizlemesi yapar.</summary>
    Task<PreviewResult> PreviewAsync(int yil, int ay, int? kurumId = null);
}

public sealed class PreviewResult
{
    /// <summary>Bu dönem için kaç operasyon kaydı var</summary>
    public int OperasyonSayisi { get; init; }

    /// <summary>Kaç farklı Guzergah+Arac+Slot grubu oluştu</summary>
    public int GrupSayisi { get; init; }

    /// <summary>Üretilecek PuantajKayit sayısı</summary>
    public int UretilecekPuantajKayit { get; init; }

    /// <summary>Önceki aktif hesap varsa onun versiyonu, yoksa 0</summary>
    public int OncekiVersiyon { get; init; }

    /// <summary>Bu hesap hangi versiyon olacak</summary>
    public int YeniVersiyon { get; init; }

    /// <summary>Önceki aktif hesap var mı (revizyon mu?)</summary>
    public bool RevizyonYapilacak => OncekiVersiyon > 0;
    public int? OncekiHesapDonemiId { get; init; }
    public string? OncekiHesaplayan { get; init; }
    public DateTime? OncekiHesaplamaTarihi { get; init; }

    // ── Finansal özet ────────────────────────────────────────────────────
    public decimal ToplamGelir { get; init; }
    public decimal ToplamGider { get; init; }
    public decimal NetKar => ToplamGelir - ToplamGider;
    public int ToplamSeferGunu { get; init; }
    public decimal OrtalamaBirimGelir { get; init; }

    // ── Grup detayları ───────────────────────────────────────────────────
    public List<PreviewGrupDetay> Gruplar { get; init; } = new();

    // ── Uygunluk ─────────────────────────────────────────────────────────
    public bool HesaplamaYapilabilir => OperasyonSayisi > 0;
    public bool AktifHesapVar => OncekiVersiyon > 0;
    public List<string> UyariMesajlari { get; init; } = new();
}

public sealed class PreviewGrupDetay
{
    public int GuzergahId { get; init; }
    public string GuzergahAdi { get; init; } = "";
    public int AracId { get; init; }
    public string Plaka { get; init; } = "";
    public string? SoforAdi { get; init; }
    public string Slot { get; init; } = "";
    public int SeferGunuToplami { get; init; }
    public decimal BirimGelir { get; init; }
    public decimal BirimGider { get; init; }
    public decimal ToplamGelir { get; init; }
    public decimal ToplamGider { get; init; }
    public int OperasyonSayisi { get; init; }
}
