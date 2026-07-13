using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Server.Controllers
{
    public record SelectCharacterRequest(Guid PlayerId);

    public record DeselectCharacterRequest(Guid PlayerId);

    public record CreateCharacterRequest(
        string Name,
        int Intelligence,
        int Wisdom,
        int ProficiencyBonus,
        int HerbalismProficiencyLevel,
        int HerbalismToolModifier,
        int AlchemistProficiencyLevel,
        int AlchemistToolModifier,
        int PoisonerProficiencyLevel,
        int PoisonerToolModifier
    );

    [ApiController]
    [Route("api/characters")]
    public class CharactersController : ControllerBase
    {
        private readonly IPlayerCharacterRepository _characterRepository;

        public CharactersController(IPlayerCharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var characters = await _characterRepository.GetAllAsync();
            return Ok(characters);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCharacterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Необходимо ввести имя персонажа." });

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Intelligence = request.Intelligence,
                Wisdom = request.Wisdom,
                ProficiencyBonus = request.ProficiencyBonus,
                HerbalismTool = new Tool
                {
                    Proficiency = request.HerbalismProficiencyLevel >= 1,
                    Expertise = request.HerbalismProficiencyLevel == 2,
                    Modifier = request.HerbalismToolModifier
                },
                AlchemistTool = new Tool
                {
                    Proficiency = request.AlchemistProficiencyLevel >= 1,
                    Expertise = request.AlchemistProficiencyLevel == 2,
                    Modifier = request.AlchemistToolModifier
                },
                PoisonerTool = new Tool
                {
                    Proficiency = request.PoisonerProficiencyLevel >= 1,
                    Expertise = request.PoisonerProficiencyLevel == 2,
                    Modifier = request.PoisonerToolModifier
                }
            };

            try
            {
                await _characterRepository.AddAsync(character);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return Ok(character);
        }

        [HttpPost("{id:guid}/select")]
        public async Task<IActionResult> Select(Guid id, [FromBody] SelectCharacterRequest request)
        {
            var success = await _characterRepository.SelectCharacterAsync(id, request.PlayerId);
            if (!success)
                return Conflict(new { message = "Персонаж уже занят" });

            var character = await _characterRepository.GetByIdAsync(id);
            return Ok(character);
        }

        [HttpPost("{id:guid}/deselect")]
        public async Task<IActionResult> Deselect(Guid id, [FromBody] DeselectCharacterRequest request)
        {
            await _characterRepository.DeselectCharacterAsync(id, request.PlayerId);
            return Ok();
        }

        [HttpGet("{id:guid}/bag")]
        public async Task<IActionResult> GetBag(Guid id)
        {
            var character = await _characterRepository.GetByIdAsync(id);
            if (character == null)
                return NotFound(new { message = "Персонаж не найден" });

            return Ok(character.Bag);
        }

        public class CoinsUpdateRequest
        {
            public int Gold { get; set; }
            public int Silver { get; set; }
            public int Copper { get; set; }
            public bool IsAdd { get; set; }
        }

        [HttpPost("{id:guid}/bag/coins")]
        public async Task<IActionResult> UpdateCoins(Guid id, [FromBody] CoinsUpdateRequest request)
        {
            var character = await _characterRepository.GetByIdAsync(id);
            if (character == null)
                return NotFound(new { message = "Персонаж не найден" });

            int totalCopperChange = (request.Gold * 100) + (request.Silver * 10) + request.Copper;

            if (request.IsAdd)
                character.Bag.Coins += totalCopperChange;
            else
                character.Bag.Coins = Math.Max(0, character.Bag.Coins - totalCopperChange);

            await _characterRepository.UpdateAsync(character);
            return Ok(character.Bag);
        }
    }
}
