using Moq;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Services;

namespace PotionCraft.Tests.Services;

/// <summary>
/// Тесты для HerbPriceCalculator: проверка оптового ценообразования.
/// </summary>
public class HerbPriceCalculatorTests
{
    /// <summary>
    /// Мок сервиса бросков кубиков.
    /// </summary>
    private readonly Mock<IDiceRoller> _diceRollerMock;

    /// <summary>
    /// Экземпляр калькулятора цен для тестирования.
    /// </summary>
    private readonly HerbPriceCalculator _calculator;

    public HerbPriceCalculatorTests()
    {
        _diceRollerMock = new Mock<IDiceRoller>();
        // По умолчанию шум = 0 (NextDouble = 0.5 даёт noise = 1.0)
        _diceRollerMock.Setup(d => d.NextDouble()).Returns(0.5);
        _calculator = new HerbPriceCalculator(_diceRollerMock.Object);
    }

    /// <summary>
    /// Проверяет, что цена продажи за единицу снижается при увеличении количества.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common)]
    [InlineData(RarityEnum.Unusual)]
    [InlineData(RarityEnum.Rare)]
    [InlineData(RarityEnum.VeryRare)]
    public void GetSellPrice_HigherQuantity_LowerUnitPrice(RarityEnum rarity)
    {
        var price1 = _calculator.GetSellPrice(rarity, 1);
        var price10 = _calculator.GetSellPrice(rarity, 10);

        Assert.True(price10 < price1,
            $"Цена продажи за единицу при qty=10 ({price10:F2}) должна быть ниже, чем при qty=1 ({price1:F2}) для {rarity}");
    }

    /// <summary>
    /// Проверяет, что цена покупки за единицу растёт при увеличении количества.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common)]
    [InlineData(RarityEnum.Unusual)]
    [InlineData(RarityEnum.Rare)]
    [InlineData(RarityEnum.VeryRare)]
    public void GetBuyPrice_HigherQuantity_HigherUnitPrice(RarityEnum rarity)
    {
        var price1 = _calculator.GetBuyPrice(rarity, 1);
        var price10 = _calculator.GetBuyPrice(rarity, 10);

        Assert.True(price10 > price1,
            $"Цена покупки за единицу при qty=10 ({price10:F2}) должна быть выше, чем при qty=1 ({price1:F2}) для {rarity}");
    }

    /// <summary>
    /// Проверяет, что цена продажи всегда положительна.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common, 1)]
    [InlineData(RarityEnum.Common, 50)]
    [InlineData(RarityEnum.VeryRare, 1)]
    [InlineData(RarityEnum.VeryRare, 100)]
    public void GetSellPrice_AlwaysPositive(RarityEnum rarity, int quantity)
    {
        var price = _calculator.GetSellPrice(rarity, quantity);
        Assert.True(price > 0, $"Цена продажи должна быть положительной: {price}");
    }

    /// <summary>
    /// Проверяет, что цена покупки всегда положительна.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common, 1)]
    [InlineData(RarityEnum.Common, 50)]
    [InlineData(RarityEnum.VeryRare, 1)]
    [InlineData(RarityEnum.VeryRare, 100)]
    public void GetBuyPrice_AlwaysPositive(RarityEnum rarity, int quantity)
    {
        var price = _calculator.GetBuyPrice(rarity, quantity);
        Assert.True(price > 0, $"Цена покупки должна быть положительной: {price}");
    }

    /// <summary>
    /// Проверяет, что цена покупки всегда выше цены продажи (маржа торговца).
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common)]
    [InlineData(RarityEnum.Unusual)]
    [InlineData(RarityEnum.Rare)]
    [InlineData(RarityEnum.VeryRare)]
    public void BuyPrice_AlwaysHigherThanSellPrice_AtSameQuantity(RarityEnum rarity)
    {
        var buyPrice = _calculator.GetBuyPrice(rarity, 1);
        var sellPrice = _calculator.GetSellPrice(rarity, 1);

        Assert.True(buyPrice > sellPrice,
            $"Цена покупки ({buyPrice:F2}) должна быть выше цены продажи ({sellPrice:F2}) для {rarity}");
    }

    /// <summary>
    /// Проверяет, что общая стоимость покупки оптом выше, чем поштучная.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common, 5)]
    [InlineData(RarityEnum.Rare, 10)]
    public void GetBuyPrice_BulkTotalCost_HigherThanSingleUnitTotal(RarityEnum rarity, int quantity)
    {
        var bulkTotal = _calculator.GetBuyPrice(rarity, quantity) * quantity;
        var singleTotal = _calculator.GetBuyPrice(rarity, 1) * quantity;

        Assert.True(bulkTotal > singleTotal,
            $"Оптовая стоимость покупки ({bulkTotal:F2}) должна быть выше поштучной ({singleTotal:F2})");
    }

    /// <summary>
    /// Проверяет, что общая выручка от оптовой продажи ниже, чем поштучная.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common, 5)]
    [InlineData(RarityEnum.Rare, 10)]
    public void GetSellPrice_BulkTotalRevenue_LowerThanSingleUnitTotal(RarityEnum rarity, int quantity)
    {
        var bulkTotal = _calculator.GetSellPrice(rarity, quantity) * quantity;
        var singleTotal = _calculator.GetSellPrice(rarity, 1) * quantity;

        Assert.True(bulkTotal < singleTotal,
            $"Оптовая выручка ({bulkTotal:F2}) должна быть ниже поштучной ({singleTotal:F2})");
    }

    /// <summary>
    /// Проверяет, что шум корректно влияет на цену при разных значениях NextDouble.
    /// </summary>
    [Fact]
    public void GetSellPrice_WithDifferentNoise_ProducesDifferentPrices()
    {
        var mockLow = new Mock<IDiceRoller>();
        mockLow.Setup(d => d.NextDouble()).Returns(0.0); // минимальный шум
        var calcLow = new HerbPriceCalculator(mockLow.Object);

        var mockHigh = new Mock<IDiceRoller>();
        mockHigh.Setup(d => d.NextDouble()).Returns(1.0); // максимальный шум
        var calcHigh = new HerbPriceCalculator(mockHigh.Object);

        var priceLow = calcLow.GetSellPrice(RarityEnum.Common, 1);
        var priceHigh = calcHigh.GetSellPrice(RarityEnum.Common, 1);

        Assert.True(priceLow < priceHigh,
            $"Цена с минимальным шумом ({priceLow:F2}) должна быть ниже цены с максимальным ({priceHigh:F2})");
    }
}
