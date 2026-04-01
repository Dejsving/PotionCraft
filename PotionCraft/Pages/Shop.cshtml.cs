using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PotionCraft.Pages
{
    /// <summary>
    /// Модель страницы магазина — торговля между персонажем и торговцем.
    /// </summary>
    public class ShopModel : PageModel
    {
        /// <summary>
        /// Логгер для диагностики.
        /// </summary>
        private readonly ILogger<ShopModel> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр модели страницы магазина.
        /// </summary>
        public ShopModel(ILogger<ShopModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обработчик GET-запроса страницы.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
