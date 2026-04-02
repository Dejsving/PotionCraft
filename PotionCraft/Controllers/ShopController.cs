using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Models;
using PotionCraft.Contracts.Services;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Controllers
{
    /// <summary>
    /// API-контроллер для работы с магазином: получение ассортимента и выполнение сделок.
    /// </summary>
    [ApiController]
    [Route("api/shop")]
    public class ShopController : ControllerBase
    {
        /// <summary>
        /// Репозиторий трав.
        /// </summary>
        private readonly IHerbRepository _herbRepository;

        /// <summary>
        /// Репозиторий персонажей.
        /// </summary>
        private readonly IPlayerCharacterRepository _characterRepository;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера магазина.
        /// </summary>
        public ShopController(IHerbRepository herbRepository, IPlayerCharacterRepository characterRepository)
        {
            _herbRepository = herbRepository;
            _characterRepository = characterRepository;
        }

        /// <summary>
        /// Возвращает текущий ассортимент магазина с ценами и количеством.
        /// </summary>
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
                BuyPrice = HerbPriceCalculator.GetBuyPrice(h.Rarity, 1),
                SellPrice = HerbPriceCalculator.GetSellPrice(h.Rarity, 1),
                AvailableQuantity = ShopInventoryGenerator.GetQuantityForShop(h.Rarity),
                HerbType = (int)h.HerbType
            })
            .OrderBy(i => i.Rarity)
            .ThenBy(i => i.Name)
            .ToList();

            return Ok(shopItems);
        }

        /// <summary>
        /// Выполняет сделку: покупка и/или продажа предметов между персонажем и торговцем.
        /// </summary>
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

            // Рассчитываем стоимость покупки (персонаж платит)
            double totalBuyCostGold = 0;
            foreach (var item in request.ItemsToBuy)
            {
                if (item.Quantity <= 0) continue;

                if (item.Category == "herb")
                {
                    if (!herbLookup.TryGetValue(item.ItemId, out var herb))
                    {
                        return BadRequest(new TradeResult
                        {
                            Success = false,
                            Message = $"Трава с ID {item.ItemId} не найдена."
                        });
                    }

                    totalBuyCostGold += HerbPriceCalculator.GetBuyPrice(herb.Rarity, item.Quantity) * item.Quantity;
                }
            }

            // Рассчитываем выручку от продажи (торговец платит)
            double totalSellRevenueGold = 0;
            foreach (var item in request.ItemsToSell)
            {
                if (item.Quantity <= 0) continue;

                if (item.Category == "herb")
                {
                    if (!herbLookup.TryGetValue(item.ItemId, out var herb))
                    {
                        return BadRequest(new TradeResult
                        {
                            Success = false,
                            Message = $"Трава с ID {item.ItemId} не найдена."
                        });
                    }

                    // Проверяем, что у персонажа есть достаточно товара
                    if (!character.Bag.Herbs.TryGetValue(item.ItemId, out var bagItem) || bagItem.Quantity < item.Quantity)
                    {
                        return BadRequest(new TradeResult
                        {
                            Success = false,
                            Message = $"Недостаточно '{herb.Name}' в сумке для продажи."
                        });
                    }

                    totalSellRevenueGold += HerbPriceCalculator.GetSellPrice(herb.Rarity, item.Quantity) * item.Quantity;
                }
            }

            // Итоговое изменение баланса в медных монетах
            int balanceChangeCopper = (int)Math.Round((totalSellRevenueGold - totalBuyCostGold) * 100);

            // Проверяем, хватает ли денег
            if (character.Bag.Coins + balanceChangeCopper < 0)
            {
                return BadRequest(new TradeResult
                {
                    Success = false,
                    Message = "Недостаточно средств для совершения сделки."
                });
            }

            // Применяем изменения: покупка
            foreach (var item in request.ItemsToBuy)
            {
                if (item.Quantity <= 0) continue;

                if (item.Category == "herb" && herbLookup.TryGetValue(item.ItemId, out var herb))
                {
                    if (character.Bag.Herbs.TryGetValue(item.ItemId, out var existing))
                    {
                        existing.Quantity += item.Quantity;
                    }
                    else
                    {
                        character.Bag.Herbs[item.ItemId] = new GatheringResult
                        {
                            Herb = herb,
                            Quantity = item.Quantity
                        };
                    }
                }
            }

            // Применяем изменения: продажа
            foreach (var item in request.ItemsToSell)
            {
                if (item.Quantity <= 0) continue;

                if (item.Category == "herb" && character.Bag.Herbs.TryGetValue(item.ItemId, out var bagItem))
                {
                    bagItem.Quantity -= item.Quantity;
                    if (bagItem.Quantity <= 0)
                    {
                        character.Bag.Herbs.Remove(item.ItemId);
                    }
                }
            }

            // Обновляем баланс
            character.Bag.Coins += balanceChangeCopper;

            await _characterRepository.UpdateAsync(character);

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
