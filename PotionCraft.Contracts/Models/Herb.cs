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
    /// Тип травы.
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
    /// Машиночитаемый эффект модификатора. None для не-модификаторов.
    /// </summary>
    public HerbModifierEffectEnum ModifierEffect { get; set; } = HerbModifierEffectEnum.None;

    /// <summary>
    /// Изменение сложности зелья при добавлении ингредиента
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Количество костей базовой формулы эффекта. Null, если у травы нет вычисляемой формулы (фиксированный текстовый эффект).
    /// </summary>
    public int? FormulaDiceCount { get; set; }

    /// <summary>
    /// Тип кости базовой формулы эффекта. Null, если у травы нет вычисляемой формулы.
    /// </summary>
    public DiceTypeEnum? FormulaDiceType { get; set; }

    /// <summary>
    /// Признак того, что к базовой формуле прибавляется Мод. Алхимии.
    /// </summary>
    public bool? FormulaIncludesAlchemyMod { get; set; }

    /// <summary>
    /// Текстовый префикс перед вычисленной формулой (например, "Исцеляет "). Null, если формула не требует префикса.
    /// </summary>
    public string? FormulaPrefix { get; set; }

    /// <summary>
    /// Тип урона базового эффекта (для трав с уроном, например ядов).
    /// </summary>
    public DamageTypeEnum? DamageType { get; set; }

    /// <summary>
    /// Варианты типов урона, которыми модификатор может заменить исходный тип урона.
    /// </summary>
    public IReadOnlyList<DamageTypeEnum>? ReplacementDamageTypes { get; set; }

    /// <summary>
    /// Итоговый эффект не вычисляется автоматически и оставляется на решение Мастера (например, Хромовая слизь).
    /// </summary>
    public bool RequiresDmJudgement { get; set; }

    /// <summary>
    /// Среда обитания растения и результат броска в среде обитания
    /// </summary>
    public IReadOnlyDictionary<TerrainEnum, int> Habitats { get; set; }
        = new Dictionary<TerrainEnum, int>();
}