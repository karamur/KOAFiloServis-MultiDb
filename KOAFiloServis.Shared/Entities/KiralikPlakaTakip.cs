using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOAFiloServis.Shared.Entities;

public class KiralikPlakaTakip : BaseEntity
{
    public int? AracId { get; set; }
    public virtual Arac? Arac { get; set; }

    [Required, StringLength(15)]
    public string Plaka { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string IsimSoyisim { get; set; } = string.Empty;

    public DateTime BaslamaTarihi { get; set; } = DateTime.Today;
    public DateTime BitisTarihi { get; set; } = DateTime.Today.AddYears(1);

    [StringLength(50)]
    public string Durum { get; set; } = "ÖNÜ AÇIK";

    [StringLength(50)]
    public string KasaDurumu { get; set; } = "PLAKA";

    public decimal FaturaOdemesi { get; set; } = 0;
    
    [StringLength(20)]
    public string Periyot { get; set; } = "AYLIK";

    public decimal AylikVeyaYillikTutar { get; set; } = 0;

    public decimal EkTutar { get; set; } = 0;

    [NotMapped]
    public decimal Toplam => FaturaOdemesi + EkTutar;
}

