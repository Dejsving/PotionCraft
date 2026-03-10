using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PotionCraft.Pages.Characters
{
    public class CreateModel : PageModel
    {
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

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Реализовать сохранение персонажа в базу данных или систему хранения.
            // Пока что просто возвращаем пользователя на главную страницу:
            return RedirectToPage("/Index");
        }
    }
}
