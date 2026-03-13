using PotionCraft.Contracts;

namespace PotionCraft.Repository.Abstraction
{
    /// <summary>
    /// Интерфейс репозитория для работы с персонажами игроков.
    /// </summary>
    public interface IPlayerCharacterRepository
    {
        /// <summary>Получить персонажа по идентификатору.</summary>
        Task<PlayerCharacter?> GetByIdAsync(Guid id);

        /// <summary>Получить всех персонажей.</summary>
        Task<List<PlayerCharacter>> GetAllAsync();

        /// <summary>Добавить нового персонажа.</summary>
        Task AddAsync(PlayerCharacter character);

        /// <summary>Обновить данные персонажа.</summary>
        Task UpdateAsync(PlayerCharacter character);

        /// <summary>Удалить персонажа по идентификатору.</summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Отметить персонажа как занятого указанным игроком.
        /// Автоматически освобождает предыдущего персонажа этого же игрока.
        /// Возвращает <c>true</c> при успехе, <c>false</c> если персонаж уже занят другим игроком.
        /// </summary>
        Task<bool> SelectCharacterAsync(Guid characterId, Guid playerId);

        /// <summary>
        /// Освобождает персонажа, только если он занят указанным игроком.
        /// </summary>
        Task DeselectCharacterAsync(Guid characterId, Guid playerId);
    }
}
