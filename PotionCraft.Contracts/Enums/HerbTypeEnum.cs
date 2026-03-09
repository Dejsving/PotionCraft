using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums;

[Flags]
public enum HerbTypeEnum
{
    None = 0,

    [Display(Name = "Ингредиент зелья")]
    Potion = 1 << 0,  // 1

    [Display(Name = "Ингредиент яда")]
    Poison = 1 << 1,  // 2

    [Display(Name = "Магический ингредиент")]
    Magic = 1 << 2    // 4
}