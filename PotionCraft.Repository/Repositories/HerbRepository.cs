using Microsoft.EntityFrameworkCore;

using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Repository.Repositories
{
    public class HerbRepository : IHerbRepository
    {
        private readonly PotionCraftDbContext _context;

        public HerbRepository(PotionCraftDbContext context)
        {
            _context = context;
        }

        public async Task<Herb?> GetByIdAsync(Guid id)
        {
            return await _context.Herbs.FindAsync(id);
        }

        public async Task<List<Herb>> GetAllAsync()
        {
            return await _context.Herbs.ToListAsync();
        }

        public async Task AddAsync(Herb herb)
        {
            await _context.Herbs.AddAsync(herb);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Herb herb)
        {
            _context.Herbs.Update(herb);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var herb = await _context.Herbs.FindAsync(id);
            if (herb != null)
            {
                _context.Herbs.Remove(herb);
                await _context.SaveChangesAsync();
            }
        }
    }
}