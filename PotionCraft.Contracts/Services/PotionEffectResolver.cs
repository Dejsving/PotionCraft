using System.Text;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Extensions;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Models;

namespace PotionCraft.Contracts.Services;

/// <summary>
/// Вычисляет итоговый эффект зелья, применяя модификаторы к базовому эффекту травы-основы.
/// </summary>
public class PotionEffectResolver : IPotionEffectResolver
{
    /// <summary>
    /// Порядок повышения типа кости на 1 шаг. Травы с RequiresDmJudgement в расчёте не участвуют.
    /// </summary>
    private static readonly DiceTypeEnum[] _diceUpgradeSteps =
    {
        DiceTypeEnum.D4, DiceTypeEnum.D6, DiceTypeEnum.D8, DiceTypeEnum.D10, DiceTypeEnum.D12, DiceTypeEnum.D20
    };

    /// <summary>
    /// Заметка о решении Мастера, добавляемая при наличии в составе трав с RequiresDmJudgement (например, Хромовой слизи).
    /// </summary>
    private const string _dmJudgementNoteText =
        "В составе есть ингредиент, итоговый эффект которого зависит от решения Мастера (например, Хромовая слизь). " +
        "Все остальные модификаторы уже учтены в расчёте выше — это основа без учёта такого ингредиента. " +
        "Мастер не обязан менять всё сразу: он может инвертировать только ОДИН параметр на выбор " +
        "(тип эффекта/урона, тип кости или количество костей), оставив остальные как в базовом расчёте.";

    /// <inheritdoc />
    public PotionEffectResult Resolve(Herb baseHerb, IReadOnlyList<Herb> orderedModifiers)
    {
        ArgumentNullException.ThrowIfNull(baseHerb);
        orderedModifiers ??= [];

        if (baseHerb.FormulaDiceType is null || baseHerb.FormulaDiceCount is null)
        {
            return ResolveFixedEffect(baseHerb, orderedModifiers);
        }

        var diceCount = baseHerb.FormulaDiceCount.Value;
        var diceType = baseHerb.FormulaDiceType.Value;
        var includesAlchemyMod = baseHerb.FormulaIncludesAlchemyMod ?? false;
        var damageType = baseHerb.DamageType;
        var damageTypeChoices = new List<DamageTypeEnum>();
        var notes = new List<string>();
        var hasDmJudgementModifier = false;

        // Модификаторы с RequiresDmJudgement (например, Хромовая слизь) исключаются из числового расчёта.
        foreach (var modifier in orderedModifiers)
        {
            if (modifier.RequiresDmJudgement)
            {
                hasDmJudgementModifier = true;
                continue;
            }

            switch (modifier.ModifierEffect)
            {
                case HerbModifierEffectEnum.HealingUpgradeDiceType:
                case HerbModifierEffectEnum.PoisonUpgradeDiceType:
                    diceType = UpgradeDiceType(diceType, notes);
                    break;

                case HerbModifierEffectEnum.HealingDoubleCountHalveResultOverTime:
                    diceCount *= 2;
                    notes.Add("Результат броска делится на 2 (с округлением вниз); эффект растягивается на 2 раунда.");
                    break;

                case HerbModifierEffectEnum.HealingDoubleCountRemoveAlchemyMod:
                    diceCount *= 2;
                    includesAlchemyMod = false;
                    break;

                case HerbModifierEffectEnum.PoisonDoubleCountHalveDuration:
                    diceCount *= 2;
                    notes.Add("Длительность эффекта уменьшается вдвое.");
                    break;

                case HerbModifierEffectEnum.PoisonChangeToColdOrNecrotic:
                case HerbModifierEffectEnum.PoisonChangeToFireOrAcid:
                case HerbModifierEffectEnum.PoisonChangeToRadiant:
                    if (modifier.ReplacementDamageTypes is { Count: > 0 })
                        damageTypeChoices.AddRange(modifier.ReplacementDamageTypes);
                    break;

                case HerbModifierEffectEnum.PoisonNonLethalUnconscious:
                case HerbModifierEffectEnum.PoisonDelayedNotice:
                case HerbModifierEffectEnum.PoisonSlowTarget:
                case HerbModifierEffectEnum.PoisonDisadvantageOnChecks:
                case HerbModifierEffectEnum.UniversalDelayEffect:
                case HerbModifierEffectEnum.UniversalStabilizeBrew:
                case HerbModifierEffectEnum.FoodSourceConversion:
                    if (!string.IsNullOrWhiteSpace(modifier.Effect))
                        notes.Add(modifier.Effect);
                    break;

                case HerbModifierEffectEnum.None:
                case HerbModifierEffectEnum.UniversalInvertEffect:
                    break;
            }
        }

        var result = new PotionEffectResult
        {
            DiceCount = diceCount,
            DiceType = diceType,
            IncludesAlchemyMod = includesAlchemyMod,
            DamageType = damageType,
            DamageTypeChoices = damageTypeChoices,
            Notes = notes,
            DmJudgementNote = hasDmJudgementModifier ? _dmJudgementNoteText : null,
            IsFixedEffect = false,
        };

        result.FormulaText = BuildFormulaText(baseHerb, result);
        result.GeneratedDescription = BuildDescription(result);
        return result;
    }

