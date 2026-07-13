using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Server.Controllers
{
    [ApiController]
    [Route("api/shop")]
    public class ShopController : ControllerBase
    {
        private readonly IHerbRepository _herbRepository;
        private readonly IPlayerCharacterRepository _characterRepository;
        private readonly IPriceCalculator _priceCalculator;
        private readonly IInventoryGenerator _inventoryGenerator;
        private readonly PotionCraftDbContext _dbContext;

        public ShopController(IHerbRepository herbRepository, IPlayerCharacterRepository characterRepository,
            IPriceCalculator priceCalculator, IInventoryGenerator inventoryGenerator, PotionCraftDbContext dbContext)
        {
            _herbRepository = herbRepository;
            _characterRepository = characterRepository;
            _priceCalculator = priceCalculator;
            _inventoryGenerator = inventoryGenerator;
            _dbContext = dbContext;
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var herbs = await _herbRepository.GetAllAsync();

            var shopItems = herbs.Select(h => new ShopItem
            {
                Id = h.Id,
                Name = h.Name,
                Category = "herb",
                Rarity = (int)h.Rarity,
                RarityName = h.Rarity.GetDisplayName(),
                BuyPrice = _priceCalculator.GetBuyPrice(h.Rarity, 1),
                SellPrice = _priceCalculator.GetSellPrice(h.Rarity, 1),
                AvailableQuantity = _inventoryGenerator.GetQuantityForShop(h.Rarity),
                HerbType = (int)h.HerbType
            })
            .OrderBy(i => i.Rarity)
            .ThenBy(i => i.Name)
            .ToList();

            return Ok(shopItems);
        }

        [HttpPost("trade")]
        public async Task<IActionResult> ExecuteTrade([FromBody] TradeRequest request)
        {
            if (request.ItemsToBuy.Count == 0 && request.ItemsToSell.Count == 0)
            {
                return BadRequest(new TradeResult
                {
                    Success = false,
                    Message = "Сделка пуста — нечего покупать или продавать."
                });
            }

            var invalidItem = request.ItemsToBuy.Concat(request.ItemsToSell)
                .FirstOrDefault(i => i.Quantity <= 0);
            if (invalidItem != null)
            {
                return BadRequest(new TradeResult
                {
                    Success = false,
                    Message = $"Количество товара должно быть положительным. Получено: {invalidItem.Quantity}."
                });
            }

            var character = await _characterRepository.GetByIdAsync(request.CharacterId);
            if (character == null)
            {
                return NotFound(new TradeResult
                {
                    Success = false,
                    Message = "Персонаж не найден."
                });
            }

            var allHerbs = await _herbRepository.GetAllAsync();
            var herbLookup = allHerbs.ToDictionary(h => h.Id);

            double totalBuyCostGold = 0;
            foreach (var item in request.ItemsToBuy)
            {
                if (item.Category == "herb")
                {
                    if (!herbLookup.TryGetValue(item.ItemId, out var herb))
                        return BadRequest(new TradeResult { Success = false, Message = $"Трава с ID {item.ItemId} не найдена." });
                    totalBuyCostGold += _priceCalculator.GetBuyPrice(herb.Rarity, item.Quantity) * item.Quantity;
                }
            }

            double totalSellRevenueGold = 0;
            foreach (var item in request.ItemsToSell)
            {
                if (item.Category == "herb")
                {
                    if (!herbLookup.TryGetValue(item.ItemId, out var herb))
                        return BadRequest(new TradeResult { Success = false, Message = $"Трава с ID {item.ItemId} не найдена." });
                    if (!character.Bag.Herbs.TryGetValue(item.ItemId, out var bagItem) || bagItem.Quantity < item.Quantity)
                        return BadRequest(new TradeResult { Success = false, Message = $"Недостаточно '{herb.Name}' в сумке для продажи." });
                    totalSellRevenueGold += _priceCalculator.GetSellPrice(herb.Rarity, item.Quantity) * item.Quantity;
                }
            }

            int balanceChangeCopper = (int)Math.Round((totalSellRevenueGold - totalBuyCostGold) * 100);

            if (character.Bag.Coins + balanceChangeCopper < 0)
                return BadRequest(new TradeResult { Success = false, Message = "Недостаточно средств для совершения сделки." });

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.ItemsToBuy)
                {
                    if (item.Category == "herb" && herbLookup.TryGetValue(item.ItemId, out var herb))
                        character.Bag.AddOrUpdateHerb(item.ItemId, herb, item.Quantity);
                }

                foreach (var item in request.ItemsToSell)
                {
                    if (item.Category == "herb" && character.Bag.Herbs.TryGetValue(item.ItemId, out var bagItem))
                    {
                        bagItem.Quantity -= item.Quantity;
                        if (bagItem.Quantity <= 0)
                            character.Bag.Herbs.Remove(item.ItemId);
                    }
                }

                character.Bag.Coins += balanceChangeCopper;
                await _characterRepository.UpdateAsync(character);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Ok(new TradeResult
            {
                Success = true,
                BalanceChange = balanceChangeCopper,
                Message = "Сделка успешна.",
                UpdatedBag = character.Bag
            });
        }
    }
}
