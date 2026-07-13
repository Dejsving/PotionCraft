namespace PotionCraft.Contracts.Enums;

public enum HerbModifierEffectEnum
{
    None = 0,

    // Исцеление — формула костей
    HealingUpgradeDiceType = 1,                // тип кости +1 шаг для исцеления
    HealingDoubleCountHalveResultOverTime = 2, // ×2 кости, ÷2 результат ⌊⌋, за 2 раунда
    HealingDoubleCountRemoveAlchemyMod = 3,    // ×2 кости, без Мод. Алхимии

    // Токсин — формула костей
    PoisonUpgradeDiceType = 4,                 // тип кости +1 шаг для токсина
    PoisonDoubleCountHalveDuration = 5,        // ×2 кости, ÷2 длительность

    // Токсин — смена типа урона
    PoisonChangeToColdOrNecrotic = 6,
    PoisonChangeToFireOrAcid = 7,
    PoisonChangeToRadiant = 8,

    // Токсин — поведенческие эффекты
    PoisonNonLethalUnconscious = 9,            // нелетален, потеря сознания
    PoisonDelayedNotice = 10,                  // цель не замечает 5 ходов
    PoisonSlowTarget = 11,                     // -10 фут скорости 1 мин
    PoisonDisadvantageOnChecks = 12,           // помеха на проверки

    // Универсальные (исцеление и яд)
    UniversalInvertEffect = 13,                // эффект противоположный (ДМ), после всех расчётов
    UniversalDelayEffect = 14,                 // задержка 1к6 раундов
    UniversalStabilizeBrew = 15,               // снижает BrewingDC (стабилизация)

    // Особые
    FoodSourceConversion = 16,                 // превращает смесь в еду на 1 день
}
