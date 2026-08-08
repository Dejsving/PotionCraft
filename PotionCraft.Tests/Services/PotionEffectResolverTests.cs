using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Contracts.Services;

namespace PotionCraft.Tests.Services;

/// <summary>
/// Тесты для PotionEffectResolver: проверка применения трав-модификаторов к базовому эффекту.
/// </summary>
public class PotionEffectResolverTests
{
    /// <summary>
    /// Экземпляр резолвера для тестирования.
    /// </summary>
    private readonly PotionEffectResolver _resolver = new();

    private static Herb HealingBase() => new()
    {
        Name = "Корень дикого Шалфея",
        HerbType = HerbTypeEnum.HealingBase,
        Effect = "Исцеляет 2к4 + Мод. Алхимии.",
        FormulaDiceCount = 2,
        FormulaDiceType = DiceTypeEnum.D4,
        FormulaIncludesAlchemyMod = true,
        FormulaPrefix = "Исцеляет ",
    };

    private static Herb PoisonBase() => new()
    {
        Name = "Лепестки Змееуста",
        HerbType = HerbTypeEnum.PoisonBase,
        Effect = "1к4 + Мод. Алхимии урона ядом за раунд.",
        FormulaDiceCount = 1,
        FormulaDiceType = DiceTypeEnum.D4,
        FormulaIncludesAlchemyMod = true,
        DamageType = DamageTypeEnum.Poison,
    };

    private static Herb FixedEffectBase() => new()
    {
        Name = "Корень Мандрагоры",
        HerbType = HerbTypeEnum.HealingBase,
        Effect = "Ослабляет любую болезнь или яд наполовину.",
    };

    private static Herb Modifier(HerbModifierEffectEnum effect, string text = "Эффект модификатора.",
        bool requiresDmJudgement = false, IReadOnlyList<DamageTypeEnum>? replacementDamageTypes = null) => new()
    {
        Name = effect.ToString(),
        HerbType = HerbTypeEnum.HealingModifier,
        ModifierEffect = effect,
        Effect = text,
        RequiresDmJudgement = requiresDmJudgement,
        ReplacementDamageTypes = replacementDamageTypes,
    };

    /// <summary>
    /// Проверяет, что без модификаторов результат совпадает с базовой формулой.
    /// </summary>
    [Fact]
    public void Resolve_NoModifiers_ReturnsBaseFormula()
    {
        var result = _resolver.Resolve(HealingBase(), []);

        Assert.False(result.IsFixedEffect);
        Assert.Equal(2, result.DiceCount);
        Assert.Equal(DiceTypeEnum.D4, result.DiceType);
        Assert.True(result.IncludesAlchemyMod);
        Assert.Null(result.DmJudgementNote);
    }

    /// <summary>
    /// Проверяет, что HealingUpgradeDiceType повышает тип кости на 1 шаг (d4 -> d6).
    /// </summary>
    [Fact]
    public void Resolve_HealingUpgradeDiceType_UpgradesDiceTypeByOneStep()
    {
        var result = _resolver.Resolve(HealingBase(), [Modifier(HerbModifierEffectEnum.HealingUpgradeDiceType)]);

        Assert.Equal(DiceTypeEnum.D6, result.DiceType);
    }

    /// <summary>
    /// Проверяет, что повышение типа кости на максимальном значении (d20) не изменяет кость, но добавляет заметку.
    /// </summary>
    [Fact]
    public void Resolve_UpgradeDiceTypeAtMax_AddsNoteAndKeepsDiceType()
    {
        var baseHerb = HealingBase();
        baseHerb.FormulaDiceType = DiceTypeEnum.D20;

        var result = _resolver.Resolve(baseHerb, [Modifier(HerbModifierEffectEnum.HealingUpgradeDiceType)]);

        Assert.Equal(DiceTypeEnum.D20, result.DiceType);
        Assert.Contains(result.Notes, n => n.Contains("максимален"));
    }

    /// <summary>
    /// Проверяет, что HealingDoubleCountHalveResultOverTime удваивает количество костей и добавляет заметку.
    /// </summary>
    [Fact]
    public void Resolve_HealingDoubleCountHalveResultOverTime_DoublesCountAndAddsNote()
    {
        var result = _resolver.Resolve(HealingBase(),
            [Modifier(HerbModifierEffectEnum.HealingDoubleCountHalveResultOverTime)]);

        Assert.Equal(4, result.DiceCount);
        Assert.True(result.IncludesAlchemyMod);
        Assert.Single(result.Notes);
    }

    /// <summary>
    /// Проверяет, что HealingDoubleCountRemoveAlchemyMod удваивает кости и убирает Мод. Алхимии.
    /// </summary>
    [Fact]
    public void Resolve_HealingDoubleCountRemoveAlchemyMod_DoublesCountAndRemovesAlchemyMod()
    {
        var result = _resolver.Resolve(HealingBase(),
            [Modifier(HerbModifierEffectEnum.HealingDoubleCountRemoveAlchemyMod)]);

        Assert.Equal(4, result.DiceCount);
        Assert.False(result.IncludesAlchemyMod);
    }

