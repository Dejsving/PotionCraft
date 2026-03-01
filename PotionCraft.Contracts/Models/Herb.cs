using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Трава — растение, используемое в гербализме и алхимии.
/// </summary>
public class Herb
{
    /// <summary>Уникальный идентификатор травы.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Название травы.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание внешнего вида и особенностей растения.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Редкость травы, влияет на DC поиска.</summary>
    public HerbRarity Rarity { get; set; }

    /// <summary>Сложность (DC) проверки для обнаружения травы.</summary>
    public int FindDC { get; set; }

    /// <summary>Среда обитания растения.</summary>
    public IReadOnlyList<Terrain> Habitats { get; set; } = [];

    /// <summary>Доступные алхимические компоненты растения по частям.</summary>
    public IReadOnlyList<HerbComponent> Components { get; set; } = [];

    /// <summary>Стоимость одной дозы в медных монетах.</summary>
    public int ValueInCopper { get; set; }
}
