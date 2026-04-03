using Moq;
using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Models;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Services.Gathering;

namespace PotionCraft.Tests.Services.Gathering;

/// <summary>
/// Тесты для сервиса сбора трав
/// </summary>
public class GatheringServiceTests
{
    private readonly Mock<IHerbRepository> _herbRepositoryMock;
    private readonly Mock<IDiceRoller> _diceRollerMock;
    private readonly GatheringService _gatheringService;

    public GatheringServiceTests()
    {
        _herbRepositoryMock = new Mock<IHerbRepository>();
        _herbRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Herb>
            {
                new Herb { Name = "Элементальная вода" },
                new Herb { Name = "Корень Мандрагоры" },
                new Herb { Name = "Кровьтрава" }
            });

        _diceRollerMock = new Mock<IDiceRoller>();

        _gatheringService = new GatheringService(_herbRepositoryMock.Object, _diceRollerMock.Object);
    }

    [Fact]
    public async Task GatherHerbAsync_DefaultRequest_ReturnsValidResult()
    {
        // Arrange
        // TwoD6 возвращает 7 (Кровьтрава), D4 возвращает 2 для количества
        _diceRollerMock.Setup(d => d.Roll(It.Is<DiceRoll>(dr => dr.Sides == 6 && dr.Count == 2)))
            .Returns(7);
        _diceRollerMock.Setup(d => d.Roll(It.Is<DiceRoll>(dr => dr.Sides == 4 && dr.Count == 1)))
            .Returns(2);

        var request = new GatheringRequest
        {
            Terrain = TerrainEnum.Everewhere,
            HasProvisions = true
        };

        // Act
        var result = await _gatheringService.GatherHerbAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Herb);
        Assert.False(string.IsNullOrEmpty(result.Herb.Name));
        Assert.True(result.Quantity > 0);
    }

    [Fact]
    public async Task GatherHerbAsync_ReRollIfNoProvisions_DoesNotReturnBloodGrassWhenNoProvisions()
    {
        // Arrange
        // Первый бросок — 7 (Кровьтрава, перебрасывается без припасов), второй — 5 (Корень дикого Шалфея)
        var rollSequence = new Queue<int>(new[] { 7, 5 });
        _diceRollerMock.Setup(d => d.Roll(It.Is<DiceRoll>(dr => dr.Sides == 6 && dr.Count == 2)))
            .Returns(() => rollSequence.Dequeue());
        _diceRollerMock.Setup(d => d.Roll(It.Is<DiceRoll>(dr => dr.Sides == 4 && dr.Count == 1)))
            .Returns(2);

        var request = new GatheringRequest
        {
            Terrain = TerrainEnum.Everewhere,
            HasProvisions = false
        };

        // Act
        var result = await _gatheringService.GatherHerbAsync(request);

        // Assert
        Assert.NotEqual("Кровьтрава", result.Herb?.Name);
    }
}