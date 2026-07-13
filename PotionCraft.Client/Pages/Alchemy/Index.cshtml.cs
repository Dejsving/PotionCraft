using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts.Enums;
using System.Net.Http.Json;

namespace PotionCraft.Pages.Alchemy;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string HerbsJson { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("PotionCraftApi");
        var allHerbs = await client.GetFromJsonAsync<List<HerbDto>>("/api/herbs") ?? new();

        var relevant = allHerbs
            .Where(h => ((HerbTypeEnum)h.HerbType).HasFlag(HerbTypeEnum.HealingBase) ||
                        ((HerbTypeEnum)h.HerbType).HasFlag(HerbTypeEnum.PoisonBase) ||
                        ((HerbTypeEnum)h.HerbType).HasFlag(HerbTypeEnum.HealingModifier) ||
                        ((HerbTypeEnum)h.HerbType).HasFlag(HerbTypeEnum.PoisonModifier))
            .Select(h => new
            {
                id = h.Id.ToString(),
                name = h.Name,
                herbType = h.HerbType,
                difficulty = h.Difficulty,
                effect = h.Effect
            })
            .OrderBy(h => h.name)
            .ToList();

        HerbsJson = JsonSerializer.Serialize(relevant);
    }

    private class HerbDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int HerbType { get; set; }
        public int Difficulty { get; set; }
        public string Effect { get; set; } = "";
    }
}
