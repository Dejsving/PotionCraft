using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Server.Controllers
{
    [ApiController]
    [Route("api/herbs")]
    public class HerbsController : ControllerBase
    {
        private readonly IHerbRepository _herbRepository;

        public HerbsController(IHerbRepository herbRepository)
        {
            _herbRepository = herbRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var herbs = await _herbRepository.GetAllAsync();
            return Ok(herbs);
        }
    }
}
