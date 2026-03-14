namespace PotionCraft.Contracts;

/// <summary>
/// Описывает владение инструментом персонажа.
/// </summary>
public class Tool
{
    /// <summary>
    /// Показывает, владеет ли персонаж инструментом.
    /// </summary>
    public bool Proficiency { get; set; }

    /// <summary>
    /// Показывает, обладает ли персонаж мастерством при использовании инструмента.
    /// </summary>
    public bool Expertise { get; set; }

    /// <summary>
    /// Дополнительный модификатор инструмента.
    /// </summary>
    public int Modifier { get; set; }

    /// <summary>
    /// Возвращает итоговый модификатор инструмента.
    /// </summary>
    /// <param name="modifier">Модификатор характеристики, от которой зависит инструмент.</param>
    /// <returns>Итоговый модификатор инструмента.</returns>
    public int GetModify(int modifier)
    {
        return modifier + Modifier;
    }
}