    /// <summary>
    /// Повышает тип кости на 1 шаг по таблице прогрессии; на D20 дальнейшее повышение не даёт эффекта.
    /// </summary>
    private static DiceTypeEnum UpgradeDiceType(DiceTypeEnum current, List<string> notes)
    {
        var index = Array.IndexOf(_diceUpgradeSteps, current);
        if (index < 0 || index == _diceUpgradeSteps.Length - 1)
        {
            notes.Add($"Тип кости уже максимален ({current}) — повышение не даёт эффекта.");
            return current;
        }

        return _diceUpgradeSteps[index + 1];
    }

    /// <summary>
    /// Строит результат для трав-основ без вычисляемой формулы — просто склеивает исходные тексты Effect по порядку.
    /// </summary>
    private static PotionEffectResult ResolveFixedEffect(Herb baseHerb, IReadOnlyList<Herb> orderedModifiers)
    {
        var hasDmJudgementModifier = orderedModifiers.Any(m => m.RequiresDmJudgement);
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseHerb.Effect))
            parts.Add(baseHerb.Effect);
        parts.AddRange(orderedModifiers.Where(m => !string.IsNullOrWhiteSpace(m.Effect)).Select(m => m.Effect));

        if (hasDmJudgementModifier)
            parts.Add(_dmJudgementNoteText);

        return new PotionEffectResult
        {
            IsFixedEffect = true,
            DmJudgementNote = hasDmJudgementModifier ? _dmJudgementNoteText : null,
            GeneratedDescription = string.Join(" ", parts),
        };
    }

    /// <summary>
    /// Строит только строку вычисленной формулы (префикс травы-основы + кости + Мод. Алхимии + тип урона), без заметок.
    /// </summary>
    private static string BuildFormulaText(Herb baseHerb, PotionEffectResult result)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(baseHerb.FormulaPrefix))
            sb.Append(baseHerb.FormulaPrefix);

        sb.Append(result.DiceCount).Append('к').Append((int)result.DiceType!.Value);

        if (result.IncludesAlchemyMod)
            sb.Append(" + Мод. Алхимии");

        if (result.DamageTypeChoices.Count > 0)
        {
            sb.Append(" урона (").Append(string.Join(" или ", result.DamageTypeChoices.Select(d => d.GetDisplayName()))).Append(')');
        }
        else if (result.DamageType is not null)
        {
            sb.Append(" урона ").Append(result.DamageType.Value.GetDisplayName());
        }

        sb.Append('.');
        return sb.ToString();
    }

    /// <summary>
    /// Строит итоговое текстовое описание: формула + заметки + решение Мастера.
    /// </summary>
    private static string BuildDescription(PotionEffectResult result)
    {
        var sb = new StringBuilder(result.FormulaText);

        foreach (var note in result.Notes)
            sb.Append(' ').Append(note);

        if (result.DmJudgementNote is not null)
            sb.Append(' ').Append(result.DmJudgementNote);

        return sb.ToString();
    }
}
