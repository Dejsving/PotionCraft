using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Interfaces;

namespace PotionCraft.Repository.Repositories;

/// <summary>
/// Реализация репозитория трав (ингридиентов) на Entity Framework Core
/// </summary>
public class HerbRepository : IHerbRepository
{
    private readonly PotionCraftDbContext _dbContext;

    public HerbRepository(PotionCraftDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Herb?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Herbs
            .AsNoTracking() // Ускоряем чтение
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Herb>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Herbs
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Herb herb, CancellationToken cancellationToken = default)
    {
        await _dbContext.Herbs.AddAsync(herb, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Herb herb, CancellationToken cancellationToken = default)
    {
        _dbContext.Herbs.Update(herb);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var herb = await _dbContext.Herbs.FindAsync([id], cancellationToken);
        if (herb != null)
        {
            _dbContext.Herbs.Remove(herb);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
