using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;

namespace PotionCraft.Tests.Contracts;

/// <summary>
/// Тесты для методов расширения EnumExtensions.
/// </summary>
public class EnumExtensionsTests
{
    /// <summary>
    /// Проверяет, что GetDisplayName возвращает значение DisplayAttribute, если атрибут задан.
    /// </summary>
    [Fact]
    public void GetDisplayName_WithDisplayAttribute_ReturnsDisplayName()
    {
        var result = RarityEnum.Common.GetDisplayName();

        Assert.Equal("Обычный", result);
    }

    /// <summary>
    /// Проверяет, что GetDisplayName возвращает ToString() для enum-значения без DisplayAttribute.
    /// </summary>
    [Fact]
    public void GetDisplayName_WithoutDisplayAttribute_ReturnsFallbackToString()
    {
        var result = EnumWithoutDisplay.ValueA.GetDisplayName();

        Assert.Equal("ValueA", result);
    }
}

/// <summary>
/// Вспомогательный enum без DisplayAttribute для тестирования fallback-поведения.
/// </summary>
public enum EnumWithoutDisplay
{
    ValueA,
    ValueB
}
