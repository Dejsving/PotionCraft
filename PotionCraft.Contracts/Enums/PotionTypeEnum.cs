using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Contracts.Enums
{
    public enum PotionTypeEnum
    {
        [Display(Name = "Зелье")]
        Potion = 0,

        [Display(Name = "Яд")]
        Poison = 1,

        [Display(Name = "Магическое зелье")]
        Magic = 2
    }
}