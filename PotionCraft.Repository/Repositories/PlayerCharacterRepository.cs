using Microsoft.EntityFrameworkCore;

using PotionCraft.Contracts;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Repository.Repositories
{
    public class PlayerCharacterRepository : IPlayerCharacterRepository
    {
        private readonly PotionCraftDbContext _context;

        public PlayerCharacterRepository(PotionCraftDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerCharacter?> GetByIdAsync(Guid id)
        {
            return await _context.PlayerCharacters.FindAsync(id);
        }

        public async Task<List<PlayerCharacter>> GetAllAsync()
        {
            return await _context.PlayerCharacters.ToListAsync();
        }

        public async Task AddAsync(PlayerCharacter character)
        {
            if (await _context.PlayerCharacters.AnyAsync(c => c.Name == character.Name))
            {
                throw new InvalidOperationException($"Character with name '{character.Name}' already exists.");
            }

            await _context.PlayerCharacters.AddAsync(character);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlayerCharacter character)
        {
            if (await _context.PlayerCharacters.AnyAsync(c => c.Name == character.Name && c.Id != character.Id))
            {
                throw new InvalidOperationException($"Character with name '{character.Name}' already exists.");
            }

            _context.PlayerCharacters.Update(character);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var character = await _context.PlayerCharacters.FindAsync(id);
            if (character != null)
            {
                _context.PlayerCharacters.Remove(character);
                await _context.SaveChangesAsync();
            }
        }
    }
}
