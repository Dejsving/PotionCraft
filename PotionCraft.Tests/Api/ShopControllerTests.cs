using Microsoft.AspNetCore.Mvc;
using Moq;
using PotionCraft.Contracts;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Controllers;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Tests.Api;

/// <summary>
/// Тесты для ShopController (ассортимент и сделки).
/// </summary>
public class ShopControllerTests
{
    /// <summary>Макет репозитория трав.</summary>
    private readonly Mock<IHerbRepository> _mockHerbRepo;

    /// <summary>Макет репозитория персонажей.</summary>
    private readonly Mock<IPlayerCharacterRepository> _mockCharRepo;

    /// <summary>Тестируемый контроллер.</summary>
    private readonly ShopController _controller;

    /// <summary>
    /// Инициализирует тестовое окружение.
    /// </summary>
    public ShopControllerTests()
    {
        _mockHerbRepo = new Mock<IHerbRepository>();
        _mockCharRepo = new Mock<IPlayerCharacterRepository>();
        _controller = new ShopController(_mockHerbRepo.Object, _mockCharRepo.Object);
    }

    /// <summary>
    /// Создаёт тестовую траву с заданной редкостью.
    /// </summary>
    private static Herb CreateHerb(string name, RarityEnum rarity)
    {
        return new Herb
        {
            Id = Guid.NewGuid(),
            Name = name,
            Rarity = rarity,
            HerbType = HerbTypeEnum.HealingBase,
            Description = "Тестовое описание",
            Effect = "Тестовый эффект",
            Difficulty = 10
        };
    }

    /// <summary>
    /// Создаёт тестового персонажа с заданным балансом.
    /// </summary>
    private static PlayerCharacter CreateCharacter(int coins = 10000)
    {
        return new PlayerCharacter
        {
            Id = Guid.NewGuid(),
            Name = "Тестовый Герой",
            Intelligence = 14,
            Wisdom = 12,
            ProficiencyBonus = 2,
            Bag = new CharacterBag { Coins = coins }
        };
    }

    // ─── GetInventory ─────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что GetInventory возвращает 200 OK со списком товаров.
    /// </summary>
    [Fact]
    public async Task GetInventory_ReturnsOkWithShopItems()
    {
        var herbs = new List<Herb>
        {
            CreateHerb("Тысячелистник", RarityEnum.Common),
            CreateHerb("Редкий корень", RarityEnum.Rare)
        };
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);

