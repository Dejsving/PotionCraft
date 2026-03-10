using PotionCraft.Contracts;

namespace PotionCraft.Repository.Abstraction
{
    public interface IPlayerCharacterRepository
    {
        Task<PlayerCharacter?> GetByIdAsync(Guid id);
        Task<List<PlayerCharacter>> GetAllAsync();
        Task AddAsync(PlayerCharacter character);
        Task UpdateAsync(PlayerCharacter character);
        Task DeleteAsync(Guid id);
    }
}
