using MKFiloServis.Shared.Entities;

namespace MKFiloServis.Web.Services.Interfaces;

public interface IOperasyonPlanService
{
    Task<(int Olusan, int Atlanan)> PlanUretAsync(DateTime tarih);
    Task<List<OperasyonPlanSatiri>> GetPlanlarAsync(DateTime tarih);
    Task<OperasyonTakvimGunu?> GetTakvimGunuAsync(DateTime tarih);
    Task<int> PlanTeyitEtAsync(List<int> planIdleri);
    Task<List<OperasyonKontrat>> GetKontratlarAsync();
    Task KontratKaydetAsync(OperasyonKontrat kontrat);
    Task TakvimGunuKaydetAsync(OperasyonTakvimGunu gun);
}
