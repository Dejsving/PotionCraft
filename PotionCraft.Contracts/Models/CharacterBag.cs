namespace PotionCraft.Contracts.Models;

/// <summary>
/// Сумка персонажа — хранит собранные травы, созданные зелья и яды.
/// </summary>
public class CharacterBag
{
    /// <summary>
    /// Уникальный идентификатор сумки.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Собранные травы. Ключ — идентификатор травы, значение — результат сбора с количеством.
    /// </summary>
    public Dictionary<Guid, GatheringResult> Herbs { get; set; } = new();

    /// <summary>
    /// Созданные зелья. Ключ — идентификатор зелья, значение — зелье с количеством.
    /// </summary>
    public Dictionary<Guid, PotionBagItem> Potions { get; set; } = new();

    /// <summary>
    /// Созданные яды. Ключ — идентификатор яда, значение — яд с количеством.
    /// </summary>
    public Dictionary<Guid, PotionBagItem> Poisons { get; set; } = new();

    /// <summary>
    /// Общее количество денег в кошельке игрока, хранящееся в медных монетах (базовая единица).
    /// </summary>
    public int Coins { get; set; }

    /// <summary>
    /// Количество золотых монет (сотни и выше от базового значения).
    /// </summary>
    public int GoldCoins => Coins / 100;

    /// <summary>
    /// Количество серебряных монет (десятки). 1 серебряная = 10 медных.
    /// </summary>
    public int SilverCoins => (Coins % 100) / 10;

    /// <summary>
    /// Количество медных монет (единицы).
    /// </summary>
    public int CopperCoins => Coins % 10;

    /// <summary>
    /// Добавляет траву в сумку или увеличивает количество уже имеющейся травы.
    /// </summary>
    /// <param name="herbId">Идентификатор травы.</param>
    /// <param name="herb">Трава для добавления.</param>
    /// <param name="quantity">Количество добавляемой травы.</param>
    public void AddOrUpdateHerb(Guid herbId, Herb herb, int quantity)
    {
        if (Herbs.TryGetValue(herbId, out var existing))
        {
            existing.Quantity += quantity;
        }
        else
        {
            Herbs[herbId] = new GatheringResult
            {
                Herb = herb,
                Quantity = quantity
            };
        }
    }
}
