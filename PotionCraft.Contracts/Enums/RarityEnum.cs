using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Редкость травы.
/// </summary>
public enum RarityEnum
{
    [Display(Name = "Обычный")]
    Common = 0,

    [Display(Name = "Необычный")]
    Unusual = 1,

    [Display(Name = "Редкий")]
    Rare = 2,

    [Display(Name = "Очень редкий")]
    VeryRare = 3
}
