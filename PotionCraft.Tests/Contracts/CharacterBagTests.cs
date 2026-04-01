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
}
