using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums;

[Flags]
public enum HerbTypeEnum
{
    [Display(Name = "Тип неопределен")]
    None = 0,                 // 0

    [Display(Name = "Основа лечебного зелья")]
    HealingBase = 1 << 0,     // 1

    [Display(Name = "Модификатор лечебного зелья")]
    HealingModifier = 1 << 1, // 2

    [Display(Name = "Основа яда")]
    PoisonBase = 1 << 2,      // 4

    [Display(Name = "Модификатор яда")]
    PoisonModifier = 1 << 3,  // 8

    [Display(Name = "Магический ингредиент")]
    Magic = 1 << 4            // 16
}