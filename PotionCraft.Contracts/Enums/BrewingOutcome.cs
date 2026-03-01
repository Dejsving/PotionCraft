namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Результат варки зелья: успех, провал, критический успех или критический провал.
/// </summary>
public enum BrewingOutcome
{
    /// <summary>Критический провал.</summary>
    CriticalFailure,

    /// <summary>Провал.</summary>
    Failure,

    /// <summary>Успех.</summary>
    Success,

    /// <summary>Критический успех.</summary>
    CriticalSuccess
}
