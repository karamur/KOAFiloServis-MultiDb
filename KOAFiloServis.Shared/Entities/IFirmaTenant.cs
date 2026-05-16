namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Firma (tenant) bazlı izolasyon yapılan entity'ler için marker interface.
/// <para>
/// Bu interface'i implemente eden entity'ler:
/// </para>
/// <list type="bullet">
///   <item>ApplicationDbContext üzerinde global query filter ile otomatik <c>FirmaId == aktifFirma</c> filtresine tabi tutulur.</item>
///   <item>SaveChanges sırasında yeni eklenen kayıtlara aktif <c>FirmaId</c> otomatik atanır.</item>
///   <item>Şirketler arası kopyalama servisinin hedefi olur.</item>
/// </list>
/// <para>
/// <b>Önemli:</b> Bütçe ve Muhasebe modüllerinin entity'leri bu interface'i implemente <b>etmez</b>;
/// onlar tenant filtresinden muaftır (kullanıcı isteği).
/// </para>
/// </summary>
public interface IFirmaTenant
{
    /// <summary>
    /// Bu kaydın ait olduğu Firma (tenant) Id'si.
    /// </summary>
    int FirmaId { get; set; }
}
