using PotionCraft.Contracts;
using PotionCraft.Contracts.Models;

namespace PotionCraft.Tests.Contracts;

/// <summary>
/// Тесты для проверки инициализации сумки при создании персонажа.
/// </summary>
public class PlayerCharacterBagTests
{
    /// <summary>
    /// Проверяет, что сумка персонажа создаётся автоматически и не равна null.
    /// </summary>
    [Fact]
    public void NewPlayerCharacter_Bag_IsNotNull()
    {
        var character = new PlayerCharacter();

        Assert.NotNull(character.Bag);
    }

    /// <summary>
    /// Проверяет, что сумка нового персонажа содержит пустой словарь трав.
    /// </summary>
    [Fact]
    public void NewPlayerCharacter_BagHerbs_IsEmptyNotNull()
    {
        var character = new PlayerCharacter();

        Assert.NotNull(character.Bag.Herbs);
        Assert.Empty(character.Bag.Herbs);
    }

    /// <summary>
    /// Проверяет, что сумка нового персонажа содержит пустой словарь зелий.
    /// </summary>
    [Fact]
    public void NewPlayerCharacter_BagPotions_IsEmptyNotNull()
    {
        var character = new PlayerCharacter();

        Assert.NotNull(character.Bag.Potions);
        Assert.Empty(character.Bag.Potions);
    }

    /// <summary>
    /// Проверяет, что сумка нового персонажа содержит пустой словарь ядов.
    /// </summary>
    [Fact]
    public void NewPlayerCharacter_BagPoisons_IsEmptyNotNull()
    {
        var character = new PlayerCharacter();

        Assert.NotNull(character.Bag.Poisons);
        Assert.Empty(character.Bag.Poisons);
    }
}
