using System.ComponentModel.DataAnnotations;

namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Operasyonel kapasite tanımı.
/// </summary>
[TenantNullableFirmaId]
public class Kapasite : BaseEntity, IFirmaTenant
{
    /// <summary>
    /// LEGACY — Eski multi-tenant Sirket kavramı. Yeni mimari `FirmaId` kullanır.
    /// </summary>
    [Obsolete("Tenant yeniden yapılandırması (Faz C-extend): SirketId yerine FirmaId kullanın.")]
    public int? SirketId { get; set; }
    [Obsolete("Tenant yeniden yapılandırması (Faz C-extend): Sirket navigasyonu yerine Firma kullanın.")]
    public virtual Sirket? Sirket { get; set; }

    /// <summary>
    /// Tenant: Bu kapasitenin ait olduğu firma. (K3+K4)
    /// </summary>
    public int? FirmaId { get; set; }
    public virtual Firma? Firma { get; set; }

    [Required]
    [StringLength(100)]
    public string KapasiteAdi { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Aciklama { get; set; }

    public decimal Carpan { get; set; } = 1m;
    public bool Aktif { get; set; } = true;
}
