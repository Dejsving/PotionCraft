using PotionCraft.Contracts;

namespace PotionCraft.Tests.Contracts;

/// <summary>
/// Тесты для модели Tool и связанных вычислений персонажа.
/// </summary>
public class ToolTests
{
    /// <summary>
    /// Проверяет, что Tool.GetModify добавляет модификатор инструмента к модификатору характеристики.
    /// </summary>
    [Fact]
    public void GetModify_ReturnsAbilityModifierWithToolModifier()
    {
        var tool = new Tool
        {
            Modifier = 3
        };

        var result = tool.GetModify(2);

        Assert.Equal(5, result);
    }

    /// <summary>
    /// Проверяет, что Tool.GetModify поддерживает отрицательный модификатор инструмента.
    /// </summary>
    [Fact]
    public void GetModify_WithNegativeToolModifier_ReturnsReducedValue()
    {
        var tool = new Tool
        {
            Modifier = -2
        };

        var result = tool.GetModify(1);

        Assert.Equal(-1, result);
    }

    /// <summary>
    /// Проверяет, что инструмент персонажа использует модификатор характеристики в расчёте.
    /// </summary>
    [Fact]
    public void PlayerCharacterTool_GetModify_UsesCharacterAbilityModifier()
    {
        var character = new PlayerCharacter
        {
            Intelligence = 16,
            Wisdom = 12,
            AlchemistTool = new Tool
            {
                Modifier = 2
            },
            HerbalismTool = new Tool
            {
                Modifier = -1
            }
        };

        var alchemistResult = character.AlchemistTool.GetModify(character.IntelligenceModifier);
        var herbalismResult = character.HerbalismTool.GetModify(character.WisdomModifier);

        Assert.Equal(5, alchemistResult);
        Assert.Equal(0, herbalismResult);
    }
}