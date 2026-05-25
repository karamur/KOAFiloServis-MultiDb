using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services.Interfaces;

public interface IPuantajFinansService
{
    // Finansal kayıt
    Task FinansalKayitOlusturAsync(int hesapDonemiId, CancellationToken ct = default);
    Task<List<PuantajFinansalKayit>> FinansalKayitlariGetirAsync(int hesapDonemiId, CancellationToken ct = default);

    // Fatura üretimi
    Task<Fatura> GelirFaturasiUretAsync(int finansalKayitId, CancellationToken ct = default);
    Task<Fatura> GiderFaturasiUretAsync(int finansalKayitId, CancellationToken ct = default);
    Task<int> TopluFaturaUretAsync(int hesapDonemiId, CancellationToken ct = default);

    // Durum
    Task<bool> FaturaUretilebilirMiAsync(int hesapDonemiId, CancellationToken ct = default);
    Task<bool> FinansalKayitOlusturulabilirMiAsync(int hesapDonemiId, CancellationToken ct = default);
}
