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
        public bool HasHerbalismKitProficiency { get; set; }

        [BindProperty]
        public bool HasHerbalismKitExpertise { get; set; }

        [BindProperty]
        public bool HasAlchemistSuppliesProficiency { get; set; }

        [BindProperty]
        public bool HasAlchemistSuppliesExpertise { get; set; }

        [BindProperty]
        public bool HasPoisonerSuppliesProficiency { get; set; }

        [BindProperty]
        public bool HasPoisonerSuppliesExpertise { get; set; }

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
                HasHerbalismKitProficiency = HasHerbalismKitProficiency,
                HasHerbalismKitExpertise = HasHerbalismKitExpertise,
                HasAlchemistSuppliesProficiency = HasAlchemistSuppliesProficiency,
                HasAlchemistSuppliesExpertise = HasAlchemistSuppliesExpertise,
                HasPoisonerSuppliesProficiency = HasPoisonerSuppliesProficiency,
                HasPoisonerSuppliesExpertise = HasPoisonerSuppliesExpertise
            };

            await _characterRepository.AddAsync(character);

            return RedirectToPage("/Index");
        }
    }
}
