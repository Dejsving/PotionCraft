using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Pages.Gathering
{
    /// <summary>
    /// Модель страницы сбора трав.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IPlayerCharacterRepository _characterRepository;

        /// <summary>
        /// Конструктор модели.
        /// </summary>
        public IndexModel(IPlayerCharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
            PrepareOptions();
        }

        /// <summary>
        /// Входные параметры для броска сбора трав.
        /// </summary>
        [BindProperty]
        public GatheringInputModel Input { get; set; } = new();

        /// <summary>
        /// Общее количество успехов при сборе трав. Null, если бросок еще не совершался.
        /// </summary>
        public int? TotalSuccesses { get; set; }

        /// <summary>
        /// Имя персонажа, совершавшего бросок.
        /// </summary>
        public string? CharacterName { get; set; }

        /// <summary>
        /// Результаты каждого отдельного броска (базовый кубик + модификатор).
        /// </summary>
        public List<int> RollResults { get; set; } = new();

        /// <summary>
        /// Доступные опции для выбора среды обитания.
        /// </summary>
        public SelectList TerrainOptions { get; set; } = null!;

        /// <summary>
        /// Обработчик GET запроса. Инициализирует значения по умолчанию.
        /// </summary>
        public void OnGet()
        {
            PrepareOptions();
            Input.IsDay = true;
            Input.IsRain = false;
            Input.IsCave = false;
            Input.Difficulty = 20;
            Input.RollsCount = 1;
            Input.IsProvisionsUsed = true;
        }

        /// <summary>
        /// Обработчик POST запроса. Выполняет расчет бросков.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            PrepareOptions();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!Input.CharacterId.HasValue || Input.CharacterId.Value == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, выберите персонажа.");
                return Page();
            }

            var character = await _characterRepository.GetByIdAsync(Input.CharacterId.Value);
            if (character == null)
            {
                ModelState.AddModelError(string.Empty, "Выбранный персонаж не найден в базе данных.");
                return Page();
            }

            CharacterName = character.Name;
            int modifier = character.HerbalismModify;
            int successes = 0;
            
            for (int i = 0; i < Input.RollsCount; i++)
            {
                int baseRoll = DiceRoll.D20.Roll();
                int total = baseRoll + modifier;
                RollResults.Add(total);
                
                if (total >= Input.Difficulty)
                {
                    successes++;
                }
            }

            TotalSuccesses = successes;

            return Page();
        }

        private void PrepareOptions()
        {
            var terrains = Enum.GetValues(typeof(TerrainEnum))
                .Cast<TerrainEnum>()
                .Select(t => new { Value = (int)t, Text = t.GetDisplayName() })
                .ToList();
                
            TerrainOptions = new SelectList(terrains, "Value", "Text");
        }

        /// <summary>
        /// Класс для хранения входных параметров формы.
        /// </summary>
        public class GatheringInputModel
        {
            /// <summary>
            /// День или ночь.
            /// </summary>
            public bool IsDay { get; set; }

            /// <summary>
            /// Дождь или нет.
            /// </summary>
            public bool IsRain { get; set; }

            /// <summary>
            /// Пещера или нет.
            /// </summary>
            public bool IsCave { get; set; }

            /// <summary>
            /// Сложность поиска (DC).
            /// </summary>
            public int Difficulty { get; set; }

            /// <summary>
            /// Количество бросков.
            /// </summary>
            public int RollsCount { get; set; }

            /// <summary>
            /// Использование провизии.
            /// </summary>
            public bool IsProvisionsUsed { get; set; }

            /// <summary>
            /// Среда обитания, где происходит сбор.
            /// </summary>
            public TerrainEnum Habitat { get; set; }

            /// <summary>
            /// Идентификатор персонажа, осуществляющего сбор.
            /// </summary>
            public Guid? CharacterId { get; set; }
        }
    }
}
