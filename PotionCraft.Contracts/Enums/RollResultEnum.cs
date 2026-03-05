using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Результат варки зелья: успех, провал, критический успех или критический провал.
/// </summary>
public enum RollResultEnum
{
    [Display(Name = "Критический провал")]
    CriticalFailure,

    [Display(Name = "Провал")]
    Failure,

    [Display(Name = "успех")]
    Success,

    [Display(Name = "Критический успех")]
    CriticalSuccess
}