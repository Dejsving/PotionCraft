namespace PotionCraft.Contracts.Models;

/// <summary>
/// Запрос на сбор трав, содержит все необходимые параметры.
/// </summary>
public class GatheringRequest
{
    /// <summary>
    /// Территория сбора.
    /// </summary>
    public Enums.TerrainEnum Terrain { get; set; }

    /// <summary>
    /// Идет ли дождь.
    /// </summary>
    public bool IsRaining { get; set; }

    /// <summary>
    /// Ночное ли время.
    /// </summary>
    public bool IsNight { get; set; }

    /// <summary>
    /// Находятся ли персонажи в пещере.
    /// </summary>
    public bool IsCave { get; set; }

    /// <summary>
    /// Используется ли провизия.
    /// </summary>
    public bool HasProvisions { get; set; }

    /// <summary>
    /// Персонаж, осуществляющий сбор.
    /// </summary>
    public PlayerCharacter? Character { get; set; }
}
