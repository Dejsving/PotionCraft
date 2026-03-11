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
    /// Заполняет таблицу Herbs данными из встроенного ресурса Herbs.json, если таблица пуста.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public static async Task SeedHerbsAsync(PotionCraftDbContext dbContext)
    {
        if (await dbContext.Herbs.AnyAsync())
        {
            return;
        }

        var herbs = LoadHerbsFromResource();
        await dbContext.Herbs.AddRangeAsync(herbs);
        await dbContext.SaveChangesAsync();
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
            Difficulty = dto.Difficulty,
            Habitats = dto.Habitats,
            AdditionalRule = dto.AdditionalRule
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
        /// Сложность.
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        /// Среда обитания и значения бросков.
        /// </summary>
        public Dictionary<TerrainEnum, int> Habitats { get; set; } = new();

        /// <summary>
        /// Дополнительные правила по экосистемам.
        /// </summary>
        public Dictionary<TerrainEnum, int> AdditionalRule { get; set; } = new();
    }

    /// <summary>
    /// Конвертер для парсинга HerbTypeEnum из строки с поддержкой флагов через запятую (например, "Potion, Poison").
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
