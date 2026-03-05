using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Среда обитания растения — определяет, где его можно найти.
/// </summary>
public enum TerrainEnum
{
    [Display(Name = "Повсеместно")]
    Everewhere = 0,

    [Display(Name = "Берега")]
    Coast = 1,

    [Display(Name = "Подъземье")]
    Underdark = 2,

    [Display(Name = "Лес")]
    Forest = 3,

    [Display(Name = "Болото")]
    Swamp = 4,

    [Display(Name = "Арктика")]
    Arctic = 5,

    [Display(Name = "Холмы")]
    Hills = 6,

    [Display(Name = "Луга")]
    Meadows = 7,

    [Display(Name = "Горы")]
    Mountains = 8,

    [Display(Name = "Пустыни")]
    Deserts = 9,

    [Display(Name = "Специальный")]
    Special = 10,

    [Display(Name = "Подводная среда")]
    UnderWater = 11,
}
