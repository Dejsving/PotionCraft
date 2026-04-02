using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Services;

namespace PotionCraft.Tests.Services;

/// <summary>
/// Тесты для HerbPriceCalculator: проверка оптового ценообразования.
/// </summary>
public class HerbPriceCalculatorTests
{
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
        // Собираем по 50 замеров для каждого количества, чтобы усреднить случайный шум
        const int samples = 50;
        double avgPrice1 = 0, avgPrice10 = 0;

        for (int i = 0; i < samples; i++)
        {
            avgPrice1 += HerbPriceCalculator.GetSellPrice(rarity, 1);
            avgPrice10 += HerbPriceCalculator.GetSellPrice(rarity, 10);
        }

        avgPrice1 /= samples;
        avgPrice10 /= samples;

        Assert.True(avgPrice10 < avgPrice1,
            $"Средняя цена продажи за единицу при qty=10 ({avgPrice10:F2}) должна быть ниже, чем при qty=1 ({avgPrice1:F2}) для {rarity}");
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
        const int samples = 50;
        double avgPrice1 = 0, avgPrice10 = 0;

        for (int i = 0; i < samples; i++)
        {
            avgPrice1 += HerbPriceCalculator.GetBuyPrice(rarity, 1);
            avgPrice10 += HerbPriceCalculator.GetBuyPrice(rarity, 10);
        }

        avgPrice1 /= samples;
        avgPrice10 /= samples;

        Assert.True(avgPrice10 > avgPrice1,
            $"Средняя цена покупки за единицу при qty=10 ({avgPrice10:F2}) должна быть выше, чем при qty=1 ({avgPrice1:F2}) для {rarity}");
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
        for (int i = 0; i < 20; i++)
        {
            var price = HerbPriceCalculator.GetSellPrice(rarity, quantity);
            Assert.True(price > 0, $"Цена продажи должна быть положительной: {price}");
        }
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
        for (int i = 0; i < 20; i++)
        {
            var price = HerbPriceCalculator.GetBuyPrice(rarity, quantity);
            Assert.True(price > 0, $"Цена покупки должна быть положительной: {price}");
        }
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
        const int samples = 50;
        double avgBuy = 0, avgSell = 0;

        for (int i = 0; i < samples; i++)
        {
            avgBuy += HerbPriceCalculator.GetBuyPrice(rarity, 1);
            avgSell += HerbPriceCalculator.GetSellPrice(rarity, 1);
        }

        avgBuy /= samples;
        avgSell /= samples;

        Assert.True(avgBuy > avgSell,
            $"Средняя цена покупки ({avgBuy:F2}) должна быть выше средней цены продажи ({avgSell:F2}) для {rarity}");
    }

    /// <summary>
    /// Проверяет, что общая стоимость покупки оптом (qty * unitPrice(qty)) выше, чем qty * unitPrice(1),
    /// то есть оптовая покупка обходится дороже чем поштучная.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common, 5)]
    [InlineData(RarityEnum.Rare, 10)]
    public void GetBuyPrice_BulkTotalCost_HigherThanSingleUnitTotal(RarityEnum rarity, int quantity)
    {
        const int samples = 50;
        double avgBulkTotal = 0, avgSingleTotal = 0;

        for (int i = 0; i < samples; i++)
        {
            avgBulkTotal += HerbPriceCalculator.GetBuyPrice(rarity, quantity) * quantity;
            avgSingleTotal += HerbPriceCalculator.GetBuyPrice(rarity, 1) * quantity;
        }

        avgBulkTotal /= samples;
        avgSingleTotal /= samples;

        Assert.True(avgBulkTotal > avgSingleTotal,
            $"Оптовая стоимость покупки ({avgBulkTotal:F2}) должна быть выше поштучной ({avgSingleTotal:F2})");
    }

    /// <summary>
    /// Проверяет, что общая выручка от оптовой продажи (qty * unitPrice(qty)) ниже, чем qty * unitPrice(1),
    /// то есть оптовая продажа приносит меньше чем поштучная.
    /// </summary>
    [Theory]
    [InlineData(RarityEnum.Common, 5)]
    [InlineData(RarityEnum.Rare, 10)]
    public void GetSellPrice_BulkTotalRevenue_LowerThanSingleUnitTotal(RarityEnum rarity, int quantity)
    {
        const int samples = 50;
        double avgBulkTotal = 0, avgSingleTotal = 0;

        for (int i = 0; i < samples; i++)
        {
            avgBulkTotal += HerbPriceCalculator.GetSellPrice(rarity, quantity) * quantity;
            avgSingleTotal += HerbPriceCalculator.GetSellPrice(rarity, 1) * quantity;
        }

        avgBulkTotal /= samples;
        avgSingleTotal /= samples;

        Assert.True(avgBulkTotal < avgSingleTotal,
            $"Оптовая выручка ({avgBulkTotal:F2}) должна быть ниже поштучной ({avgSingleTotal:F2})");
    }
}
