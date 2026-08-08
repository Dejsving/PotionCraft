using Microsoft.EntityFrameworkCore;

using PotionCraft.Contracts.Enums;
using PotionCraft.InitData;
using PotionCraft.Repository;

namespace PotionCraft.Tests.InitData;

/// <summary>
/// Тесты для сервиса начального заполнения таблицы Herbs.
/// </summary>
public class HerbDataSeederTests
{
    /// <summary>
    /// Создаёт контекст базы данных с InMemory-провайдером.
    /// </summary>
    private static PotionCraftDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PotionCraftDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PotionCraftDbContext(options);
    }

    /// <summary>
    /// Проверяет, что загрузка трав из встроенного ресурса возвращает непустой список.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_ReturnsNonEmptyList()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        Assert.NotNull(herbs);
        Assert.NotEmpty(herbs);
    }

    /// <summary>
    /// Проверяет, что все загруженные травы имеют заполненное название.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_AllHerbsHaveNames()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        Assert.All(herbs, herb => Assert.False(string.IsNullOrWhiteSpace(herb.Name)));
    }

    /// <summary>
    /// Проверяет, что трава с флаговым типом "HealingModifier, PoisonModifier" корректно парсится.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_ParsesFlaggedHerbType()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        var chromiumSlime = herbs.FirstOrDefault(h => h.Name == "Хромовая слизь");
        Assert.NotNull(chromiumSlime);
        Assert.True(chromiumSlime.HerbType.HasFlag(HerbTypeEnum.HealingModifier));
        Assert.True(chromiumSlime.HerbType.HasFlag(HerbTypeEnum.PoisonModifier));
    }

    /// <summary>
    /// Проверяет, что SeedHerbsAsync заполняет пустую базу данных.
    /// </summary>
    [Fact]
    public async Task SeedHerbsAsync_SeedsEmptyDatabase()
    {
        using var context = CreateInMemoryContext();

        await HerbDataSeeder.SeedHerbsAsync(context);

        var count = await context.Herbs.CountAsync();
        Assert.True(count > 0);
    }

    /// <summary>
    /// Проверяет, что SeedHerbsAsync не дублирует данные при повторном вызове.
    /// </summary>
    [Fact]
    public async Task SeedHerbsAsync_DoesNotDuplicateOnSecondCall()
    {
        using var context = CreateInMemoryContext();

        await HerbDataSeeder.SeedHerbsAsync(context);
        var countAfterFirst = await context.Herbs.CountAsync();

        await HerbDataSeeder.SeedHerbsAsync(context);
        var countAfterSecond = await context.Herbs.CountAsync();

        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    /// <summary>
    /// Регрессия на баг со старыми данными в БД: SeedHerbsAsync должен обновлять поля уже засеянной травы
    /// (например, новые поля Formula*/DamageType), а не пропускать их, если таблица уже не пуста, и сохранять Id для ссылок из сумок персонажей.
    /// </summary>
    [Fact]
    public async Task SeedHerbsAsync_ExistingHerbWithStaleData_UpdatesFieldsAndKeepsId()
    {
        using var context = CreateInMemoryContext();

        var staleHerb = new PotionCraft.Contracts.Models.Herb
        {
            Id = Guid.NewGuid(),
            Name = "Корень дикого Шалфея",
            Effect = "Устаревший текст без формулы.",
            HerbType = HerbTypeEnum.HealingBase,
            Habitats = new Dictionary<TerrainEnum, int> { [TerrainEnum.Everewhere] = 5 },
        };
        var originalId = staleHerb.Id;
        await context.Herbs.AddAsync(staleHerb);
        await context.SaveChangesAsync();

        await HerbDataSeeder.SeedHerbsAsync(context);

        var updated = await context.Herbs.SingleAsync(h => h.Name == "Корень дикого Шалфея");
        Assert.Equal(originalId, updated.Id);
        Assert.Equal(2, updated.FormulaDiceCount);
        Assert.Equal(DiceTypeEnum.D4, updated.FormulaDiceType);
        Assert.Equal("Исцеляет 2к4 + Мод. Алхимии.", updated.Effect);
    }

    /// <summary>
    /// Проверяет, что у загруженных трав корректно заполнены среды обитания.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_HerbsHaveHabitats()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        Assert.All(herbs, herb => Assert.NotEmpty(herb.Habitats));
    }

    /// <summary>
    /// Проверяет, что у Хромовой слизи выставлен флаг RequiresDmJudgement.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_ChromeSlime_HasRequiresDmJudgement()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        var chromiumSlime = herbs.First(h => h.Name == "Хромовая слизь");
        Assert.True(chromiumSlime.RequiresDmJudgement);
    }

    /// <summary>
    /// Проверяет, что у Корня дикого Шалфея корректно распарсилась структурированная формула эффекта.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_WildSageRoot_HasFormula()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        var wildSageRoot = herbs.First(h => h.Name == "Корень дикого Шалфея");
        Assert.Equal(2, wildSageRoot.FormulaDiceCount);
        Assert.Equal(DiceTypeEnum.D4, wildSageRoot.FormulaDiceType);
        Assert.True(wildSageRoot.FormulaIncludesAlchemyMod);
        Assert.Equal("Исцеляет ", wildSageRoot.FormulaPrefix);
    }

    /// <summary>
    /// Проверяет, что у Лепестков Змееуста корректно распарсилась формула эффекта и тип урона.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_SnakemouthPetals_HasFormulaAndDamageType()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        var snakemouthPetals = herbs.First(h => h.Name == "Лепестки Змееуста");
        Assert.Equal(1, snakemouthPetals.FormulaDiceCount);
        Assert.Equal(DiceTypeEnum.D4, snakemouthPetals.FormulaDiceType);
        Assert.Equal(DamageTypeEnum.Poison, snakemouthPetals.DamageType);
    }

    /// <summary>
    /// Проверяет, что у модификаторов смены типа урона корректно распарсились варианты замены.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_DamageChangeModifiers_HaveReplacementDamageTypes()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        var arcticIvy = herbs.First(h => h.Name == "Арктический плющ");
        Assert.Equal([DamageTypeEnum.Cold, DamageTypeEnum.Necrotic], arcticIvy.ReplacementDamageTypes);

        var dracusFlowers = herbs.First(h => h.Name == "Цветы Дракуса");
        Assert.Equal([DamageTypeEnum.Fire, DamageTypeEnum.Acid], dracusFlowers.ReplacementDamageTypes);

        var glowingSyntoflower = herbs.First(h => h.Name == "Сияющий синтоцвет");
        Assert.Equal([DamageTypeEnum.Radiant], glowingSyntoflower.ReplacementDamageTypes);
    }
}
