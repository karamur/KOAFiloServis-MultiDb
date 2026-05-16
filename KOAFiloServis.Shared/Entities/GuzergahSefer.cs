namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Bir güzergaha bağlı sefer detay satırı.
/// Güzergah listesinde "Sefer Detayları" panelinden girilen verileri kalıcı olarak saklar.
/// </summary>
public class GuzergahSefer : BaseEntity
{
    public int GuzergahId { get; set; }
    public virtual Guzergah? Guzergah { get; set; }

    /// <summary>Satır sırası (1. sefer, 2. sefer ...)</summary>
    public int Sira { get; set; }

    public SeferTipi SeferTipi { get; set; } = SeferTipi.SabahAksam;

    /// <summary>Kapasite tablosundan gelen ad ("16+1" gibi)</summary>
    public string? KapasiteAdi { get; set; }

    public int? AracId { get; set; }
    public virtual Arac? Arac { get; set; }

    public string? SoforAd { get; set; }
    public string? SoforTelefon { get; set; }

    /// <summary>Firma veya tedarikçi adı (serbest metin).</summary>
    public string? Firma { get; set; }
}
