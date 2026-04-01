using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository;
using PotionCraft.Repository.Repositories;

namespace PotionCraft.Tests.Repository;

/// <summary>
/// Интеграционные тесты сохранения сумки персонажа через реальный SQLite-провайдер.
/// InMemory-провайдер не применяет ValueConversion, поэтому используем SQLite.
/// </summary>
public class PlayerCharacterRepositoryBagTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public PlayerCharacterRepositoryBagTests()
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
    /// Проверяет, что травы, добавленные в сумку через новый экземпляр словаря,
    /// корректно сохраняются и считываются из базы данных.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithNewHerbsInBag_PersistsHerbsToDatabase()
    {
        // Arrange — создаём схему и персонажа
        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "Травник",
                Wisdom = 16,
                ProficiencyBonus = 3,
                HerbalismTool = new Tool { Proficiency = true }
            };

            await ctx.PlayerCharacters.AddAsync(character);
            await ctx.SaveChangesAsync();
        }

        Guid characterId;
        var herbId = Guid.NewGuid();

        // Act — читаем персонажа, добавляем траву в сумку, сохраняем
        await using (var ctx = CreateSqliteContext())
        {
            var repo = new PlayerCharacterRepository(ctx);
            var character = await repo.GetByIdAsync(
                (await ctx.PlayerCharacters.FirstAsync()).Id);

            characterId = character!.Id;

            // Имитируем логику AddHerbsToBag: создаём новый словарь
            var updatedHerbs = new Dictionary<Guid, GatheringResult>(character.Bag.Herbs);
            updatedHerbs.Add(herbId, new GatheringResult
            {
                Herb = new Herb
                {
                    Id = herbId,
                    Name = "Кровьтрава",
                    Rarity = RarityEnum.Common
                },
                Quantity = 3
            });
            character.Bag.Herbs = updatedHerbs;

            await repo.UpdateAsync(character);
        }

        // Assert — читаем в новом контексте
        await using (var ctx = CreateSqliteContext())
        {
            var saved = await ctx.PlayerCharacters.FindAsync(characterId);

            Assert.NotNull(saved);
            Assert.Single(saved!.Bag.Herbs);
            Assert.True(saved.Bag.Herbs.ContainsKey(herbId));
            Assert.Equal(3, saved.Bag.Herbs[herbId].Quantity);
            Assert.Equal("Кровьтрава", saved.Bag.Herbs[herbId].Herb!.Name);
        }
    }

    /// <summary>
    /// Проверяет, что при добавлении трав к уже существующим в сумке,
    /// количество корректно суммируется и сохраняется в базе данных.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_AddingToExistingHerbsInBag_SumsQuantityCorrectly()
    {
        var herbId = Guid.NewGuid();
        Guid characterId;

        // Arrange — создаём персонажа с 5 травами в сумке
        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "Алхимик",
                Wisdom = 14,
                ProficiencyBonus = 2,
                HerbalismTool = new Tool { Proficiency = true },
                Bag = new CharacterBag
                {
                    Herbs = new Dictionary<Guid, GatheringResult>
                    {
                        {
                            herbId, new GatheringResult
                            {
                                Herb = new Herb
                                {
                                    Id = herbId,
                                    Name = "Мандрагора",
                                    Rarity = RarityEnum.Unusual
                                },
                                Quantity = 5
                            }
                        }
                    }
                }
            };

            characterId = character.Id;
            await ctx.PlayerCharacters.AddAsync(character);
            await ctx.SaveChangesAsync();
        }

        // Act — читаем персонажа, увеличиваем количество, сохраняем
        await using (var ctx = CreateSqliteContext())
        {
            var repo = new PlayerCharacterRepository(ctx);
            var character = await repo.GetByIdAsync(characterId);

            // Имитируем логику AddHerbsToBag с суммированием
            var updatedHerbs = new Dictionary<Guid, GatheringResult>(character!.Bag.Herbs);
            updatedHerbs[herbId].Quantity += 3;
            character.Bag.Herbs = updatedHerbs;

            await repo.UpdateAsync(character);
        }

        // Assert — проверяем в новом контексте
        await using (var ctx = CreateSqliteContext())
        {
            var saved = await ctx.PlayerCharacters.FindAsync(characterId);

            Assert.NotNull(saved);
            Assert.Single(saved!.Bag.Herbs);
            Assert.Equal(8, saved.Bag.Herbs[herbId].Quantity);
        }
    }
}
