using PotionCraft.Contracts.Models;

namespace PotionCraft.Tests.Contracts;

/// <summary>
/// Тесты для модели CharacterBag.
/// </summary>
public class CharacterBagTests
{
    /// <summary>
    /// Проверяет, что при создании CharacterBag словарь трав инициализирован как пустой.
    /// </summary>
    [Fact]
    public void NewCharacterBag_HerbsDictionary_IsEmptyNotNull()
    {
        var bag = new CharacterBag();

        Assert.NotNull(bag.Herbs);
        Assert.Empty(bag.Herbs);
    }

    /// <summary>
    /// Проверяет, что при создании CharacterBag словарь зелий инициализирован как пустой.
    /// </summary>
    [Fact]
    public void NewCharacterBag_PotionsDictionary_IsEmptyNotNull()
    {
        var bag = new CharacterBag();

        Assert.NotNull(bag.Potions);
        Assert.Empty(bag.Potions);
    }

    /// <summary>
    /// Проверяет, что при создании CharacterBag словарь ядов инициализирован как пустой.
    /// </summary>
    [Fact]
    public void NewCharacterBag_PoisonsDictionary_IsEmptyNotNull()
    {
        var bag = new CharacterBag();

        Assert.NotNull(bag.Poisons);
        Assert.Empty(bag.Poisons);
    }

    /// <summary>
    /// Проверяет, что при создании CharacterBag генерируется уникальный идентификатор.
    /// </summary>
    [Fact]
    public void NewCharacterBag_Id_IsNotEmpty()
    {
        var bag = new CharacterBag();

        Assert.NotEqual(Guid.Empty, bag.Id);
    }

    /// <summary>
    /// Проверяет, что в словарь трав можно добавить элемент.
    /// </summary>
    [Fact]
    public void CharacterBag_AddHerb_StoresGatheringResult()
    {
        var bag = new CharacterBag();
        var herb = new Herb { Id = Guid.NewGuid(), Name = "Тысячелистник" };
        var result = new GatheringResult { Herb = herb, Quantity = 3 };

        bag.Herbs.Add(herb.Id, result);

        Assert.Single(bag.Herbs);
        Assert.Equal(3, bag.Herbs[herb.Id].Quantity);
        Assert.Equal("Тысячелистник", bag.Herbs[herb.Id].Herb!.Name);
    }

    /// <summary>
    /// Проверяет, что в словарь зелий можно добавить элемент.
    /// </summary>
    [Fact]
    public void CharacterBag_AddPotion_StoresPotionBagItem()
    {
        var bag = new CharacterBag();
        var recipe = new AlchemyRecipe { Name = "Зелье лечения" };
        var potion = new Potion { Id = Guid.NewGuid(), Name = "Зелье лечения", Recipe = recipe };
        var item = new PotionBagItem { Potion = potion, Quantity = 2 };

        bag.Potions.Add(potion.Id, item);

        Assert.Single(bag.Potions);
        Assert.Equal(2, bag.Potions[potion.Id].Quantity);
    }

    /// <summary>
    /// Проверяет, что в словарь ядов можно добавить элемент.
    /// </summary>
    [Fact]
    public void CharacterBag_AddPoison_StoresPotionBagItem()
    {
        var bag = new CharacterBag();
        var recipe = new AlchemyRecipe { Name = "Яд паука" };
        var potion = new Potion { Id = Guid.NewGuid(), Name = "Яд паука", Recipe = recipe };
        var item = new PotionBagItem { Potion = potion, Quantity = 1 };

        bag.Poisons.Add(potion.Id, item);

        Assert.Single(bag.Poisons);
        Assert.Equal(1, bag.Poisons[potion.Id].Quantity);
    }

    /// <summary>
    /// Проверяет, что сумма из монет корректно распределяется на золотые, серебряные и медные монеты.
    /// </summary>
    [Theory]
    [InlineData(1234, 12, 3, 4)]
    [InlineData(8, 0, 0, 8)]
    [InlineData(50, 0, 5, 0)]
    [InlineData(100, 1, 0, 0)]
    [InlineData(0, 0, 0, 0)]
    public void CharacterBag_Coins_SplitsIntoGoldSilverCopperCorrectly(int totalCoins, int expectedGold, int expectedSilver, int expectedCopper)
    {
        var bag = new CharacterBag { Coins = totalCoins };

        Assert.Equal(expectedGold, bag.GoldCoins);
        Assert.Equal(expectedSilver, bag.SilverCoins);
        Assert.Equal(expectedCopper, bag.CopperCoins);
    }

    /// <summary>
    /// Проверяет, что AddOrUpdateHerb добавляет новую траву в пустую сумку.
    /// </summary>
    [Fact]
    public void AddOrUpdateHerb_NewHerb_AddsToEmptyBag()
    {
        var bag = new CharacterBag();
        var herb = new Herb { Id = Guid.NewGuid(), Name = "Лаванда" };

        bag.AddOrUpdateHerb(herb.Id, herb, 5);

        Assert.Single(bag.Herbs);
        Assert.Equal(5, bag.Herbs[herb.Id].Quantity);
        Assert.Equal("Лаванда", bag.Herbs[herb.Id].Herb!.Name);
    }

    /// <summary>
    /// Проверяет, что AddOrUpdateHerb увеличивает количество уже существующей травы.
    /// </summary>
    [Fact]
    public void AddOrUpdateHerb_ExistingHerb_IncreasesQuantity()
    {
        var bag = new CharacterBag();
        var herb = new Herb { Id = Guid.NewGuid(), Name = "Ромашка" };
        bag.Herbs[herb.Id] = new GatheringResult { Herb = herb, Quantity = 3 };

        bag.AddOrUpdateHerb(herb.Id, herb, 2);

        Assert.Single(bag.Herbs);
        Assert.Equal(5, bag.Herbs[herb.Id].Quantity);
    }

    /// <summary>
    /// Проверяет, что AddOrUpdateHerb корректно работает при добавлении нескольких разных трав.
    /// </summary>
    [Fact]
    public void AddOrUpdateHerb_MultipleDifferentHerbs_AddsAll()
    {
        var bag = new CharacterBag();
        var herb1 = new Herb { Id = Guid.NewGuid(), Name = "Мята" };
        var herb2 = new Herb { Id = Guid.NewGuid(), Name = "Шалфей" };

        bag.AddOrUpdateHerb(herb1.Id, herb1, 1);
        bag.AddOrUpdateHerb(herb2.Id, herb2, 4);

        Assert.Equal(2, bag.Herbs.Count);
        Assert.Equal(1, bag.Herbs[herb1.Id].Quantity);
        Assert.Equal(4, bag.Herbs[herb2.Id].Quantity);
    }
}
