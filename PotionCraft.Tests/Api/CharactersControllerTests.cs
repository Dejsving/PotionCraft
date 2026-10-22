using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PotionCraft.Contracts;
using PotionCraft.Contracts.Models;
using PotionCraft.Server.Controllers;
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

    /// <summary>Идентификатор текущего пользователя, от имени которого выполняются запросы.</summary>
    private readonly Guid _playerId = Guid.NewGuid();

    /// <summary>
    /// Инициализирует тестовое окружение.
    /// </summary>
    public CharactersControllerTests()
    {
        _mockRepo = new Mock<IPlayerCharacterRepository>();
        _controller = new CharactersController(_mockRepo.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, _playerId.ToString())], "test"))
                }
            }
        };
    }

    // ─── Select ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что Select возвращает 200 OK с персонажем при успешном резервировании.
    /// </summary>
    [Fact]
    public async Task Select_SuccessfulReservation_Returns200WithCharacter()
    {
        var id = Guid.NewGuid();
        var playerId = _playerId;
        var character = new PlayerCharacter { Id = id, Name = "Герой", SelectedBy = playerId };
        _mockRepo.Setup(r => r.SelectCharacterAsync(id, playerId)).ReturnsAsync(true);
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(character);

        var result = await _controller.Select(id);

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
        var playerId = _playerId;
        _mockRepo.Setup(r => r.SelectCharacterAsync(id, playerId)).ReturnsAsync(false);

        var result = await _controller.Select(id);

        Assert.IsType<ConflictObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что при возврате 409 GetByIdAsync не вызывается.
    /// </summary>
    [Fact]
    public async Task Select_CharacterAlreadyTaken_DoesNotCallGetById()
    {
        var id = Guid.NewGuid();
        var playerId = _playerId;
        _mockRepo.Setup(r => r.SelectCharacterAsync(id, playerId)).ReturnsAsync(false);

        await _controller.Select(id);

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
        var playerId = _playerId;
        _mockRepo.Setup(r => r.DeselectCharacterAsync(id, playerId)).Returns(Task.CompletedTask);

        var result = await _controller.Deselect(id);

        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.DeselectCharacterAsync(id, playerId), Times.Once);
    }

    /// <summary>
    /// Проверяет, что Select возвращает 401, если идентификатор пользователя отсутствует в запросе.
    /// </summary>
    [Fact]
    public async Task Select_NoUserIdClaim_Returns401AndDoesNotCallRepository()
    {
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await _controller.Select(Guid.NewGuid());

        Assert.IsType<UnauthorizedObjectResult>(result);
        _mockRepo.Verify(r => r.SelectCharacterAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что Select передаёт в репозиторий идентификатор текущего пользователя из claim.
    /// </summary>
    [Fact]
    public async Task Select_UsesCurrentUserIdFromClaims()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.SelectCharacterAsync(id, _playerId)).ReturnsAsync(false);

        await _controller.Select(id);

        _mockRepo.Verify(r => r.SelectCharacterAsync(id, _playerId), Times.Once);
    }

    // ─── GetBag ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что GetBag возвращает 200 OK с содержимым сумки для существующего персонажа.
    /// </summary>
    [Fact]
    public async Task GetBag_ExistingCharacter_Returns200WithBag()
    {
        var id = Guid.NewGuid();
        var character = new PlayerCharacter { Id = id, Name = "Герой" };

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(character);

        var result = await _controller.GetBag(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var bag = Assert.IsType<CharacterBag>(okResult.Value);
        Assert.NotNull(bag);
        Assert.Empty(bag.Herbs);
        Assert.Empty(bag.Potions);
        Assert.Empty(bag.Poisons);
    }

    /// <summary>
    /// Проверяет, что GetBag возвращает 404 NotFound для несуществующего персонажа.
    /// </summary>
    [Fact]
    public async Task GetBag_NonExistingCharacter_Returns404()
    {
        var id = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((PlayerCharacter?)null);

        var result = await _controller.GetBag(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что GetBag возвращает сумку с травами, если они есть.
    /// </summary>
    [Fact]
    public async Task GetBag_CharacterWithHerbs_ReturnsBagWithHerbs()
    {
        var id = Guid.NewGuid();
        var herb = new Herb { Id = Guid.NewGuid(), Name = "Тысячелистник" };
        var character = new PlayerCharacter { Id = id, Name = "Герой" };
        character.Bag.Herbs.Add(herb.Id, new GatheringResult { Herb = herb, Quantity = 5 });

        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(character);

        var result = await _controller.GetBag(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var bag = Assert.IsType<CharacterBag>(okResult.Value);
        Assert.Single(bag.Herbs);
        Assert.Equal(5, bag.Herbs[herb.Id].Quantity);
    }
}

