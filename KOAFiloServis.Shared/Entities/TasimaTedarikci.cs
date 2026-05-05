namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Personel taşımacılığında dış tedarikçi (alt yüklenici) firma.
/// Tedarikçinin kendi personeli mevcut <see cref="Sofor"/>, kendi araçları
/// mevcut <see cref="Arac"/> kayıtları üzerinden takip edilir; bu modül
/// sadece tedarikçi şirketinin kimlik/sözleşme bilgisini ve iş eşleşmesini tutar.
/// </summary>
public class TasimaTedarikci : BaseEntity
{
    /// <summary>
    /// Multi-tenant: Şirket ID (null = sistem geneli)
    /// </summary>
    public int? SirketId { get; set; }
    public virtual Sirket? Sirket { get; set; }

    public string TedarikciKodu { get; set; } = string.Empty;
    public string Unvan { get; set; } = string.Empty;

    // İletişim
    public string? YetkiliKisi { get; set; }
    public string? Telefon { get; set; }
    public string? Telefon2 { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    public string? Il { get; set; }
    public string? Ilce { get; set; }

    // Vergi
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }

    // Finans / Cari bağlantısı (mali analiz ve fatura için tek kaynak)
    public int? CariId { get; set; }
    public virtual Cari? Cari { get; set; }

    // Sözleşme bilgileri
    public DateTime? SozlesmeBaslangicTarihi { get; set; }
    public DateTime? SozlesmeBitisTarihi { get; set; }
    public string? SozlesmeNo { get; set; }
    public decimal? VarsayilanSeferUcreti { get; set; }

    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }

    // Navigation - tedarikçinin personeli (Sofor.TasimaTedarikciId)
    public virtual ICollection<Sofor> Personeller { get; set; } = new List<Sofor>();

    // Navigation - tedarikçinin araçları (Arac.TasimaTedarikciId)
    public virtual ICollection<Arac> Araclar { get; set; } = new List<Arac>();

    // Navigation - tedarikçi iş atamaları (güzergah eşleşmeleri)
    public virtual ICollection<TasimaTedarikciIs> Isler { get; set; } = new List<TasimaTedarikciIs>();
}

/// <summary>
/// Tedarikçi - Güzergah - İş eşleşmesi.
/// Bir tedarikçinin hangi güzergahta hangi tarih aralığında, hangi araç/şoför ile
/// çalıştığını ve sözleşme ücretini tutar.
/// </summary>
public class TasimaTedarikciIs : BaseEntity
{
    public int? SirketId { get; set; }
    public virtual Sirket? Sirket { get; set; }

    public int TasimaTedarikciId { get; set; }
    public virtual TasimaTedarikci TasimaTedarikci { get; set; } = null!;

    public int GuzergahId { get; set; }
    public virtual Guzergah Guzergah { get; set; } = null!;

    // Tedarikçinin atadığı araç / şoför (opsiyonel - tek kaynak: Arac / Sofor tabloları)
    public int? AracId { get; set; }
    public virtual Arac? Arac { get; set; }

    public int? SoforId { get; set; }
    public virtual Sofor? Sofor { get; set; }

    public DateTime BaslangicTarihi { get; set; } = DateTime.Today;
    public DateTime? BitisTarihi { get; set; }

    public decimal? SeferUcreti { get; set; }
    public decimal? AylikUcret { get; set; }

    public TasimaTedarikciIsDurum Durum { get; set; } = TasimaTedarikciIsDurum.Aktif;
    public string? Aciklama { get; set; }
}

public enum TasimaTedarikciIsDurum
{
    Beklemede = 0,
    Aktif = 1,
    Tamamlandi = 2,
    IptalEdildi = 3
}
