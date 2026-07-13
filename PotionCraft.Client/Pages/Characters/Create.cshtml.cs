using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace PotionCraft.Pages.Characters
{
    public class CreateCharacterApiRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int ProficiencyBonus { get; set; }
        public int HerbalismProficiencyLevel { get; set; }
        public int HerbalismToolModifier { get; set; }
        public int AlchemistProficiencyLevel { get; set; }
        public int AlchemistToolModifier { get; set; }
        public int PoisonerProficiencyLevel { get; set; }
        public int PoisonerToolModifier { get; set; }
    }

    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
        public int HerbalismProficiencyLevel { get; set; } = 0;

        [BindProperty]
        [Range(-3, 3, ErrorMessage = "Модификатор набора травника должен быть от -3 до 3.")]
        public int HerbalismToolModifier { get; set; } = 0;

        [BindProperty]
        public int AlchemistProficiencyLevel { get; set; } = 0;

        [BindProperty]
        [Range(-3, 3, ErrorMessage = "Модификатор инструментов алхимика должен быть от -3 до 3.")]
        public int AlchemistToolModifier { get; set; } = 0;

        [BindProperty]
        public int PoisonerProficiencyLevel { get; set; } = 0;

        [BindProperty]
        [Range(-3, 3, ErrorMessage = "Модификатор инструментов отравителя должен быть от -3 до 3.")]
        public int PoisonerToolModifier { get; set; } = 0;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("PotionCraftApi");

            var request = new CreateCharacterApiRequest
            {
                Name = Name,
                Intelligence = Intelligence,
                Wisdom = Wisdom,
                ProficiencyBonus = ProficiencyBonus,
                HerbalismProficiencyLevel = HerbalismProficiencyLevel,
                HerbalismToolModifier = HerbalismToolModifier,
                AlchemistProficiencyLevel = AlchemistProficiencyLevel,
                AlchemistToolModifier = AlchemistToolModifier,
                PoisonerProficiencyLevel = PoisonerProficiencyLevel,
                PoisonerToolModifier = PoisonerToolModifier
            };

            var response = await client.PostAsJsonAsync("/api/characters", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                ModelState.AddModelError("Name", error?.Message ?? "Ошибка при создании персонажа.");
                return Page();
            }

            var character = await response.Content.ReadFromJsonAsync<CharacterIdResponse>();
            return RedirectToPage("Select", new { autoSelect = character?.Id });
        }

        private record ErrorResponse(string Message);
        private record CharacterIdResponse(Guid Id);
    }
}
