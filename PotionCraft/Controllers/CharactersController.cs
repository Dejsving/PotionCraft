using Microsoft.AspNetCore.Mvc;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Controllers
{
    /// <summary>
    /// Тело запроса на выбор персонажа.
    /// </summary>
    public record SelectCharacterRequest(
        /// <summary>Уникальный ID игрока, сгенерированный на стороне браузера.</summary>
        Guid PlayerId);

    /// <summary>
    /// Тело запроса на снятие выбора персонажа.
    /// </summary>
    public record DeselectCharacterRequest(
        /// <summary>Уникальный ID игрока, сгенерированный на стороне браузера.</summary>
        Guid PlayerId);

    /// <summary>
    /// API-контроллер для управления резервированием персонажей.
    /// </summary>
    [ApiController]
    [Route("api/characters")]
    public class CharactersController : ControllerBase
    {
        private readonly IPlayerCharacterRepository _characterRepository;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера.
        /// </summary>
        public CharactersController(IPlayerCharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        /// <summary>
        /// Резервирует персонажа для игрока с указанным <c>playerId</c>.
        /// Автоматически освобождает предыдущего персонажа этого игрока.
        /// Возвращает 200 OK с моделью персонажа при успехе,
        /// или 409 Conflict если персонаж уже занят другим игроком.
        /// </summary>
        [HttpPost("{id:guid}/select")]
        public async Task<IActionResult> Select(Guid id, [FromBody] SelectCharacterRequest request)
        {
            var success = await _characterRepository.SelectCharacterAsync(id, request.PlayerId);
            if (!success)
                return Conflict(new { message = "Персонаж уже занят" });

            var character = await _characterRepository.GetByIdAsync(id);
            return Ok(character);
        }

        /// <summary>
        /// Снимает резервирование с персонажа, если он занят указанным игроком.
        /// </summary>
        [HttpPost("{id:guid}/deselect")]
        public async Task<IActionResult> Deselect(Guid id, [FromBody] DeselectCharacterRequest request)
        {
            await _characterRepository.DeselectCharacterAsync(id, request.PlayerId);
            return Ok();
        }

        /// <summary>
        /// Возвращает содержимое сумки персонажа.
        /// </summary>
        [HttpGet("{id:guid}/bag")]
        public async Task<IActionResult> GetBag(Guid id)
        {
            var character = await _characterRepository.GetByIdAsync(id);
            if (character == null)
                return NotFound(new { message = "Персонаж не найден" });

            return Ok(character.Bag);
        }
    }
}
