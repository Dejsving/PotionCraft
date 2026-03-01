using PotionCraft.Contracts.Models;

namespace PotionCraft.Repository.Interfaces;

/// <summary>
/// Интерфейс для работы с базой данных трав и ингридиентов
/// </summary>
public interface IHerbRepository
{
    Task<Herb?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Herb>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Herb herb, CancellationToken cancellationToken = default);
    Task UpdateAsync(Herb herb, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
