using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums
{
    public enum AdditionalRulesEnum
    {
        [Display(Name = "")]
        None = 0,

        [Display(Name = "1 шт.")]
        OneOnly = 1,

        [Display(Name = "2 шт.")]
        TwoOnly = 2,

        [Display(Name = "2 шт, если дождь.")]
        TwoIfRain = 3,

        [Display(Name = "2 шт, если пещера.")]
        TwoIfCave = 4,

        [Display(Name = "Дополнительно: Элементальная вода.")]
        AddElementalWater = 5,

        [Display(Name = "Ночь - 2 шт., День - рерол.")]
        NightTwoDayReroll = 6,

        [Display(Name = "Рерол.")]
        ReRoll = 7,

        [Display(Name = "См. таблицу \"Обычные ингредиенты\".")]
        CheckCommon = 8,

        [Display(Name = "Перебросить, если не используется провизия")]
        ReRollIfNoProvisions = 9,

        [Display(Name = "Только на берегу (иначе переброс/отсутствие)")]
        OnlyOnCoast = 10,

        [Display(Name = "1-2 шт.")]
        OneOrTwo = 11
    }
}