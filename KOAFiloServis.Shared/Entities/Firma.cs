using System.ComponentModel.DataAnnotations;

namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Firma Bilgileri - Coklu firma destegi
/// </summary>
public class Firma : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string FirmaKodu { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string FirmaAdi { get; set; } = string.Empty;

    [StringLength(250)]
    public string? UnvanTam { get; set; }

    [StringLength(11)]
    public string? VergiNo { get; set; }

    [StringLength(100)]
    public string? VergiDairesi { get; set; }

    [StringLength(500)]
    public string? Adres { get; set; }

    [StringLength(100)]
    public string? Il { get; set; }

    [StringLength(100)]
    public string? Ilce { get; set; }

    [StringLength(20)]
    public string? Telefon { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? WebSite { get; set; }

    public string? Logo { get; set; } // Base64 veya dosya yolu

    public bool Aktif { get; set; } = true;
    public bool VarsayilanFirma { get; set; } = false;

    public int SiraNo { get; set; } = 0;

    /// <summary>
    /// Bu firma "kurum" rolünde de görünüyorsa (yani başka bir firmamız ona fatura kesiyorsa),
    /// muhasebe tarafında temsil ettiği Cari kaydı.
    /// Mutabakat / fatura eşleştirme bu ID üzerinden yapılır.
    /// </summary>
    public int? CariId { get; set; }

    // Muhasebe Donem Bilgisi
    public int AktifDonemYil { get; set; } = DateTime.Today.Year;
    public int AktifDonemAy { get; set; } = DateTime.Today.Month;
}

/// <summary>
/// Aktif firma bilgisini tutan servis
/// </summary>
public class AktifFirmaBilgisi
{
    public int FirmaId { get; set; }
    public string FirmaKodu { get; set; } = "";
    public string FirmaAdi { get; set; } = "";
    public int AktifDonemYil { get; set; } = DateTime.Today.Year;
    public int AktifDonemAy { get; set; } = DateTime.Today.Month;
    public bool TumFirmalar { get; set; } = false;
}
