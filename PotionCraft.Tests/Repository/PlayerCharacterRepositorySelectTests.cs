using Microsoft.EntityFrameworkCore;

using PotionCraft.Contracts;
using PotionCraft.Repository;
using PotionCraft.Repository.Repositories;

namespace PotionCraft.Tests.Repository;

/// <summary>
/// Тесты для методов SelectCharacterAsync и DeselectCharacterAsync репозитория персонажей.
/// </summary>
public class PlayerCharacterRepositorySelectTests
{
    /// <summary>
    /// Создаёт контекст базы данных с InMemory-провайдером и уникальным именем.
    /// </summary>
    private static PotionCraftDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PotionCraftDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PotionCraftDbContext(options);
    }

    /// <summary>
    /// Создаёт тестового персонажа с заданным значением SelectedBy.
    /// </summary>
    private static PlayerCharacter CreateCharacter(Guid? selectedBy = null) =>
        new() { Id = Guid.NewGuid(), Name = "Тест", SelectedBy = selectedBy };

    // ─── SelectCharacterAsync ─────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что SelectCharacterAsync возвращает true и сохраняет playerId, когда персонаж свободен.
    /// </summary>
    [Fact]
    public async Task SelectCharacterAsync_FreeCharacter_ReturnsTrueAndSetsPlayerId()
    {
        await using var context = CreateInMemoryContext();
        var character = CreateCharacter();
        await context.PlayerCharacters.AddAsync(character);
        await context.SaveChangesAsync();
        var playerId = Guid.NewGuid();

        var repo = new PlayerCharacterRepository(context);
        var result = await repo.SelectCharacterAsync(character.Id, playerId);

        Assert.True(result);
        var saved = await context.PlayerCharacters.FindAsync(character.Id);
        Assert.Equal(playerId, saved!.SelectedBy);
    }

    /// <summary>
    /// Проверяет, что SelectCharacterAsync возвращает false, когда персонаж уже занят другим игроком.
    /// </summary>
    [Fact]
    public async Task SelectCharacterAsync_TakenByOtherPlayer_ReturnsFalse()
    {
        await using var context = CreateInMemoryContext();
        var otherId = Guid.NewGuid();
        var character = CreateCharacter(selectedBy: otherId);
        await context.PlayerCharacters.AddAsync(character);
        await context.SaveChangesAsync();

        var repo = new PlayerCharacterRepository(context);
        var result = await repo.SelectCharacterAsync(character.Id, Guid.NewGuid());

        Assert.False(result);
    }

    /// <summary>
    /// Проверяет, что SelectCharacterAsync возвращает false для несуществующего персонажа.
    /// </summary>
    [Fact]
    public async Task SelectCharacterAsync_NonExistentCharacter_ReturnsFalse()
    {
        await using var context = CreateInMemoryContext();
        var repo = new PlayerCharacterRepository(context);

        var result = await repo.SelectCharacterAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    /// <summary>
    /// Проверяет, что SelectCharacterAsync автоматически освобождает предыдущего персонажа игрока.
    /// </summary>
    [Fact]
    public async Task SelectCharacterAsync_PlayerSwitchesCharacter_ReleasesPrevious()
    {
        await using var context = CreateInMemoryContext();
        var playerId = Guid.NewGuid();
        var charA = CreateCharacter(selectedBy: playerId);
        var charB = CreateCharacter();
        await context.PlayerCharacters.AddRangeAsync(charA, charB);
        await context.SaveChangesAsync();

        var repo = new PlayerCharacterRepository(context);
        var result = await repo.SelectCharacterAsync(charB.Id, playerId);

        Assert.True(result);
        var savedA = await context.PlayerCharacters.FindAsync(charA.Id);
        var savedB = await context.PlayerCharacters.FindAsync(charB.Id);
        Assert.Null(savedA!.SelectedBy);
        Assert.Equal(playerId, savedB!.SelectedBy);
    }

    /// <summary>
    /// Проверяет, что SelectCharacterAsync идемпотентен для того же игрока (повторный выбор).
    /// </summary>
    [Fact]
    public async Task SelectCharacterAsync_SamePlayerReselects_ReturnsTrue()
    {
        await using var context = CreateInMemoryContext();
        var playerId = Guid.NewGuid();
        var character = CreateCharacter(selectedBy: playerId);
        await context.PlayerCharacters.AddAsync(character);
        await context.SaveChangesAsync();

        var repo = new PlayerCharacterRepository(context);
        var result = await repo.SelectCharacterAsync(character.Id, playerId);

        Assert.True(result);
    }

    // ─── DeselectCharacterAsync ───────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что DeselectCharacterAsync очищает SelectedBy, если персонаж принадлежит игроку.
    /// </summary>
    [Fact]
    public async Task DeselectCharacterAsync_OwnedCharacter_ClearsSelectedBy()
    {
        await using var context = CreateInMemoryContext();
        var playerId = Guid.NewGuid();
        var character = CreateCharacter(selectedBy: playerId);
        await context.PlayerCharacters.AddAsync(character);
        await context.SaveChangesAsync();

        var repo = new PlayerCharacterRepository(context);
        await repo.DeselectCharacterAsync(character.Id, playerId);

        var saved = await context.PlayerCharacters.FindAsync(character.Id);
        Assert.Null(saved!.SelectedBy);
    }

    /// <summary>
    /// Проверяет, что DeselectCharacterAsync не снимает выбор, если персонаж принадлежит другому игроку.
    /// </summary>
    [Fact]
    public async Task DeselectCharacterAsync_OtherPlayersCharacter_DoesNotChange()
    {
        await using var context = CreateInMemoryContext();
        var ownerId = Guid.NewGuid();
        var character = CreateCharacter(selectedBy: ownerId);
        await context.PlayerCharacters.AddAsync(character);
        await context.SaveChangesAsync();

        var repo = new PlayerCharacterRepository(context);
        await repo.DeselectCharacterAsync(character.Id, Guid.NewGuid());

        var saved = await context.PlayerCharacters.FindAsync(character.Id);
        Assert.Equal(ownerId, saved!.SelectedBy);
    }

    /// <summary>
    /// Проверяет, что DeselectCharacterAsync не выбрасывает исключение для несуществующего персонажа.
    /// </summary>
    [Fact]
    public async Task DeselectCharacterAsync_NonExistentCharacter_DoesNotThrow()
    {
        await using var context = CreateInMemoryContext();
        var repo = new PlayerCharacterRepository(context);

        var exception = await Record.ExceptionAsync(() =>
            repo.DeselectCharacterAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Null(exception);
    }
}

