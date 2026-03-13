using Microsoft.AspNetCore.Mvc;
using Moq;
using PotionCraft.Contracts;
using PotionCraft.Controllers;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Tests.Api;

/// <summary>
/// Тесты для CharactersController (select/deselect).
/// </summary>
public class CharactersControllerTests
{
    /// <summary>Макет репозитория персонажей.</summary>
    private readonly Mock<IPlayerCharacterRepository> _mockRepo;

    /// <summary>Тестируемый контроллер.</summary>
    private readonly CharactersController _controller;

    /// <summary>
    /// Инициализирует тестовое окружение.
    /// </summary>
    public CharactersControllerTests()
    {
        _mockRepo = new Mock<IPlayerCharacterRepository>();
        _controller = new CharactersController(_mockRepo.Object);
    }

    // ─── Select ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что Select возвращает 200 OK с персонажем при успешном резервировании.
    /// </summary>
    [Fact]
    public async Task Select_SuccessfulReservation_Returns200WithCharacter()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var character = new PlayerCharacter { Id = id, Name = "Герой", SelectedBy = playerId };
        var request = new SelectCharacterRequest(playerId);

        _mockRepo.Setup(r => r.SelectCharacterAsync(id, playerId)).ReturnsAsync(true);
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(character);

        var result = await _controller.Select(id, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(character, okResult.Value);
    }

    /// <summary>
    /// Проверяет, что Select возвращает 409 Conflict, когда персонаж уже занят другим игроком.
    /// </summary>
    [Fact]
    public async Task Select_CharacterAlreadyTakenByOther_Returns409Conflict()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var request = new SelectCharacterRequest(playerId);

        _mockRepo.Setup(r => r.SelectCharacterAsync(id, playerId)).ReturnsAsync(false);

        var result = await _controller.Select(id, request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что при возврате 409 GetByIdAsync не вызывается.
    /// </summary>
    [Fact]
    public async Task Select_CharacterAlreadyTaken_DoesNotCallGetById()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var request = new SelectCharacterRequest(playerId);

        _mockRepo.Setup(r => r.SelectCharacterAsync(id, playerId)).ReturnsAsync(false);

        await _controller.Select(id, request);

        _mockRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    // ─── Deselect ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что Deselect возвращает 200 OK и вызывает DeselectCharacterAsync с правильным playerId.
    /// </summary>
    [Fact]
    public async Task Deselect_ValidRequest_Returns200AndCallsRepository()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var request = new DeselectCharacterRequest(playerId);

        _mockRepo.Setup(r => r.DeselectCharacterAsync(id, playerId)).Returns(Task.CompletedTask);

        var result = await _controller.Deselect(id, request);

        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.DeselectCharacterAsync(id, playerId), Times.Once);
    }
}

