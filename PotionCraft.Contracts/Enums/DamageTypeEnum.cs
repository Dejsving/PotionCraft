using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Тип урона, наносимого эффектом зелья или яда.
/// </summary>
public enum DamageTypeEnum
{
    [Display(Name = "Яд")]
    Poison = 0,

    [Display(Name = "Холод")]
    Cold = 1,

    [Display(Name = "Некротическая энергия")]
    Necrotic = 2,

    [Display(Name = "Огонь")]
    Fire = 3,

    [Display(Name = "Кислота")]
    Acid = 4,

    [Display(Name = "Свет")]
    Radiant = 5,
}
