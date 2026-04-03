using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Services.Gathering;

/// <summary>
/// Реализация сервиса сбора трав, использующая таблицы сбора.
/// </summary>
public class GatheringService : IGatheringService
{
    private static readonly
        Dictionary<TerrainEnum, Dictionary<int, (string HerbName, AdditionalRulesEnum Rule)>>
        GatheringTables = new()
    {
        [TerrainEnum.Everewhere] = new()
        {
            { 2, ("Корень Мандрагоры", AdditionalRulesEnum.None) },
            { 3, ("Ртутный лишайник", AdditionalRulesEnum.None) },
            { 4, ("Ртутный лишайник", AdditionalRulesEnum.None) },
            { 5, ("Корень дикого Шалфея", AdditionalRulesEnum.None) },
            { 6, ("Корень дикого Шалфея", AdditionalRulesEnum.None) },
            { 7, ("Кровьтрава", AdditionalRulesEnum.ReRollIfNoProvisions) },
            { 8, ("Лепестки змееуста", AdditionalRulesEnum.None) },
            { 9, ("Лепестки змееуста", AdditionalRulesEnum.None) },
            { 10, ("Семена Молочая", AdditionalRulesEnum.None) },
            { 11, ("Семена Молочая", AdditionalRulesEnum.None) },
            { 12, ("Корень Мандрагоры", AdditionalRulesEnum.None) }
        },
        [TerrainEnum.Arctic] = new()
        {
            { 2, ("Серебряный Гибискус", AdditionalRulesEnum.None) },
            { 3, ("Порошок морплоти", AdditionalRulesEnum.None) },
            { 4, ("Сердце Жлезодрева", AdditionalRulesEnum.None) },
            { 5, ("Замороженные саженцы", AdditionalRulesEnum.TwoOnly) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Арктический плющ", AdditionalRulesEnum.TwoOnly) },
            { 10, ("Фенхелейвый шёлк", AdditionalRulesEnum.None) },
            { 11, ("Дьявольский плющ", AdditionalRulesEnum.None) },
            { 12, ("Корень пустоты", AdditionalRulesEnum.None) }
        },
        [TerrainEnum.Coast] = new() // Берега/Подводная среда
        {
            { 2, ("Водополох", AdditionalRulesEnum.OneOrTwo) },
            { 3, ("Шляпка мухомора", AdditionalRulesEnum.OnlyOnCoast) },
            { 4, ("Нектар Гиацинта", AdditionalRulesEnum.None) },
            { 5, ("Хромовая слизь", AdditionalRulesEnum.OneOrTwo) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Веточка лаванды", AdditionalRulesEnum.OnlyOnCoast) },
            { 10, ("Синий Кривожаб", AdditionalRulesEnum.OnlyOnCoast) },
            { 11, ("Зловонная луковица", AdditionalRulesEnum.None) },
            { 12, ("Ко-Глонд", AdditionalRulesEnum.OneOrTwo) }
        },
        [TerrainEnum.Deserts] = new()
        {
            { 2, ("Ко-Глонд", AdditionalRulesEnum.None) },
            { 3, ("Корень Стрела", AdditionalRulesEnum.None) },
            { 4, ("Высушенная Эфедра", AdditionalRulesEnum.None) },
            { 5, ("Сок кактуса", AdditionalRulesEnum.TwoOnly) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Цветы Дракуса", AdditionalRulesEnum.None) },
            { 10, ("Бобы Сцили", AdditionalRulesEnum.None) },
            { 11, ("Ягоды Шипоцвета", AdditionalRulesEnum.None) },
            { 12, ("Корень пустоты", AdditionalRulesEnum.AddElementalWater) }
        },
        [TerrainEnum.Forest] = new()
        {
            { 2, ("Листья Харады", AdditionalRulesEnum.None) },
            { 3, ("Ягоды Паслёна", AdditionalRulesEnum.None) },
            { 4, ("Рвоск", AdditionalRulesEnum.None) },
            { 5, ("Вердинская Крапива", AdditionalRulesEnum.None) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Корень Стрела", AdditionalRulesEnum.None) },
            { 10, ("Сердце Жлезодрева", AdditionalRulesEnum.None) },
            { 11, ("Синий Кривожаб", AdditionalRulesEnum.None) },
            { 12, ("Стебли Гифломы", AdditionalRulesEnum.NightTwoDayReroll) }
        },
        [TerrainEnum.Meadows] = new()
        {
            { 2, ("Листья Харады", AdditionalRulesEnum.None) },
            { 3, ("Цветы Дракуса", AdditionalRulesEnum.None) },
            { 4, ("Веточка лаванды", AdditionalRulesEnum.TwoOnly) },
            { 5, ("Корень Стрела", AdditionalRulesEnum.None) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Бобы Сцили", AdditionalRulesEnum.TwoOnly) },
            { 10, ("Сок кактуса", AdditionalRulesEnum.None) },
            { 11, ("Листохвост", AdditionalRulesEnum.None) },
            { 12, ("Нектар Гиацинта", AdditionalRulesEnum.None) }
        },
        [TerrainEnum.Hills] = new()
        {
            { 2, ("Дьявольский кроволист", AdditionalRulesEnum.None) },
            { 3, ("Ягоды Паслёна", AdditionalRulesEnum.None) },
            { 4, ("Листохвост", AdditionalRulesEnum.TwoOnly) },
            { 5, ("Веточка лаванды", AdditionalRulesEnum.None) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Сердце Жлезодрева", AdditionalRulesEnum.None) },
            { 10, ("Кисть Генгкоу", AdditionalRulesEnum.None) },
            { 11, ("Каменный вьюн", AdditionalRulesEnum.TwoOnly) },
            { 12, ("Листья Харады", AdditionalRulesEnum.None) }
        },
        [TerrainEnum.Mountains] = new()
        {
            { 2, ("Дыхание Василиска", AdditionalRulesEnum.None) },
            { 3, ("Замороженные саженцы", AdditionalRulesEnum.TwoOnly) },
            { 4, ("Арктический плющ", AdditionalRulesEnum.TwoOnly) },
            { 5, ("Высушенная Эфедра", AdditionalRulesEnum.None) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Цветы Дракуса", AdditionalRulesEnum.None) },
            { 10, ("Сетопыль шляпки", AdditionalRulesEnum.TwoIfCave) },
            { 11, ("Каменный вьюн", AdditionalRulesEnum.None) },
            { 12, ("Изначальный бальзам", AdditionalRulesEnum.None) }
        },
        [TerrainEnum.Swamp] = new()
        {
            { 2, ("Дьявольский кроволист", AdditionalRulesEnum.None) },
            { 3, ("Ягоды шипоцвета", AdditionalRulesEnum.None) },
            { 4, ("Рвоск", AdditionalRulesEnum.None) },
            { 5, ("Шляпка мухомора", AdditionalRulesEnum.TwoOnly) },
            { 6, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 7, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 8, ("Повсеместный ингредиент", AdditionalRulesEnum.CheckCommon) },
            { 9, ("Синий Кривожаб", AdditionalRulesEnum.TwoOnly) },
            { 10, ("Зловонная луковица", AdditionalRulesEnum.None) },
            { 11, ("Водополох", AdditionalRulesEnum.TwoIfRain) },
            { 12, ("Изначальный бальзам", AdditionalRulesEnum.None) }
        },
        [TerrainEnum.Underdark] = new()
        {
            { 2, ("Изначальный бальзам", AdditionalRulesEnum.TwoOnly) },
            { 3, ("Серебряный гибискус", AdditionalRulesEnum.None) },
            { 4, ("Дьявольский кроволист", AdditionalRulesEnum.None) },
            { 5, ("Хромовая слизь", AdditionalRulesEnum.None) },
            { 6, ("Порошок морплоти", AdditionalRulesEnum.TwoOnly) },
            { 7, ("Фенхелейвый шёлк", AdditionalRulesEnum.None) },
            { 8, ("Дьявольский плющ", AdditionalRulesEnum.None) },
            { 9, ("Кисть Генгкоу", AdditionalRulesEnum.None) },
            { 10, ("Сетопыль шляпки", AdditionalRulesEnum.TwoOnly) },
            { 11, ("Сияющий синтоцвет", AdditionalRulesEnum.None) },
            { 12, ("Стебли Гифломы", AdditionalRulesEnum.None) }
        }
    };

    /// <summary>
    /// Статический конструктор: UnderWater использует ту же таблицу, что и Coast.
    /// </summary>
    static GatheringService()
    {
        GatheringTables[TerrainEnum.UnderWater] = GatheringTables[TerrainEnum.Coast];
    }

    private readonly IHerbRepository _herbRepository;

    /// <summary>
    /// Сервис бросков кубиков.
    /// </summary>
    private readonly IDiceRoller _diceRoller;

    /// <summary>
    /// Конструктор сервиса.
    /// </summary>
    /// <param name="herbRepository">Репозиторий трав.</param>
    /// <param name="diceRoller">Сервис бросков кубиков.</param>
    public GatheringService(IHerbRepository herbRepository, IDiceRoller diceRoller)
    {
        _herbRepository = herbRepository;
        _diceRoller = diceRoller;
    }

    /// <summary>
    /// Осуществляет одиночный бросок сбора трав.
    /// </summary>
    public async Task<GatheringResult> GatherHerbAsync(GatheringRequest request)
    {
        var allHerbs = await _herbRepository.GetAllAsync();
        
        while (true)
        {
            int roll = _diceRoller.Roll(DiceRoll.TwoD6);
            
            // Если выпадает 2-4 или 10-12, шанс на Элементальную воду
            if ((roll >= 2 && roll <= 4) || (roll >= 10 && roll <= 12))
            {
                if (_diceRoller.Roll(DiceRoll.D100) > 75)
                {
                    var elementalWater = allHerbs.FirstOrDefault(h => h.Name.Equals("Элементальная вода", StringComparison.OrdinalIgnoreCase))
                                         ?? new Herb { Name = "Элементальная вода", Description = "Вспомогательный ингредиент для мощных зелий" };
                    return new GatheringResult { Herb = elementalWater, Quantity = 1 }; // по правилам нужно минимум половина сосуда
                }
            }

            if (!GatheringTables.TryGetValue(request.Terrain, out var terrainTable))
            {
                // На случай если таблица для местности не определена, используем Everewhere
                terrainTable = GatheringTables[TerrainEnum.Everewhere];
            }

            var (herbName, rule) = terrainTable[roll];

            if (rule == AdditionalRulesEnum.CheckCommon)
            {
                int commonRoll = _diceRoller.Roll(DiceRoll.TwoD6);
                (herbName, rule) = GatheringTables[TerrainEnum.Everewhere][commonRoll];
            }

            if (rule == AdditionalRulesEnum.ReRollIfNoProvisions && !request.HasProvisions)
            {
                // Повторяем бросок
                continue;
            }

            if (rule == AdditionalRulesEnum.ReRoll)
            {
                // Повторяем бросок
                continue;
            }

            if (rule == AdditionalRulesEnum.OnlyOnCoast && request.Terrain != TerrainEnum.Coast)
            {
                // Только на берегу (иначе переброс)
                continue; 
            }

            if (rule == AdditionalRulesEnum.NightTwoDayReroll && !request.IsNight)
            {
                // Днем перебросить
                continue;
            }

            int quantity = DetermineQuantity(rule, request);

            Herb? gatheredHerb = allHerbs.FirstOrDefault(h => h.Name.Equals(herbName, StringComparison.OrdinalIgnoreCase));
            
            // Если травы нет в БД, создадим пустой, чтобы вернуть название
            if (gatheredHerb == null)
            {
                gatheredHerb = new Herb { Name = herbName };
            }

            return new GatheringResult { Herb = gatheredHerb, Quantity = quantity };
        }
    }

    private int DetermineQuantity(AdditionalRulesEnum rule, GatheringRequest request)
    {
        return rule switch
        {
            AdditionalRulesEnum.OneOnly => 1,
            AdditionalRulesEnum.TwoOnly => 2,
            AdditionalRulesEnum.TwoIfRain => request.IsRaining ? 2 : _diceRoller.Roll(DiceRoll.D4),
            AdditionalRulesEnum.TwoIfCave => request.IsCave ? 2 : _diceRoller.Roll(DiceRoll.D4),
            AdditionalRulesEnum.NightTwoDayReroll => 2, // Днем переброс, сюда дойдет только ночью
            AdditionalRulesEnum.OneOrTwo => _diceRoller.Roll(DiceRoll.D4) % 2 == 0 ? 2 : 1, // 1-2 шт.
            AdditionalRulesEnum.AddElementalWater => 1, // Элементальная вода всегда 1 шт.
            _ => _diceRoller.Roll(DiceRoll.D4) // результат броска D4 растений, если нет других правил
        };
    }
}