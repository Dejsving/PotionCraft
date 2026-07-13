using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Server.Services.Gathering;

namespace PotionCraft.Server.Controllers
{
    public record GatheringApiRequest(
        TerrainEnum Terrain,
        bool IsRaining,
        bool IsNight,
        bool IsCave,
        bool HasProvisions,
        int RollsCount,
        int Difficulty
    );

    public class GatheringApiResponse
    {
        public string CharacterName { get; set; } = "";
        public List<int> RollResults { get; set; } = new();
        public int TotalSuccesses { get; set; }
        public Dictionary<Guid, GatheringResult> GatheredHerbs { get; set; } = new();
    }

    [ApiController]
    [Route("api/gathering")]
    public class GatheringController : ControllerBase
    {
        private readonly IPlayerCharacterRepository _characterRepository;
        private readonly IGatheringService _gatheringService;
        private readonly IDiceRoller _diceRoller;

        public GatheringController(IPlayerCharacterRepository characterRepository,
            IGatheringService gatheringService, IDiceRoller diceRoller)
        {
            _characterRepository = characterRepository;
            _gatheringService = gatheringService;
            _diceRoller = diceRoller;
        }

        [HttpPost("{characterId:guid}")]
        public async Task<IActionResult> Gather(Guid characterId, [FromBody] GatheringApiRequest request)
        {
            var character = await _characterRepository.GetByIdAsync(characterId);
            if (character == null)
                return NotFound(new { message = "Персонаж не найден." });

            int modifier = character.HerbalismModify;
            var rollResults = new List<int>();
            int successes = 0;

            for (int i = 0; i < request.RollsCount; i++)
            {
                int total = _diceRoller.Roll(DiceRoll.D20) + modifier;
                rollResults.Add(total);
                if (total >= request.Difficulty)
                    successes++;
            }

            var gatheredHerbs = new Dictionary<Guid, GatheringResult>();

            if (successes > 0)
            {
                var gatherRequest = new GatheringRequest
                {
                    Terrain = request.Terrain,
                    IsRaining = request.IsRaining,
                    IsNight = request.IsNight,
                    IsCave = request.IsCave,
                    HasProvisions = request.HasProvisions,
                    Character = character
                };

                for (int i = 0; i < successes; i++)
                {
                    var res = await _gatheringService.GatherHerbAsync(gatherRequest);
                    if (res?.Herb != null)
                    {
                        if (gatheredHerbs.TryGetValue(res.Herb.Id, out var existing))
                            existing.Quantity += res.Quantity;
                        else
                            gatheredHerbs[res.Herb.Id] = res;
                    }
                }

                character.Bag.Herbs = new Dictionary<Guid, GatheringResult>(character.Bag.Herbs);
                foreach (var (herbId, result) in gatheredHerbs)
                    character.Bag.AddOrUpdateHerb(herbId, result.Herb!, result.Quantity);

                await _characterRepository.UpdateAsync(character);
            }

            return Ok(new GatheringApiResponse
            {
                CharacterName = character.Name,
                RollResults = rollResults,
                TotalSuccesses = successes,
                GatheredHerbs = gatheredHerbs
            });
        }
    }
}
