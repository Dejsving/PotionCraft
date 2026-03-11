using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Pages.Herbs
{
    /// <summary>
    /// Модель страницы списка трав с возможностью фильтрации.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Репозиторий трав.
        /// </summary>
        private readonly IHerbRepository _herbRepository;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IndexModel"/>.
        /// </summary>
        public IndexModel(IHerbRepository herbRepository)
        {
            _herbRepository = herbRepository;
        }

        /// <summary>
        /// Список трав для отображения на странице.
        /// </summary>
        public List<Herb> Herbs { get; set; } = new();

        /// <summary>
        /// Фильтр по названию травы.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? FilterName { get; set; }

        /// <summary>
        /// Фильтр по описанию травы.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? FilterDescription { get; set; }

        /// <summary>
        /// Фильтр по типу травы.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public HerbTypeEnum? FilterHerbType { get; set; }

        /// <summary>
        /// Фильтр по редкости травы.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public RarityEnum? FilterRarity { get; set; }

        /// <summary>
        /// Фильтр по эффекту травы.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? FilterEffect { get; set; }

        /// <summary>
        /// Фильтр по сложности.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? FilterDifficulty { get; set; }

        /// <summary>
        /// Фильтр по среде обитания.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public TerrainEnum? FilterHabitat { get; set; }

        /// <summary>
        /// Обрабатывает GET-запрос. Загружает и фильтрует список трав.
        /// </summary>
        public async Task OnGetAsync()
        {
            var allHerbs = await _herbRepository.GetAllAsync();
            Herbs = ApplyFilters(allHerbs);
        }

        /// <summary>
        /// Применяет фильтры к списку трав.
        /// </summary>
        /// <param name="herbs">Исходный список трав.</param>
        /// <returns>Отфильтрованный список трав.</returns>
        public List<Herb> ApplyFilters(List<Herb> herbs)
        {
            var filtered = herbs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterName))
            {
                filtered = filtered.Where(h =>
                    h.Name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(FilterDescription))
            {
                filtered = filtered.Where(h =>
                    h.Description.Contains(FilterDescription, StringComparison.OrdinalIgnoreCase));
            }

            if (FilterHerbType.HasValue)
            {
                filtered = filtered.Where(h =>
                    h.HerbType.HasFlag(FilterHerbType.Value));
            }

            if (FilterRarity.HasValue)
            {
                filtered = filtered.Where(h => h.Rarity == FilterRarity.Value);
            }

            if (!string.IsNullOrWhiteSpace(FilterEffect))
            {
                filtered = filtered.Where(h =>
                    h.Effect.Contains(FilterEffect, StringComparison.OrdinalIgnoreCase));
            }

            if (FilterDifficulty.HasValue)
            {
                filtered = filtered.Where(h => h.Difficulty == FilterDifficulty.Value);
            }

            if (FilterHabitat.HasValue)
            {
                filtered = filtered.Where(h =>
                    h.Habitats.ContainsKey(FilterHabitat.Value));
            }

            return filtered.ToList();
        }
    }
}
