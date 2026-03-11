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
    /// Проверяет, что трава с флаговым типом "Potion, Poison" корректно парсится.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_ParsesFlaggedHerbType()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        var chromiumSlime = herbs.FirstOrDefault(h => h.Name == "Хромовая слизь");
        Assert.NotNull(chromiumSlime);
        Assert.True(chromiumSlime.HerbType.HasFlag(HerbTypeEnum.Potion));
        Assert.True(chromiumSlime.HerbType.HasFlag(HerbTypeEnum.Poison));
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
    /// Проверяет, что у загруженных трав корректно заполнены среды обитания.
    /// </summary>
    [Fact]
    public void LoadHerbsFromResource_HerbsHaveHabitats()
    {
        var herbs = HerbDataSeeder.LoadHerbsFromResource();

        Assert.All(herbs, herb => Assert.NotEmpty(herb.Habitats));
    }
}
