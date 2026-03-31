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
        [Range(-3, 3, ErrorMessage = "Модификатор набора травника должен быть от -3 до 3.")]
        public int HerbalismToolModifier { get; set; } = 0;

        [BindProperty]
        public int AlchemistProficiencyLevel { get; set; } = 0; // 0 = Нет, 1 = Владение, 2 = Мастерство

        [BindProperty]
        [Range(-3, 3, ErrorMessage = "Модификатор инструментов алхимика должен быть от -3 до 3.")]
        public int AlchemistToolModifier { get; set; } = 0;

        [BindProperty]
        public int PoisonerProficiencyLevel { get; set; } = 0; // 0 = Нет, 1 = Владение, 2 = Мастерство

        [BindProperty]
        [Range(-3, 3, ErrorMessage = "Модификатор инструментов отравителя должен быть от -3 до 3.")]
        public int PoisonerToolModifier { get; set; } = 0;

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
                HerbalismTool = new Tool
                {
                    Proficiency = HerbalismProficiencyLevel >= 1,
                    Expertise = HerbalismProficiencyLevel == 2,
                    Modifier = HerbalismToolModifier
                },
                AlchemistTool = new Tool
                {
                    Proficiency = AlchemistProficiencyLevel >= 1,
                    Expertise = AlchemistProficiencyLevel == 2,
                    Modifier = AlchemistToolModifier
                },
                PoisonerTool = new Tool
                {
                    Proficiency = PoisonerProficiencyLevel >= 1,
                    Expertise = PoisonerProficiencyLevel == 2,
                    Modifier = PoisonerToolModifier
                }
            };

            try
            {
                await _characterRepository.AddAsync(character);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Name", ex.Message);
                return Page();
            }

            return RedirectToPage("Select", new { autoSelect = character.Id });
        }
    }
}
