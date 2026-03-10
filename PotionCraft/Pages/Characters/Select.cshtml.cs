using Microsoft.AspNetCore.Mvc.RazorPages;
using PotionCraft.Contracts;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Pages.Characters
{
    /// <summary>
    /// Модель страницы выбора персонажа.
    /// </summary>
    public class SelectModel : PageModel
    {
        private readonly IPlayerCharacterRepository _characterRepository;

        /// <summary>
        /// Конструктор модели.
        /// </summary>
        public SelectModel(IPlayerCharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        /// <summary>
        /// Список всех персонажей игрока.
        /// </summary>
        public List<PlayerCharacter> Characters { get; set; } = new();

        /// <summary>
        /// Обработчик GET запроса. Достает всех персонажей из БД.
        /// </summary>
        public async Task OnGetAsync()
        {
            Characters = await _characterRepository.GetAllAsync();
        }
    }
}