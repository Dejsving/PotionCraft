using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotionCraft.Contracts.Enums
{
    public enum DestinationEnum
    {
        [Display(Name = "Ингредиент зелья")]
        Potion = 0,

        [Display(Name = "Ингредиент яда")]
        Poison = 1,

        [Display(Name = "Магический ингредиент")]
        Magic = 2
    }
}
