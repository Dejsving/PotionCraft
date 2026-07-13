using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Models;
using System.Net.Http.Json;

namespace PotionCraft.Pages.Herbs
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Herb> Herbs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FilterName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterDescription { get; set; }

        [BindProperty(SupportsGet = true)]
        public HerbTypeEnum? FilterHerbType { get; set; }

        [BindProperty(SupportsGet = true)]
        public RarityEnum? FilterRarity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterEffect { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterDifficulty { get; set; }

        [BindProperty(SupportsGet = true)]
        public TerrainEnum? FilterHabitat { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool SortDesc { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("PotionCraftApi");
            var allHerbs = await client.GetFromJsonAsync<List<Herb>>("/api/herbs") ?? new();
            var filtered = ApplyFilters(allHerbs);
            Herbs = ApplySorting(filtered);
        }

        public List<Herb> ApplyFilters(List<Herb> herbs)
        {
            var filtered = herbs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterName))
                filtered = filtered.Where(h => h.Name.Contains(FilterName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FilterDescription))
                filtered = filtered.Where(h => h.Description.Contains(FilterDescription, StringComparison.OrdinalIgnoreCase));

            if (FilterHerbType.HasValue)
                filtered = filtered.Where(h => h.HerbType.HasFlag(FilterHerbType.Value));

            if (FilterRarity.HasValue)
                filtered = filtered.Where(h => h.Rarity == FilterRarity.Value);

            if (!string.IsNullOrWhiteSpace(FilterEffect))
                filtered = filtered.Where(h => h.Effect.Contains(FilterEffect, StringComparison.OrdinalIgnoreCase));

            if (FilterDifficulty.HasValue)
                filtered = filtered.Where(h => h.Difficulty == FilterDifficulty.Value);

            if (FilterHabitat.HasValue)
                filtered = filtered.Where(h => h.Habitats.ContainsKey(FilterHabitat.Value));

            return filtered.ToList();
        }

        public List<Herb> ApplySorting(List<Herb> herbs)
        {
            return SortBy switch
            {
                "Name" => SortDesc ? herbs.OrderByDescending(h => h.Name).ToList() : herbs.OrderBy(h => h.Name).ToList(),
                "Description" => SortDesc ? herbs.OrderByDescending(h => h.Description).ToList() : herbs.OrderBy(h => h.Description).ToList(),
                "HerbType" => SortDesc ? herbs.OrderByDescending(h => (int)h.HerbType).ToList() : herbs.OrderBy(h => (int)h.HerbType).ToList(),
                "Rarity" => SortDesc ? herbs.OrderByDescending(h => (int)h.Rarity).ToList() : herbs.OrderBy(h => (int)h.Rarity).ToList(),
                "Difficulty" => SortDesc ? herbs.OrderByDescending(h => h.Difficulty).ToList() : herbs.OrderBy(h => h.Difficulty).ToList(),
                "Effect" => SortDesc ? herbs.OrderByDescending(h => h.Effect).ToList() : herbs.OrderBy(h => h.Effect).ToList(),
                "Habitat" => SortDesc
                    ? herbs.OrderByDescending(h => h.Habitats.Any() ? (int)h.Habitats.Keys.Min() : int.MaxValue).ToList()
                    : herbs.OrderBy(h => h.Habitats.Any() ? (int)h.Habitats.Keys.Min() : int.MaxValue).ToList(),
                _ => herbs
            };
        }
    }
}
