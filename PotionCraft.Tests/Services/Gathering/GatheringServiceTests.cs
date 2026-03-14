using Moq;
using PotionCraft.Contracts.Enums;
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

        _gatheringService = new GatheringService(_herbRepositoryMock.Object);
    }

    [Fact]
    public async Task GatherHerbAsync_DefaultRequest_ReturnsValidResult()
    {
        // Arrange
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
        var request = new GatheringRequest
        {
            Terrain = TerrainEnum.Everewhere,
            HasProvisions = false // Without provisions BloodGrass requires reroll
        };

        // Act
        // Run enough times to be reasonably sure BloodGrass is never returned
        for (int i = 0; i < 50; i++)
        {
            var result = await _gatheringService.GatherHerbAsync(request);
            Assert.NotEqual("Кровьтрава", result.Herb?.Name);
        }
    }
}