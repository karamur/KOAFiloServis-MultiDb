using System.ComponentModel.DataAnnotations;

namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Operasyonel kapasite tanımı.
/// </summary>
public class Kapasite : BaseEntity
{
    public int? SirketId { get; set; }
    public virtual Sirket? Sirket { get; set; }

    [Required]
    [StringLength(100)]
    public string KapasiteAdi { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Aciklama { get; set; }

    public decimal Carpan { get; set; } = 1m;
    public bool Aktif { get; set; } = true;
}