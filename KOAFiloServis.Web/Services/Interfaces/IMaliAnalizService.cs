using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Models;

namespace KOAFiloServis.Web.Services;

public interface IMaliAnalizService
{
    // Dashboard
    Task<MaliAnalizDashboard> GetDashboardAsync(int yil, int ay);

    // �zmal Ara� Raporu
    Task<OzmalAracRaporu> GetOzmalAracRaporuAsync(int yil, int ay);

    // Kiral�k Ara� Raporu
    Task<KiralikAracRaporu> GetKiralikAracRaporuAsync(int yil, int ay);

    // Komisyon Raporu
    Task<KomisyonRaporu> GetKomisyonRaporuAsync(int yil, int ay);

    // Taşıma Tedarikçisi (Alt Yüklenici) Raporu
    Task<TasimaTedarikciRaporu> GetTasimaTedarikciRaporuAsync(int yil, int ay);

    // Checklist
    Task<ChecklistOzet> GetChecklistOzetAsync(int yil, int ay);

    // Trend Analizi
    Task<List<GrafikVeri>> GetYillikTrendAsync(int yil);
}
