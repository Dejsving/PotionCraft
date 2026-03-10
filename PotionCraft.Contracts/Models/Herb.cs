using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Трава — растение, используемое в гербализме и алхимии.
/// </summary>
public class Herb
{
    /// <summary>
    /// Уникальный идентификатор травы.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Название травы.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание внешнего вида и особенностей растения.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Редкость травы.
    /// </summary>
    public HerbTypeEnum HerbType { get; set; }

    /// <summary>
    /// Редкость травы.
    /// </summary>
    public RarityEnum Rarity { get; set; }

    /// <summary>
    /// Описание эффекта растения.
    /// </summary>
    public string Effect { get; set; } = string.Empty;

    /// <summary>
    /// Среда обитания растения и результат броска в среде обитания
    /// </summary>
    public IReadOnlyDictionary<TerrainEnum, int> Habitats { get; set; }
        = new Dictionary<TerrainEnum, int>();

    /// <summary>
    /// Словарь зависимости дополнительных правил от экосистемы (местности)
    /// </summary>
    public IReadOnlyDictionary<TerrainEnum, int> AdditionalRule { get; set; }
        = new Dictionary<TerrainEnum, int>();

    /// <summary>
    /// Словарь зависимости сложности ингредиента от назначения ингредиента
    /// </summary>
    public IReadOnlyDictionary<TerrainEnum, int> Difficulty { get; set; }
        = new Dictionary<TerrainEnum, int>();
}