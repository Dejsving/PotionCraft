using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository;

namespace PotionCraft.InitData;

/// <summary>
/// Сервис для начального заполнения таблицы Herbs данными из встроенного JSON-файла.
/// </summary>
public static class HerbDataSeeder
{
    /// <summary>
    /// Синхронизирует таблицу Herbs с данными из встроенного ресурса Herbs.json: обновляет существующие травы по имени (сохраняя их Id, на который ссылаются сумки персонажей) и добавляет новые.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public static async Task SeedHerbsAsync(PotionCraftDbContext dbContext)
    {
        var seedHerbs = LoadHerbsFromResource();
        var existingByName = await dbContext.Herbs.ToDictionaryAsync(h => h.Name);

        foreach (var seedHerb in seedHerbs)
        {
            if (existingByName.TryGetValue(seedHerb.Name, out var existing))
            {
                ApplySeedData(existing, seedHerb);
            }
            else
            {
                await dbContext.Herbs.AddAsync(seedHerb);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет изменяемые поля существующей травы данными из Herbs.json, не трогая её Id.
    /// </summary>
    private static void ApplySeedData(Herb existing, Herb seedHerb)
    {
        existing.Description = seedHerb.Description;
        existing.HerbType = seedHerb.HerbType;
        existing.Rarity = seedHerb.Rarity;
        existing.Effect = seedHerb.Effect;
        existing.ModifierEffect = seedHerb.ModifierEffect;
        existing.Difficulty = seedHerb.Difficulty;
        existing.Habitats = seedHerb.Habitats;
        existing.FormulaDiceCount = seedHerb.FormulaDiceCount;
        existing.FormulaDiceType = seedHerb.FormulaDiceType;
        existing.FormulaIncludesAlchemyMod = seedHerb.FormulaIncludesAlchemyMod;
        existing.FormulaPrefix = seedHerb.FormulaPrefix;
        existing.DamageType = seedHerb.DamageType;
        existing.ReplacementDamageTypes = seedHerb.ReplacementDamageTypes;
        existing.RequiresDmJudgement = seedHerb.RequiresDmJudgement;
    }

    /// <summary>
    /// Загружает список трав из встроенного ресурса Herbs.json.
    /// </summary>
    /// <returns>Список трав.</returns>
    internal static List<Herb> LoadHerbsFromResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("Herbs.json"))
            ?? throw new InvalidOperationException("Встроенный ресурс Herbs.json не найден.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(), new HerbTypeFlagsConverter() }
        };

        var dtos = JsonSerializer.Deserialize<List<HerbDto>>(json, options)
            ?? throw new InvalidOperationException("Не удалось десериализовать Herbs.json.");

        return dtos.Select(MapToHerb).ToList();
    }

    /// <summary>
    /// Преобразует DTO травы в модель Herb.
    /// </summary>
    private static Herb MapToHerb(HerbDto dto)
    {
        return new Herb
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            HerbType = dto.HerbType,
            Rarity = dto.Rarity,
            Effect = dto.Effect,
            ModifierEffect = dto.ModifierEffect,
            Difficulty = dto.Difficulty,
            Habitats = dto.Habitats,
            FormulaDiceCount = dto.Formula?.DiceCount,
            FormulaDiceType = dto.Formula?.DiceType,
            FormulaIncludesAlchemyMod = dto.Formula?.IncludesAlchemyMod,
            FormulaPrefix = dto.Formula?.Prefix,
            DamageType = dto.DamageType,
            ReplacementDamageTypes = dto.ReplacementDamageTypes,
            RequiresDmJudgement = dto.RequiresDmJudgement
        };
    }

    /// <summary>
    /// DTO для десериализации травы из JSON.
    /// </summary>
    internal class HerbDto
    {
        /// <summary>
        /// Название травы.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание травы.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Тип травы (может быть комбинацией флагов).
        /// </summary>
        public HerbTypeEnum HerbType { get; set; }

        /// <summary>
        /// Редкость травы.
        /// </summary>
        public RarityEnum Rarity { get; set; }

        /// <summary>
        /// Описание эффекта.
        /// </summary>
        public string Effect { get; set; } = string.Empty;

        /// <summary>
        /// Машиночитаемый эффект модификатора.
        /// </summary>
        public HerbModifierEffectEnum ModifierEffect { get; set; } = HerbModifierEffectEnum.None;

        /// <summary>
        /// Сложность.
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        /// Среда обитания и значения бросков.
        /// </summary>
        public Dictionary<TerrainEnum, int> Habitats { get; set; } = new();

        /// <summary>
        /// Структурированная формула базового эффекта (кости + Мод. Алхимии). Null для трав без вычисляемой формулы.
        /// </summary>
        public HerbFormulaDto? Formula { get; set; }

        /// <summary>
        /// Тип урона базового эффекта (для трав с уроном, например ядов).
        /// </summary>
        public DamageTypeEnum? DamageType { get; set; }

        /// <summary>
        /// Варианты типов урона, которыми модификатор может заменить исходный тип урона.
        /// </summary>
        public IReadOnlyList<DamageTypeEnum>? ReplacementDamageTypes { get; set; }

        /// <summary>
        /// Итоговый эффект не вычисляется автоматически и оставляется на решение Мастера (например, Хромовая слизь).
        /// </summary>
        public bool RequiresDmJudgement { get; set; }
    }

    /// <summary>
    /// DTO для десериализации структурированной формулы базового эффекта из JSON.
    /// </summary>
    internal class HerbFormulaDto
    {
        /// <summary>
        /// Количество костей.
        /// </summary>
        public int DiceCount { get; set; }

        /// <summary>
        /// Тип кости.
        /// </summary>
        public DiceTypeEnum DiceType { get; set; }

        /// <summary>
        /// Признак того, что к результату прибавляется Мод. Алхимии.
        /// </summary>
        public bool IncludesAlchemyMod { get; set; }

        /// <summary>
        /// Текстовый префикс перед вычисленной формулой (например, "Исцеляет ").
        /// </summary>
        public string? Prefix { get; set; }
    }

    /// <summary>
    /// Конвертер для парсинга HerbTypeEnum из строки с поддержкой флагов через запятую (например, "HealingBase, PoisonModifier").
    /// </summary>
    internal class HerbTypeFlagsConverter : JsonConverter<HerbTypeEnum>
    {
        /// <summary>
        /// Читает значение HerbTypeEnum из JSON-строки.
        /// </summary>
        public override HerbTypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()
                ?? throw new JsonException("Значение HerbType не может быть null.");

            HerbTypeEnum result = HerbTypeEnum.None;
            var parts = value.Split(',', StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                if (Enum.TryParse<HerbTypeEnum>(part, true, out var parsed))
                {
                    result |= parsed;
                }
                else
                {
                    throw new JsonException($"Неизвестное значение HerbTypeEnum: '{part}'.");
                }
            }

            return result;
        }

        /// <summary>
        /// Записывает значение HerbTypeEnum в JSON-строку.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, HerbTypeEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
