using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Models;
using System.Net.Http.Json;

namespace PotionCraft.Pages.Gathering
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            PrepareOptions();
        }

        [BindProperty]
        public GatheringInputModel Input { get; set; } = new();

        public int? TotalSuccesses { get; set; }
        public Dictionary<Guid, GatheringResult> GatheredHerbs { get; set; } = new();
        public string? CharacterName { get; set; }
        public List<int> RollResults { get; set; } = new();
        public SelectList TerrainOptions { get; set; } = null!;

        public string GetClipboardText()
        {
            if (GatheredHerbs == null || !GatheredHerbs.Any())
                return string.Empty;

            var groupedForClipboard = GatheredHerbs.Values
                .Where(x => x.Herb != null)
                .GroupBy(x => x.Herb!.Rarity)
                .OrderBy(g => g.Key)
                .Select(g =>
                    g.Key.GetDisplayName() + ":\n" +
                    string.Join("\n", g.OrderBy(x => x.Herb!.Name)
                        .Select(x => $"- {x.Herb!.Name} {x.Quantity}"))
                );

            return string.Join("\n", groupedForClipboard);
        }

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

        public async Task<IActionResult> OnPostAsync()
        {
            PrepareOptions();

            if (!ModelState.IsValid)
                return Page();

            if (!Input.CharacterId.HasValue || Input.CharacterId.Value == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, выберите персонажа.");
                return Page();
            }

            var client = _httpClientFactory.CreateClient("PotionCraftApi");

            var apiRequest = new
            {
                terrain = Input.Habitat,
                isRaining = Input.IsRain,
                isNight = !Input.IsDay,
                isCave = Input.IsCave,
                hasProvisions = Input.IsProvisionsUsed,
                rollsCount = Input.RollsCount,
                difficulty = Input.Difficulty
            };

            var response = await client.PostAsJsonAsync($"/api/gathering/{Input.CharacterId.Value}", apiRequest);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Ошибка при обращении к серверу.");
                return Page();
            }

            var result = await response.Content.ReadFromJsonAsync<GatheringApiResponse>();
            if (result != null)
            {
                CharacterName = result.CharacterName;
                RollResults = result.RollResults;
                TotalSuccesses = result.TotalSuccesses;
                GatheredHerbs = result.GatheredHerbs;
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

        public class GatheringInputModel
        {
            public bool IsDay { get; set; }
            public bool IsRain { get; set; }
            public bool IsCave { get; set; }
            public int Difficulty { get; set; }
            public int RollsCount { get; set; }
            public bool IsProvisionsUsed { get; set; }
            public TerrainEnum Habitat { get; set; }
            public Guid? CharacterId { get; set; }
        }

        private class GatheringApiResponse
        {
            public string CharacterName { get; set; } = "";
            public List<int> RollResults { get; set; } = new();
            public int TotalSuccesses { get; set; }
            public Dictionary<Guid, GatheringResult> GatheredHerbs { get; set; } = new();
        }
    }
}
