using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

/// <summary>
/// Aktif (seçili) firmayı per-user / per-circuit tutar.
/// <para>
/// Blazor Server'da <b>Scoped</b> olarak kaydedilir; her circuit (kullanıcı oturumu)
/// kendi aktif firma bilgisine sahip olur. <see cref="FirmaService"/> içindeki eski
/// <c>static</c> yaklaşımın aksine farklı kullanıcılar birbirinin firmasını <b>görmez</b>.
/// </para>
/// <para>
/// Aktif firma bilgisi ApplicationDbContext'in global query filter'ı ve SaveChanges
/// interceptor'u tarafından da okunur, bu sayede tenant izolasyonu otomatik sağlanır.
/// </para>
/// </summary>
public interface IAktifFirmaProvider
{
    /// <summary>
    /// Aktif firmanın Id'si. 0 veya null ise henüz firma seçilmemiştir.
    /// </summary>
    int? AktifFirmaId { get; }

    /// <summary>
    /// "Tüm firmalar" modu (SuperAdmin / yönetici için cross-tenant rapor).
    /// True iken global query filter devre dışı bırakılır.
    /// </summary>
    bool TumFirmalar { get; }

    /// <summary>
    /// Aktif firmanın tüm bilgisi (Id, kod, ad, dönem).
    /// </summary>
    AktifFirmaBilgisi Mevcut { get; }

    /// <summary>
    /// Aktif firmayı değiştirir. Login sonrası firma seçim ekranı veya üst bardaki
    /// firma değiştiriciden çağrılır.
    /// </summary>
    void Set(AktifFirmaBilgisi firma);

    /// <summary>
    /// "Tüm firmalar" modunu açar/kapatır.
    /// </summary>
    void SetTumFirmalar(bool tumFirmalar);

    /// <summary>
    /// Aktif dönem (yıl/ay) günceller. Firma kaydındaki dönem alanını da senkronlamak
    /// FirmaService.SetAktifDonem'in sorumluluğundadır.
    /// </summary>
    void SetDonem(int yil, int ay);

    /// <summary>
    /// Aktif firma değiştiğinde tetiklenir (UI yenileme, cache invalidation vb. için).
    /// </summary>
    event Action? AktifFirmaDegisti;
}

/// <summary>
/// <see cref="IAktifFirmaProvider"/> default implementasyonu.
/// <para>
/// Per-circuit in-memory state tutar. Kalıcılık (session/cookie) Aşama H'de eklenecek.
/// </para>
/// </summary>
public sealed class AktifFirmaProvider : IAktifFirmaProvider
{
    private AktifFirmaBilgisi _mevcut = new();

    public int? AktifFirmaId => _mevcut.FirmaId > 0 ? _mevcut.FirmaId : null;

    public bool TumFirmalar => _mevcut.TumFirmalar;

    public AktifFirmaBilgisi Mevcut => _mevcut;

    public event Action? AktifFirmaDegisti;

    public void Set(AktifFirmaBilgisi firma)
    {
        _mevcut = firma ?? new AktifFirmaBilgisi();
        AktifFirmaDegisti?.Invoke();
    }

    public void SetTumFirmalar(bool tumFirmalar)
    {
        _mevcut.TumFirmalar = tumFirmalar;
        AktifFirmaDegisti?.Invoke();
    }

    public void SetDonem(int yil, int ay)
    {
        _mevcut.AktifDonemYil = yil;
        _mevcut.AktifDonemAy = ay;
        AktifFirmaDegisti?.Invoke();
    }
}
