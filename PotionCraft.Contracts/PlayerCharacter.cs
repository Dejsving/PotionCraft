using PotionCraft.Contracts.Models;

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
    /// Инструменты алхимика персонажа.
    /// </summary>
    public Tool AlchemistTool { get; set; } = new();

    /// <summary>
    /// Набор травника персонажа.
    /// </summary>
    public Tool HerbalismTool { get; set; } = new();

    /// <summary>
    /// Инструменты отравителя персонажа.
    /// </summary>
    public Tool PoisonerTool { get; set; } = new();

    /// <summary>
    /// Сумка персонажа для хранения трав, зелий и ядов.
    /// </summary>
    public CharacterBag Bag { get; set; } = new();

    /// <summary>
    /// Идентификатор игрока, выбравшего этого персонажа. Null — персонаж свободен.
    /// </summary>
    public Guid? SelectedBy { get; set; }

    /// <summary>
    /// Токен оптимистичной конкурентности. Обновляется автоматически при каждом сохранении.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

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
    /// Модификатор Алхимии
    /// </summary>
    public int AlchemistModify
    {
        get => AlchemistTool.GetModify(IntelligenceModifier, ProficiencyBonus);
    }

    /// <summary>
    /// Модификатор Гербализма
    /// </summary>
    public int HerbalismModify
    {
        get => HerbalismTool.GetModify(WisdomModifier, ProficiencyBonus);
    }

    /// <summary>
    /// Модификатор Отравителя
    /// </summary>
    public int PoisonerModify
    {
        get => PoisonerTool.GetModify(WisdomModifier, ProficiencyBonus);
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