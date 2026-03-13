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

        /// <summary>
        /// Атомарно помечает персонажа как выбранного указанным игроком.
        /// Если у этого игрока ранее был выбран другой персонаж — автоматически освобождает его.
        /// Возвращает <c>true</c>, если резервирование прошло успешно;
        /// <c>false</c>, если персонаж уже занят другим игроком.
        /// </summary>
        public async Task<bool> SelectCharacterAsync(Guid characterId, Guid playerId)
        {
            // Освобождаем предыдущего персонажа этого игрока
            var previous = await _context.PlayerCharacters
                .FirstOrDefaultAsync(c => c.SelectedBy == playerId && c.Id != characterId);
            if (previous != null)
                previous.SelectedBy = null;

            var character = await _context.PlayerCharacters.FindAsync(characterId);
            if (character == null || (character.SelectedBy.HasValue && character.SelectedBy != playerId))
                return false;

            character.SelectedBy = playerId;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Освобождает персонажа, только если он занят указанным игроком.
        /// </summary>
        public async Task DeselectCharacterAsync(Guid characterId, Guid playerId)
        {
            var character = await _context.PlayerCharacters.FindAsync(characterId);
            if (character != null && character.SelectedBy == playerId)
            {
                character.SelectedBy = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
