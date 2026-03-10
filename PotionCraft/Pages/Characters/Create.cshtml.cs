using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts;
using PotionCraft.Repository.Abstraction;
using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Pages.Characters
{
    public class CreateModel : PageModel
    {
        private readonly IPlayerCharacterRepository _characterRepository;

        public CreateModel(IPlayerCharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        [BindProperty]
        [Required(ErrorMessage = "Необходимо ввести имя персонажа.")]
        [StringLength(100, ErrorMessage = "Имя слишком длинное.")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Range(0, 100, ErrorMessage = "Значение интеллекта должно быть от 0 до 100.")]
        public int Intelligence { get; set; } = 10;

        [BindProperty]
        [Range(0, 100, ErrorMessage = "Значение мудрости должно быть от 0 до 100.")]
        public int Wisdom { get; set; } = 10;

        [BindProperty]
        [Range(2, 6, ErrorMessage = "Бонус мастерства должен быть от 2 до 6.")]
        public int ProficiencyBonus { get; set; } = 2;

        [BindProperty]
        public int HerbalismProficiencyLevel { get; set; } = 0; // 0 = Нет, 1 = Владение, 2 = Мастерство

        [BindProperty]
        public int AlchemistProficiencyLevel { get; set; } = 0; // 0 = Нет, 1 = Владение, 2 = Мастерство

        [BindProperty]
        public int PoisonerProficiencyLevel { get; set; } = 0; // 0 = Нет, 1 = Владение, 2 = Мастерство

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = Name,
                Intelligence = Intelligence,
                Wisdom = Wisdom,
                ProficiencyBonus = ProficiencyBonus,
                HasHerbalismKitProficiency = HerbalismProficiencyLevel >= 1,
                HasHerbalismKitExpertise = HerbalismProficiencyLevel == 2,
                HasAlchemistSuppliesProficiency = AlchemistProficiencyLevel >= 1,
                HasAlchemistSuppliesExpertise = AlchemistProficiencyLevel == 2,
                HasPoisonerSuppliesProficiency = PoisonerProficiencyLevel >= 1,
                HasPoisonerSuppliesExpertise = PoisonerProficiencyLevel == 2
            };

            await _characterRepository.AddAsync(character);

            return RedirectToPage("/Index");
        }
    }
}
