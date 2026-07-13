using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace PotionCraft.Pages
{
    public class ShopModel : PageModel
    {
        private readonly ILogger<ShopModel> _logger;

        public string ApiBaseUrl { get; private set; } = "";

        public ShopModel(ILogger<ShopModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            ApiBaseUrl = configuration["ApiBaseUrl"] ?? "";
        }

        public void OnGet()
        {
        }
    }
}
