using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

public interface IKurumService
{
    Task<List<Kurum>> GetAllAsync(bool includeDeleted = false);
    Task<List<Kurum>> GetAktifAsync();
    Task<Kurum?> GetByIdAsync(int id);
    Task<Kurum> CreateAsync(Kurum kurum);
    Task<Kurum> UpdateAsync(Kurum kurum);
    Task DeleteAsync(int id); // Soft delete
}
