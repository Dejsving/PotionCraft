using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts.Enums;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Pages.Alchemy;

public class IndexModel : PageModel
{
    private readonly IHerbRepository _herbRepository;

    public IndexModel(IHerbRepository herbRepository)
    {
        _herbRepository = herbRepository;
    }

    public string HerbsJson { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        var allHerbs = await _herbRepository.GetAllAsync();

        var relevant = allHerbs
            .Where(h => h.HerbType.HasFlag(HerbTypeEnum.HealingBase) ||
                        h.HerbType.HasFlag(HerbTypeEnum.PoisonBase) ||
                        h.HerbType.HasFlag(HerbTypeEnum.HealingModifier) ||
                        h.HerbType.HasFlag(HerbTypeEnum.PoisonModifier))
            .Select(h => new
            {
                id = h.Id.ToString(),
                name = h.Name,
                herbType = (int)h.HerbType,
                difficulty = h.Difficulty,
                effect = h.Effect
            })
            .OrderBy(h => h.name)
            .ToList();

        HerbsJson = JsonSerializer.Serialize(relevant);
    }
}
