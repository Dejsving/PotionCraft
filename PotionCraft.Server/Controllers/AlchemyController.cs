using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Server.Controllers
{
    /// <summary>
    /// Контроллер расчёта итогового эффекта зелья на странице «Алхимия».
    /// </summary>
    [ApiController]
    [Route("api/alchemy")]
    public class AlchemyController : ControllerBase
    {
        private readonly IHerbRepository _herbRepository;
        private readonly IPotionEffectResolver _potionEffectResolver;

        /// <summary>
        /// Создаёт экземпляр контроллера алхимии.
        /// </summary>
        public AlchemyController(IHerbRepository herbRepository, IPotionEffectResolver potionEffectResolver)
        {
            _herbRepository = herbRepository;
            _potionEffectResolver = potionEffectResolver;
        }

        /// <summary>
        /// Вычисляет итоговый эффект зелья на основе травы-основы и упорядоченного списка модификаторов.
        /// </summary>
        /// <param name="request">Идентификаторы травы-основы и модификаторов в порядке их добавления.</param>
        [HttpPost("resolve")]
        public async Task<IActionResult> Resolve([FromBody] ResolvePotionEffectRequest request)
        {
            var baseHerb = await _herbRepository.GetByIdAsync(request.BaseHerbId);
            if (baseHerb is null)
                return NotFound($"Трава-основа с id '{request.BaseHerbId}' не найдена.");

            var modifiers = new List<Contracts.Models.Herb>();
            foreach (var modifierId in request.ModifierHerbIds)
            {
                var modifier = await _herbRepository.GetByIdAsync(modifierId);
                if (modifier is null)
                    return NotFound($"Трава-модификатор с id '{modifierId}' не найдена.");
                modifiers.Add(modifier);
            }

            var result = _potionEffectResolver.Resolve(baseHerb, modifiers);
            return Ok(result);
        }
    }
}
