using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts;
using PotionCraft.Repository;

namespace PotionCraft.Tests.Repository;

/// <summary>
/// Тесты оптимистичной конкурентности для сущности PlayerCharacter.
/// Проверяют, что при одновременном редактировании одного персонажа бросается DbUpdateConcurrencyException.
/// </summary>
public class PlayerCharacterConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public PlayerCharacterConcurrencyTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    /// <summary>
    /// Создаёт контекст базы данных с SQLite in-memory провайдером.
    /// </summary>
    private PotionCraftDbContext CreateSqliteContext()
    {
        var options = new DbContextOptionsBuilder<PotionCraftDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new PotionCraftDbContext(options);
    }

    /// <summary>
    /// Проверяет, что RowVersion обновляется при сохранении нового персонажа.
    /// </summary>
    [Fact]
    public async Task SaveChanges_NewCharacter_SetsRowVersion()
    {
        await using var ctx = CreateSqliteContext();
        await ctx.Database.EnsureCreatedAsync();

        var character = new PlayerCharacter
        {
            Id = Guid.NewGuid(),
            Name = "Тестовый",
            Intelligence = 10,
            Wisdom = 14,
            ProficiencyBonus = 2
        };

        await ctx.PlayerCharacters.AddAsync(character);
        await ctx.SaveChangesAsync();

        Assert.NotNull(character.RowVersion);
        Assert.NotEmpty(character.RowVersion);
    }

    /// <summary>
    /// Проверяет, что RowVersion изменяется при обновлении персонажа.
    /// </summary>
    [Fact]
    public async Task SaveChanges_UpdateCharacter_ChangesRowVersion()
    {
        Guid characterId;
        byte[] originalRowVersion;

        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "Тестовый",
                Intelligence = 10,
                Wisdom = 14,
                ProficiencyBonus = 2
            };

            await ctx.PlayerCharacters.AddAsync(character);
            await ctx.SaveChangesAsync();

            characterId = character.Id;
            originalRowVersion = character.RowVersion.ToArray();
        }

        await using (var ctx = CreateSqliteContext())
        {
            var character = await ctx.PlayerCharacters.FindAsync(characterId);
            character!.Name = "Обновлённый";
            await ctx.SaveChangesAsync();

            Assert.NotEqual(originalRowVersion, character.RowVersion);
        }
    }

    /// <summary>
    /// Проверяет, что при одновременном редактировании одного персонажа
    /// вторая попытка сохранения бросает DbUpdateConcurrencyException.
    /// </summary>
    [Fact]
    public async Task SaveChanges_ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        Guid characterId;

        // Создаём персонажа
        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "Конкурентный",
                Intelligence = 12,
                Wisdom = 16,
                ProficiencyBonus = 3
            };

            await ctx.PlayerCharacters.AddAsync(character);
            await ctx.SaveChangesAsync();
            characterId = character.Id;
        }

        // Два контекста читают одного и того же персонажа
        await using var ctx1 = CreateSqliteContext();
        await using var ctx2 = CreateSqliteContext();

        var character1 = await ctx1.PlayerCharacters.FindAsync(characterId);
        var character2 = await ctx2.PlayerCharacters.FindAsync(characterId);

        // Первый контекст обновляет персонажа
        character1!.Name = "Изменение_1";
        await ctx1.SaveChangesAsync();

        // Второй контекст пытается обновить того же персонажа со старым RowVersion
        character2!.Name = "Изменение_2";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => ctx2.SaveChangesAsync());
    }
}
