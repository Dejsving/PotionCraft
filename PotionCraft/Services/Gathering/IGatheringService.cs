using PotionCraft.Contracts.Models;

namespace PotionCraft.Services.Gathering;

/// <summary>
/// Интерфейс сервиса сбора трав.
/// </summary>
public interface IGatheringService
{
    /// <summary>
    /// Осуществляет одиночный бросок сбора трав.
    /// </summary>
    /// <param name="request">Запрос на сбор.</param>
    /// <returns>Результат сбора: собранный ингредиент и его количество.</returns>
    Task<GatheringResult> GatherHerbAsync(GatheringRequest request);
}