namespace PotionCraft.Contracts;

/// <summary>
/// Класс для хранения персонажа игрока.
/// </summary>
public class PlayerCharacter
{
    /// <summary>
    /// Уникальный идентификатор персонажа.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя персонажа.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Значение интеллекта.
    /// </summary>
    public int Intelligence { get; set; }

    /// <summary>
    /// Значение мудрости.
    /// </summary>
    public int Wisdom { get; set; }

    /// <summary>
    /// Профессиональный бонус (бонус мастерства).
    /// </summary>
    public int ProficiencyBonus { get; set; }

    /// <summary>
    /// Владение набором травника (herbalism kit).
    /// </summary>
    public bool HasHerbalismKitProficiency { get; set; }

    /// <summary>
    /// Мастерство (экспертиза) во владении набором травника.
    /// </summary>
    public bool HasHerbalismKitExpertise { get; set; }

    /// <summary>
    /// Владение инструментами алхимика (alchemist's supplies).
    /// </summary>
    public bool HasAlchemistSuppliesProficiency { get; set; }

    /// <summary>
    /// Мастерство (экспертиза) во владении инструментами алхимика.
    /// </summary>
    public bool HasAlchemistSuppliesExpertise { get; set; }

    /// <summary>
    /// Владение инструментами отравителя (poisoner's kit).
    /// </summary>
    public bool HasPoisonerSuppliesProficiency { get; set; }

    /// <summary>
    /// Мастерство (экспертиза) во владении инструментами отравителя.
    /// </summary>
    public bool HasPoisonerSuppliesExpertise { get; set; }

    /// <summary>
    /// Модификатор Алхимии
    /// </summary>
    public int AlchemistModify
    {
        get
        {
            int bonus = HasAlchemistSuppliesExpertise ? ProficiencyBonus * 2 :
                        HasAlchemistSuppliesProficiency ? ProficiencyBonus : 0;
            return IntelligenceModifier + bonus;
        }
    }

    /// <summary>
    /// Модификатор Гербализма
    /// </summary>
    public int HerbalismModify
    {
        get
        {
            int bonus = HasHerbalismKitExpertise ? ProficiencyBonus * 2 :
                        HasHerbalismKitProficiency ? ProficiencyBonus : 0;
            return WisdomModifier + bonus;
        }
    }

    /// <summary>
    /// Модификатор Отравителя
    /// </summary>
    public int PoisonerModify
    {
        get
        {
            int bonus = HasPoisonerSuppliesExpertise ? ProficiencyBonus * 2 :
                        HasPoisonerSuppliesProficiency ? ProficiencyBonus : 0;
            return WisdomModifier + bonus;
        }
    }

    /// <summary>
    /// Модификатор мудрости на основе её значения.
    /// </summary>
    public int WisdomModifier
    {
        get => GetAbilityModifier(Wisdom);
    }

    /// <summary>
    /// Модификатор интеллекта на основе её значения.
    /// </summary>
    public int IntelligenceModifier
    {
        get => GetAbilityModifier(Intelligence);
    }

    /// <summary>
    /// Вычисляет модификатор характеристики на основе её значения.
    /// </summary>
    /// <param name="ability">Значение характеристики (например, интеллекта или мудрости).</param>
    /// <returns>Модификатор характеристики.</returns>
    private static int GetAbilityModifier(int ability)
    {
        return (ability / 2) - 5;
    }
}