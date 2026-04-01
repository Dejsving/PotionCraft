using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Services.Gathering;

namespace PotionCraft.Pages.Gathering
{
    /// <summary>
    /// Модель страницы сбора трав.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IPlayerCharacterRepository _characterRepository;
        private readonly IGatheringService _gatheringService;

        public IndexModel(IPlayerCharacterRepository characterRepository, IGatheringService gatheringService)
        {
            _characterRepository = characterRepository;
            _gatheringService = gatheringService;
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

        public Dictionary<Guid, GatheringResult> GatheredHerbs { get; set; } = new();

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
        /// Возвращает текстовое представление собранных трав для копирования в буфер обмена.
        /// </summary>
        /// <returns>Форматированная строка со списком трав, сгруппированных по редкости.</returns>
        public string GetClipboardText()
        {
            if (GatheredHerbs == null || !GatheredHerbs.Any())
            {
                return string.Empty;
            }

            var groupedForClipboard = GatheredHerbs.Values
                .Where(x => x.Herb != null)
                .GroupBy(x => x.Herb!.Rarity)
                .OrderBy(g => g.Key)
                .Select(g =>
                    g.Key.GetDisplayName() + ":\n" +
                    string.Join("\n", g.OrderBy(x => x.Herb!.Name).Select(x => $"- {x.Herb!.Name} {x.Quantity}"))
                );

            return string.Join("\n", groupedForClipboard);
        }

        /// <summary>
        /// Обработчик GET запроса. Инициализирует значения по умолчанию.
        /// </summary>
        public void OnGet()
        {
            PrepareOptions();
            Input.IsDay = true;
            Input.IsRain = false;
            Input.IsCave = false;
            Input.IsProvisionsUsed = true;
            Input.Difficulty = 20;
            Input.RollsCount = 1;
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

            if (successes > 0)
            {
                var request = new GatheringRequest
                {
                    Terrain = Input.Habitat,
                    IsRaining = Input.IsRain,
                    IsNight = !Input.IsDay,
                    IsCave = Input.IsCave,
                    HasProvisions = Input.IsProvisionsUsed,
                    Character = character
                };

                for (int i = 0; i < successes; i++)
                {
                    var res = await _gatheringService.GatherHerbAsync(request);
                    if (res != null && res.Herb != null)
                    {
                        if (GatheredHerbs.TryGetValue(res.Herb.Id, out var existing))
                        {
                            existing.Quantity += res.Quantity;
                        }
                        else
                        {
                            GatheredHerbs.Add(res.Herb.Id, res);
                        }
                    }
                }
            }

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