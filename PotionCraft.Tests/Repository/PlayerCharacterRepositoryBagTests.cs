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

    /// <summary>
    /// Проверяет, что мутация словаря Herbs in-place (без замены ссылки)
    /// корректно сохраняется в базу данных — добавление нового элемента напрямую.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_MutateHerbsDictionaryInPlace_PersistsChanges()
    {
        Guid characterId;
        var herbId = Guid.NewGuid();

        // Arrange — создаём персонажа без трав
        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "Торговец",
                Wisdom = 12,
                ProficiencyBonus = 2,
                HerbalismTool = new Tool { Proficiency = true }
            };

            characterId = character.Id;
            await ctx.PlayerCharacters.AddAsync(character);
            await ctx.SaveChangesAsync();
        }

        // Act — мутируем словарь in-place (как делает ShopController)
        await using (var ctx = CreateSqliteContext())
        {
            var repo = new PlayerCharacterRepository(ctx);
            var character = await repo.GetByIdAsync(characterId);

            character!.Bag.Herbs[herbId] = new GatheringResult
            {
                Herb = new Herb
                {
                    Id = herbId,
                    Name = "Огнецвет",
                    Rarity = RarityEnum.Rare
                },
                Quantity = 2
            };

            await repo.UpdateAsync(character);
        }

        // Assert — проверяем в новом контексте
        await using (var ctx = CreateSqliteContext())
        {
            var saved = await ctx.PlayerCharacters.FindAsync(characterId);

            Assert.NotNull(saved);
            Assert.Single(saved!.Bag.Herbs);
            Assert.True(saved.Bag.Herbs.ContainsKey(herbId));
            Assert.Equal(2, saved.Bag.Herbs[herbId].Quantity);
        }
    }

    /// <summary>
    /// Проверяет, что удаление элемента из словаря Herbs in-place
    /// корректно сохраняется в базу данных.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_RemoveHerbInPlace_PersistsChanges()
    {
        var herbId = Guid.NewGuid();
        Guid characterId;

        // Arrange — создаём персонажа с одной травой
        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "Продавец",
                Wisdom = 10,
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
                                    Name = "Лунный шалфей",
                                    Rarity = RarityEnum.Common
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

        // Act — удаляем траву из словаря in-place (как при продаже в ShopController)
        await using (var ctx = CreateSqliteContext())
        {
            var repo = new PlayerCharacterRepository(ctx);
            var character = await repo.GetByIdAsync(characterId);

            character!.Bag.Herbs.Remove(herbId);

            await repo.UpdateAsync(character);
        }

        // Assert — проверяем в новом контексте
        await using (var ctx = CreateSqliteContext())
        {
            var saved = await ctx.PlayerCharacters.FindAsync(characterId);

            Assert.NotNull(saved);
            Assert.Empty(saved!.Bag.Herbs);
        }
    }

    /// <summary>
    /// Проверяет, что изменение Coins и Herbs in-place одновременно
    /// корректно сохраняется — имитация полной торговой сделки.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_TradeScenario_CoinsAndHerbsInPlacePersisted()
    {
        var herbId = Guid.NewGuid();
        var newHerbId = Guid.NewGuid();
        Guid characterId;

        // Arrange — создаём персонажа с монетами и одной травой
        await using (var ctx = CreateSqliteContext())
        {
            await ctx.Database.EnsureCreatedAsync();

            var character = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                Name = "ТорговецПолный",
                Wisdom = 14,
                ProficiencyBonus = 3,
                HerbalismTool = new Tool { Proficiency = true },
                Bag = new CharacterBag
                {
                    Coins = 500,
                    Herbs = new Dictionary<Guid, GatheringResult>
                    {
                        {
                            herbId, new GatheringResult
                            {
                                Herb = new Herb
                                {
                                    Id = herbId,
                                    Name = "Кровьтрава",
                                    Rarity = RarityEnum.Common
                                },
                                Quantity = 10
                            }
                        }
                    }
                }
            };

            characterId = character.Id;
            await ctx.PlayerCharacters.AddAsync(character);
            await ctx.SaveChangesAsync();
        }

        // Act — продаём часть трав и покупаем новую (мутация in-place)
        await using (var ctx = CreateSqliteContext())
        {
            var repo = new PlayerCharacterRepository(ctx);
            var character = await repo.GetByIdAsync(characterId);

            // Продажа: уменьшаем количество
            character!.Bag.Herbs[herbId].Quantity -= 7;

            // Покупка: добавляем новый вид
            character.Bag.Herbs[newHerbId] = new GatheringResult
            {
                Herb = new Herb
                {
                    Id = newHerbId,
                    Name = "Ледяной корень",
                    Rarity = RarityEnum.Unusual
                },
                Quantity = 2
            };

            // Обновляем баланс
            character.Bag.Coins = 350;

            await repo.UpdateAsync(character);
        }

        // Assert — проверяем в новом контексте
        await using (var ctx = CreateSqliteContext())
        {
            var saved = await ctx.PlayerCharacters.FindAsync(characterId);

            Assert.NotNull(saved);
            Assert.Equal(350, saved!.Bag.Coins);
            Assert.Equal(2, saved.Bag.Herbs.Count);
            Assert.Equal(3, saved.Bag.Herbs[herbId].Quantity);
            Assert.Equal(2, saved.Bag.Herbs[newHerbId].Quantity);
        }
    }
}