        var result = await _controller.GetInventory();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<List<ShopItem>>(okResult.Value);
        // Все травы возвращаются (включая те, у которых AvailableQuantity == 0)
        Assert.Equal(2, items.Count);
        Assert.All(items, item =>
        {
            Assert.True(item.BuyPrice > 0);
            Assert.True(item.SellPrice > 0);
            Assert.Equal("herb", item.Category);
        });
    }

    /// <summary>
    /// Проверяет, что GetInventory возвращает пустой список, если трав нет.
    /// </summary>
    [Fact]
    public async Task GetInventory_NoHerbs_ReturnsEmptyList()
    {
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb>());

        var result = await _controller.GetInventory();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<List<ShopItem>>(okResult.Value);
        Assert.Empty(items);
    }

    /// <summary>
    /// Проверяет, что предметы с нулевым количеством всё равно возвращаются с корректными ценами.
    /// </summary>
    [Fact]
    public async Task GetInventory_ZeroQuantityItems_StillHavePrices()
    {
        var herb = CreateHerb("Стебли Гифломы", RarityEnum.VeryRare);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        // Вызываем много раз — VeryRare имеет 1% шанс появления, поэтому
        // хотя бы в одном вызове AvailableQuantity будет 0
        bool foundZeroQuantityWithPrice = false;
        for (int i = 0; i < 200; i++)
        {
            var result = await _controller.GetInventory();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<List<ShopItem>>(okResult.Value);

            // Всегда должен возвращаться 1 элемент, даже если количество = 0
            Assert.Single(items);
            var item = items[0];
            Assert.True(item.SellPrice > 0, "Цена продажи должна быть больше 0 даже при нулевом количестве");
            Assert.True(item.BuyPrice > 0, "Цена покупки должна быть больше 0 даже при нулевом количестве");

            if (item.AvailableQuantity == 0)
                foundZeroQuantityWithPrice = true;
        }

        Assert.True(foundZeroQuantityWithPrice, "За 200 попыток должен был встретиться предмет с AvailableQuantity == 0");
    }

    /// <summary>
    /// Проверяет, что все товары в ассортименте имеют названия и корректные редкости.
    /// </summary>
    [Fact]
    public async Task GetInventory_ItemsHaveCorrectFields()
    {
        var herb = CreateHerb("Лунник", RarityEnum.Unusual);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        var result = await _controller.GetInventory();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<List<ShopItem>>(okResult.Value);

        Assert.Single(items);
        var item = items[0];
        Assert.Equal(herb.Id, item.Id);
        Assert.Equal("Лунник", item.Name);
        Assert.Equal(1, item.Rarity);
        Assert.Equal("Необычный", item.RarityName);
        Assert.Equal("herb", item.Category);
    }

    // ─── ExecuteTrade ─────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что пустая сделка возвращает 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_EmptyTrade_Returns400()
    {
        var request = new TradeRequest
        {
            CharacterId = Guid.NewGuid(),
            ItemsToBuy = new List<TradeItem>(),
            ItemsToSell = new List<TradeItem>()
        };

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
    }

    /// <summary>
    /// Проверяет, что сделка с несуществующим персонажем возвращает 404.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_CharacterNotFound_Returns404()
    {
        var request = new TradeRequest
        {
            CharacterId = Guid.NewGuid(),
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = Guid.NewGuid(), Quantity = 1, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(request.CharacterId))
            .ReturnsAsync((PlayerCharacter?)null);

        var result = await _controller.ExecuteTrade(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что покупка несуществующей травы возвращает 400.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_BuyNonExistentHerb_Returns400()
    {
        var character = CreateCharacter(10000);
        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = Guid.NewGuid(), Quantity = 1, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb>());

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
    }

    /// <summary>
    /// Проверяет, что успешная покупка добавляет траву в сумку и уменьшает баланс.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_BuyHerb_AddsToCharacterBag()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(100000); // 1000 золотых = 100000 медных

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 2, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.True(tradeResult.Success);
        Assert.True(character.Bag.Herbs.ContainsKey(herb.Id));
        Assert.Equal(2, character.Bag.Herbs[herb.Id].Quantity);
        Assert.True(character.Bag.Coins < 100000); // Баланс уменьшился
    }

    /// <summary>
    /// Проверяет, что покупка при существующих травах в сумке увеличивает количество.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_BuyHerb_IncreasesExistingQuantity()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(100000);
        character.Bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 3 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 2, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.True(tradeResult.Success);
        Assert.Equal(5, character.Bag.Herbs[herb.Id].Quantity);
    }

    /// <summary>
    /// Проверяет, что продажа травы уменьшает количество в сумке и увеличивает баланс.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_SellHerb_RemovesFromBagAndAddsCoins()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(0);
        character.Bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 5 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>(),
            ItemsToSell = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 3, Category = "herb" }
            }
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.True(tradeResult.Success);
        Assert.Equal(2, character.Bag.Herbs[herb.Id].Quantity);
        Assert.True(character.Bag.Coins > 0); // Баланс увеличился
        Assert.True(tradeResult.BalanceChange > 0); // Доход
    }

    /// <summary>
    /// Проверяет, что продажа всего количества полностью удаляет траву из сумки.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_SellAllOfHerb_RemovesEntryFromBag()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(0);
        character.Bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 2 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>(),
            ItemsToSell = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 2, Category = "herb" }
            }
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.True(tradeResult.Success);
        Assert.False(character.Bag.Herbs.ContainsKey(herb.Id));
    }

    /// <summary>
    /// Проверяет, что продажа больше чем есть в сумке возвращает 400.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_SellMoreThanOwned_Returns400()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(0);
        character.Bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 1 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>(),
            ItemsToSell = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 5, Category = "herb" }
            }
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
    }

    /// <summary>
    /// Проверяет, что покупка при недостаточном балансе возвращает 400.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_InsufficientFunds_Returns400()
    {
        var herb = CreateHerb("Дорогая трава", RarityEnum.VeryRare);
        var character = CreateCharacter(1); // Только 1 медная монета

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 1, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
    }

    /// <summary>
    /// Проверяет, что бартер (покупка и продажа одновременно) работает корректно.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_BarterBuyAndSell_WorksCorrectly()
    {
        var herbToBuy = CreateHerb("Дорогой цветок", RarityEnum.Unusual);
        var herbToSell = CreateHerb("Обычная трава", RarityEnum.Common);

        var character = CreateCharacter(50000); // 500 золотых
        character.Bag.Herbs[herbToSell.Id] = new GatheringResult { Herb = herbToSell, Quantity = 10 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herbToBuy.Id, Quantity = 1, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>
            {
                new TradeItem { ItemId = herbToSell.Id, Quantity = 5, Category = "herb" }
            }
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herbToBuy, herbToSell });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.True(tradeResult.Success);
        Assert.True(character.Bag.Herbs.ContainsKey(herbToBuy.Id));
        Assert.Equal(1, character.Bag.Herbs[herbToBuy.Id].Quantity);
        Assert.Equal(5, character.Bag.Herbs[herbToSell.Id].Quantity);

        _mockCharRepo.Verify(r => r.UpdateAsync(character), Times.Once);
    }

    /// <summary>
    /// Проверяет, что после успешной сделки вызывается UpdateAsync.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_Success_CallsUpdateAsync()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(100000);

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 1, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        await _controller.ExecuteTrade(request);

        _mockCharRepo.Verify(r => r.UpdateAsync(character), Times.Once);
    }

    /// <summary>
    /// Проверяет, что после успешной покупки UpdatedBag в ответе содержит актуальное состояние сумки.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_BuyHerb_UpdatedBagReflectsDbState()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(100000);

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 3, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.NotNull(tradeResult.UpdatedBag);
        Assert.True(tradeResult.UpdatedBag.Herbs.ContainsKey(herb.Id));
        Assert.Equal(3, tradeResult.UpdatedBag.Herbs[herb.Id].Quantity);
        Assert.Equal(character.Bag.Coins, tradeResult.UpdatedBag.Coins);
    }

    /// <summary>
    /// Проверяет, что после успешной продажи UpdatedBag в ответе содержит обновлённый баланс и уменьшенное количество трав.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_SellHerb_UpdatedBagReflectsDbState()
    {
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        var character = CreateCharacter(0);
        character.Bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 5 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>(),
            ItemsToSell = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 2, Category = "herb" }
            }
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });
        _mockCharRepo.Setup(r => r.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

        var result = await _controller.ExecuteTrade(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(okResult.Value);
        Assert.NotNull(tradeResult.UpdatedBag);
        Assert.Equal(3, tradeResult.UpdatedBag.Herbs[herb.Id].Quantity);
        Assert.True(tradeResult.UpdatedBag.Coins > 0);
        Assert.Equal(character.Bag.Coins, tradeResult.UpdatedBag.Coins);
    }

    /// <summary>
    /// Проверяет, что покупка с отрицательным количеством возвращает 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_NegativeQuantityInBuy_Returns400()
    {
        var character = CreateCharacter(100000);
        var herb = CreateHerb("Зверобой", RarityEnum.Common);

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = -3, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
        Assert.Contains("-3", tradeResult.Message);
    }

    /// <summary>
    /// Проверяет, что продажа с отрицательным количеством возвращает 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_NegativeQuantityInSell_Returns400()
    {
        var character = CreateCharacter(0);
        var herb = CreateHerb("Зверобой", RarityEnum.Common);
        character.Bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 5 };

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>(),
            ItemsToSell = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = -1, Category = "herb" }
            }
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
    }

    /// <summary>
    /// Проверяет, что покупка с нулевым количеством возвращает 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task ExecuteTrade_ZeroQuantity_Returns400()
    {
        var character = CreateCharacter(100000);
        var herb = CreateHerb("Зверобой", RarityEnum.Common);

        var request = new TradeRequest
        {
            CharacterId = character.Id,
            ItemsToBuy = new List<TradeItem>
            {
                new TradeItem { ItemId = herb.Id, Quantity = 0, Category = "herb" }
            },
            ItemsToSell = new List<TradeItem>()
        };

        _mockCharRepo.Setup(r => r.GetByIdAsync(character.Id)).ReturnsAsync(character);
        _mockHerbRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Herb> { herb });

        var result = await _controller.ExecuteTrade(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var tradeResult = Assert.IsType<TradeResult>(badResult.Value);
        Assert.False(tradeResult.Success);
    }
}
