using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts;

namespace PotionCraft.Pages.Characters
{
    public class SelectModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SelectModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<PlayerCharacter> Characters { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("PotionCraftApi");
            Characters = await client.GetFromJsonAsync<List<PlayerCharacter>>("/api/characters")
                         ?? new List<PlayerCharacter>();
        }
    }
}
