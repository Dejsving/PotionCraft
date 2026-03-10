using PotionCraft.Contracts.Models;

namespace PotionCraft.Repository.Abstraction
{
    public interface IHerbRepository
    {
        Task<Herb?> GetByIdAsync(Guid id);
        Task<List<Herb>> GetAllAsync();
        Task AddAsync(Herb herb);
        Task UpdateAsync(Herb herb);
        Task DeleteAsync(Guid id);
    }
}