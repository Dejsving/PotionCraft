using Moq;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Services;

namespace PotionCraft.Tests.Services;

/// <summary>
/// Тесты для ShopInventoryGenerator с мокированным IDiceRoller.
/// </summary>
public class ShopInventoryGeneratorTests
{
    /// <summary>
    /// Мок сервиса бросков кубиков.
    /// </summary>
    private readonly Mock<IDiceRoller> _diceRollerMock;

    /// <summary>
    /// Экземпляр генератора инвентаря для тестирования.
    /// </summary>
    private readonly ShopInventoryGenerator _generator;

    public ShopInventoryGeneratorTests()
    {
        _diceRollerMock = new Mock<IDiceRoller>();
        _generator = new ShopInventoryGenerator(_diceRollerMock.Object);
    }

    /// <summary>
    /// Проверяет, что товар появляется при удачном броске на появление.
    /// </summary>
    [Fact]
    public void GetQuantityForShop_SpawnChancePassed_ReturnsQuantity()
    {
        // Common: шанс 0.5, NextDouble вернёт 0.3 (проходит)
        _diceRollerMock.Setup(d => d.NextDouble()).Returns(0.3);
        _diceRollerMock.Setup(d => d.Next(15, 41)).Returns(25);

        var quantity = _generator.GetQuantityForShop(RarityEnum.Common);

        Assert.Equal(25, quantity);
    }

    /// <summary>
    /// Проверяет, что товар не появляется при неудачном броске на появление.
    /// </summary>
    [Fact]
    public void GetQuantityForShop_SpawnChanceFailed_ReturnsZero()
    {
        // Common: шанс 0.5, NextDouble вернёт 0.7 (не проходит)
        _diceRollerMock.Setup(d => d.NextDouble()).Returns(0.7);

        var quantity = _generator.GetQuantityForShop(RarityEnum.Common);

        Assert.Equal(0, quantity);
    }

    /// <summary>
    /// Проверяет, что для VeryRare товара шанс появления очень мал.
    /// </summary>
    [Fact]
    public void GetQuantityForShop_VeryRare_SpawnChancePassed_ReturnsOne()
    {
        // VeryRare: шанс 0.01, NextDouble вернёт 0.005 (проходит)
        _diceRollerMock.Setup(d => d.NextDouble()).Returns(0.005);
        _diceRollerMock.Setup(d => d.Next(1, 2)).Returns(1);

        var quantity = _generator.GetQuantityForShop(RarityEnum.VeryRare);

        Assert.Equal(1, quantity);
    }

    /// <summary>
    /// Проверяет, что для VeryRare товара при неудачном броске возвращается 0.
    /// </summary>
    [Fact]
    public void GetQuantityForShop_VeryRare_SpawnChanceFailed_ReturnsZero()
    {
        // VeryRare: шанс 0.01, NextDouble вернёт 0.5 (не проходит)
        _diceRollerMock.Setup(d => d.NextDouble()).Returns(0.5);

        var quantity = _generator.GetQuantityForShop(RarityEnum.VeryRare);

        Assert.Equal(0, quantity);
    }
}