    /// <summary>
    /// Проверяет, что PoisonDoubleCountHalveDuration удваивает кости и добавляет заметку про длительность.
    /// </summary>
    [Fact]
    public void Resolve_PoisonDoubleCountHalveDuration_DoublesCountAndAddsNote()
    {
        var result = _resolver.Resolve(PoisonBase(),
            [Modifier(HerbModifierEffectEnum.PoisonDoubleCountHalveDuration)]);

        Assert.Equal(2, result.DiceCount);
        Assert.Single(result.Notes);
    }

    /// <summary>
    /// Проверяет, что модификатор смены типа урона добавляет варианты выбора типа урона.
    /// </summary>
    [Fact]
    public void Resolve_PoisonChangeToColdOrNecrotic_AddsDamageTypeChoices()
    {
        var modifier = Modifier(HerbModifierEffectEnum.PoisonChangeToColdOrNecrotic,
            replacementDamageTypes: [DamageTypeEnum.Cold, DamageTypeEnum.Necrotic]);

        var result = _resolver.Resolve(PoisonBase(), [modifier]);

        Assert.Equal([DamageTypeEnum.Cold, DamageTypeEnum.Necrotic], result.DamageTypeChoices);
    }

    /// <summary>
    /// Проверяет, что поведенческий модификатор добавляет свой текст Effect в заметки без изменения формулы.
    /// </summary>
    [Fact]
    public void Resolve_BehavioralModifier_AddsEffectTextAsNoteWithoutChangingFormula()
    {
        var modifier = Modifier(HerbModifierEffectEnum.PoisonNonLethalUnconscious,
            "Меняет любой эффект яда на не летальный, цель падает без сознания.");

        var result = _resolver.Resolve(PoisonBase(), [modifier]);

        Assert.Equal(1, result.DiceCount);
        Assert.Contains(modifier.Effect, result.Notes);
    }

    /// <summary>
    /// Ключевой сценарий: Хромовая слизь (RequiresDmJudgement) исключается из числового расчёта — остальные
    /// модификаторы применяются как обычно, а по слизи добавляется отдельная заметка для Мастера.
    /// </summary>
    [Fact]
    public void Resolve_ChromeSlimeAmongOtherModifiers_ExcludedFromCalculation_OthersStillApplied()
    {
        var slime = Modifier(HerbModifierEffectEnum.UniversalInvertEffect, requiresDmJudgement: true);
        var upgradeDice = Modifier(HerbModifierEffectEnum.HealingUpgradeDiceType);

        var result = _resolver.Resolve(HealingBase(), [upgradeDice, slime]);

        // Второй модификатор (повышение кости) применился, несмотря на присутствие слизи.
        Assert.Equal(DiceTypeEnum.D6, result.DiceType);
        Assert.Equal(2, result.DiceCount);
        Assert.True(result.IncludesAlchemyMod);
        // Слизь не участвует в числовом расчёте, но помечается отдельной заметкой для Мастера.
        Assert.NotNull(result.DmJudgementNote);
    }

    /// <summary>
    /// Проверяет, что без слизи заметка для Мастера отсутствует.
    /// </summary>
    [Fact]
    public void Resolve_WithoutDmJudgementModifier_DmJudgementNoteIsNull()
    {
        var result = _resolver.Resolve(HealingBase(), [Modifier(HerbModifierEffectEnum.HealingUpgradeDiceType)]);

        Assert.Null(result.DmJudgementNote);
    }

    /// <summary>
    /// Проверяет, что для базы без формулы (фиксированный эффект) результат — это склейка исходных текстов.
    /// </summary>
    [Fact]
    public void Resolve_FixedEffectBase_ReturnsConcatenatedEffectTexts()
    {
        var baseHerb = FixedEffectBase();
        var modifier = Modifier(HerbModifierEffectEnum.UniversalDelayEffect, "Задерживает эффект на 1к6 раундов.");

        var result = _resolver.Resolve(baseHerb, [modifier]);

        Assert.True(result.IsFixedEffect);
        Assert.Contains(baseHerb.Effect, result.GeneratedDescription);
        Assert.Contains(modifier.Effect, result.GeneratedDescription);
    }

    /// <summary>
    /// Проверяет, что FormulaText содержит префикс травы-основы и не содержит заметки/решение Мастера.
    /// </summary>
    [Fact]
    public void Resolve_FormulaText_IncludesBasePrefixWithoutNotes()
    {
        var result = _resolver.Resolve(HealingBase(), []);

        Assert.Equal("Исцеляет 2к4 + Мод. Алхимии.", result.FormulaText);
    }

    /// <summary>
    /// Регрессия: при нескольких одинаковых ингредиентах-удвоителях кости итоговая формула должна
    /// корректно перемножиться, а не дублировать текст модификатора в итоговом описании.
    /// </summary>
    [Fact]
    public void Resolve_ThreeIdenticalDoubleCountModifiers_MultipliesCorrectlyWithoutTextDuplication()
    {
        var modifier = Modifier(HerbModifierEffectEnum.HealingDoubleCountRemoveAlchemyMod,
            "Удваивает кол-во костей при броске любого исцеляющего Эффекта, но убирает Мод. Алхимии.");

        var result = _resolver.Resolve(HealingBase(), [modifier, modifier, modifier]);

        Assert.Equal(16, result.DiceCount);
        Assert.False(result.IncludesAlchemyMod);
        Assert.Equal("Исцеляет 16к4.", result.FormulaText);
        Assert.Empty(result.Notes);
        Assert.DoesNotContain(modifier.Effect, result.GeneratedDescription);
    }
}
