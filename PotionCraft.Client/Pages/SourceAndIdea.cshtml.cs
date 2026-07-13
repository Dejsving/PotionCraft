using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PotionCraft.Pages
{
    public class SourceAndIdea : PageModel
    {
        private readonly ILogger<SourceAndIdea> _logger;

        public SourceAndIdea(ILogger<SourceAndIdea> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
