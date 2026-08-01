using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MKFiloServis.Shared.Entities;

/// <summary>
/// İzole operasyonel puantaj çekirdeği: Kurum ile yapılan taşıma sözleşmesi başlığı.
/// Diğer modüllere yazmaz; yalnızca Id referansı tutar.
/// </summary>
public class OperasyonKontrat : FirmaBaseEntity
{
    [Required]
    public int KurumCariId { get; set; }

    [Required, MaxLength(150)]
    public string KontratAdi { get; set; } = string.Empty;

    public DateTime BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }

    public OperasyonKontratDurumu Durum { get; set; } = OperasyonKontratDurumu.Aktif;

    public string? Notlar { get; set; }

    public virtual ICollection<OperasyonKontratFiyat> Fiyatlar { get; set; } = new List<OperasyonKontratFiyat>();
}

/// <summary>
/// Kontrat kapsamında güzergah bazlı sefer fiyatı (kurum geliri / tedarikçi gideri).
/// </summary>
public class OperasyonKontratFiyat : FirmaBaseEntity
{
    [Required]
    public int OperasyonKontratId { get; set; }

    [Required]
    public int GuzergahId { get; set; }

    /// <summary>1 sefer için kuruma fatura edilecek tutar.</summary>
    public decimal KurumSeferUcreti { get; set; }

    /// <summary>1 sefer için taşerona ödenecek tutar (özmalda 0).</summary>
    public decimal TaseronSeferUcreti { get; set; }

    public DateTime GecerlilikBaslangic { get; set; }
    public DateTime? GecerlilikBitis { get; set; }

    [ForeignKey(nameof(OperasyonKontratId))]
    public virtual OperasyonKontrat? Kontrat { get; set; }
}

/// <summary>
/// Firma bazlı çalışma takvimi günü: tam gün / yarım gün / tatil ve puantaj çarpanı.
/// </summary>
public class OperasyonTakvimGunu : FirmaBaseEntity
{
    [Required]
    public DateTime Tarih { get; set; }

    public OperasyonGunTipi GunTipi { get; set; } = OperasyonGunTipi.TamGun;

    /// <summary>Örn: 1.0 tam gün, 0.5 yarım gün, 0 tatil.</summary>
    public decimal PuantajCarpani { get; set; } = 1.0m;

    public string? Aciklama { get; set; }
}

/// <summary>
/// Günlük plan satırı: eşleştirme şablonundan üretilen "bugün çalışması beklenen" kayıt.
/// Teyit edildiğinde FiloGunlukPuantaj'a dönüştürülür (mevcut akışa dokunmadan).
/// </summary>
public class OperasyonPlanSatiri : FirmaBaseEntity
{
    [Required]
    public DateTime Tarih { get; set; }

    [Required]
    public int FiloGuzergahEslestirmeId { get; set; }

    // Snapshot alanları (plan üretildiği andaki değerler)
    public int KurumFirmaId { get; set; }
    public int GuzergahId { get; set; }
    public int AracId { get; set; }
    public int SoforId { get; set; }
    public ServisTuru ServisTuru { get; set; } = ServisTuru.SabahAksam;
    public decimal PlanlananSefer { get; set; } = 1m;
    public decimal PuantajCarpani { get; set; } = 1.0m;
    public decimal KurumSeferUcretiSnapshot { get; set; }
    public decimal TaseronSeferUcretiSnapshot { get; set; }

    public OperasyonPlanDurumu Durum { get; set; } = OperasyonPlanDurumu.Planlandi;

    /// <summary>Teyit sonrası oluşan FiloGunlukPuantaj kaydına bağlantı.</summary>
    public int? FiloGunlukPuantajId { get; set; }

    public DateTime? TeyitTarihi { get; set; }
    public string? Notlar { get; set; }
}

public enum OperasyonKontratDurumu
{
    Taslak = 0,
    Aktif = 1,
    Kapali = 2
}

public enum OperasyonGunTipi
{
    TamGun = 1,
    YarimGun = 2,
    Tatil = 3
}

public enum OperasyonPlanDurumu
{
    Planlandi = 1,
    TeyitEdildi = 2,
    IptalEdildi = 3,
    EksikGiris = 4
}
