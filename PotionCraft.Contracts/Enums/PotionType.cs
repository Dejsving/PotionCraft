namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Тип зелья по категории воздействия.
/// </summary>
public enum PotionType
{
    /// <summary>Зелье лечения.</summary>
    Healing,

    /// <summary>Яд.</summary>
    Poison,

    /// <summary>Противоядие.</summary>
    Antidote,

    /// <summary>Зелье усиления.</summary>
    Enhancement,

    /// <summary>Утилитарное зелье.</summary>
    Utility,

    /// <summary>Зелье стихийного воздействия.</summary>
    Elemental,

    /// <summary>Зелье восстановления.</summary>
    Restorative
}
