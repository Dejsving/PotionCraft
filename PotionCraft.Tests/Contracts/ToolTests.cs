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

        var result = tool.GetModify(2, 2);

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

        var result = tool.GetModify(1, 2);

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
            ProficiencyBonus = 2,
            AlchemistTool = new Tool
            {
                Modifier = 2
            },
            HerbalismTool = new Tool
            {
                Modifier = -1
            }
        };

        var alchemistResult = character.AlchemistTool.GetModify(character.IntelligenceModifier, character.ProficiencyBonus);
        var herbalismResult = character.HerbalismTool.GetModify(character.WisdomModifier, character.ProficiencyBonus);

        Assert.Equal(5, alchemistResult);
        Assert.Equal(0, herbalismResult);
    }
        
    /// <summary>
    /// Проверяет, что свойства модификаторов инструментов персонажа вычисляются верно.
    /// </summary>
    [Fact]
    public void PlayerCharacter_Modifications_ReturnsCorrectValues()
    {
        var character = new PlayerCharacter
        {
            Intelligence = 16,
            Wisdom = 12,
            ProficiencyBonus = 2,
            AlchemistTool = new Tool { Modifier = 2 },
            HerbalismTool = new Tool { Modifier = -1 },
            PoisonerTool = new Tool { Modifier = 0 }
        };

        Assert.Equal(character.AlchemistTool.GetModify(character.IntelligenceModifier, character.ProficiencyBonus), character.AlchemistModify);
        Assert.Equal(character.HerbalismTool.GetModify(character.WisdomModifier, character.ProficiencyBonus), character.HerbalismModify);
        Assert.Equal(character.PoisonerTool.GetModify(character.WisdomModifier, character.ProficiencyBonus), character.PoisonerModify);
    }
}