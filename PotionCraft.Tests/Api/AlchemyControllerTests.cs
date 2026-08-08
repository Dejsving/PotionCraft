using Microsoft.AspNetCore.Mvc;
using Moq;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Contracts.Services;
using PotionCraft.Repository.Abstraction;
using PotionCraft.Server.Controllers;

namespace PotionCraft.Tests.Api;

/// <summary>
/// Тесты для AlchemyController (расчёт итогового эффекта зелья).
/// </summary>
public class AlchemyControllerTests
{
    /// <summary>Макет репозитория трав.</summary>
    private readonly Mock<IHerbRepository> _mockHerbRepo;

    /// <summary>Тестируемый контроллер (с реальным резолвером, т.к. он не имеет внешних зависимостей).</summary>
    private readonly AlchemyController _controller;

    /// <summary>
    /// Инициализирует тестовое окружение.
    /// </summary>
    public AlchemyControllerTests()
    {
        _mockHerbRepo = new Mock<IHerbRepository>();
        _controller = new AlchemyController(_mockHerbRepo.Object, new PotionEffectResolver());
    }

    /// <summary>
    /// Создаёт тестовую траву-основу с формулой эффекта.
    /// </summary>
    private static Herb CreateBaseHerb()
    {
        return new Herb
        {
            Id = Guid.NewGuid(),
            Name = "Корень дикого Шалфея",
            HerbType = HerbTypeEnum.HealingBase,
            Effect = "Исцеляет 2к4 + Мод. Алхимии.",
            FormulaDiceCount = 2,
            FormulaDiceType = DiceTypeEnum.D4,
            FormulaIncludesAlchemyMod = true,
        };
    }

    /// <summary>
    /// Создаёт тестовую траву-модификатор, повышающую тип кости.
    /// </summary>
    private static Herb CreateUpgradeDiceModifier()
    {
        return new Herb
        {
            Id = Guid.NewGuid(),
            Name = "Сушеная Эфедра",
            HerbType = HerbTypeEnum.HealingModifier,
            ModifierEffect = HerbModifierEffectEnum.HealingUpgradeDiceType,
            Effect = "Увеличивает тип кости на 1 за любой исцеляющий эффект.",
        };
    }

    /// <summary>
    /// Проверяет, что при неизвестном id травы-основы возвращается 404.
    /// </summary>
    [Fact]
    public async Task Resolve_UnknownBaseHerbId_ReturnsNotFound()
    {
        var baseHerbId = Guid.NewGuid();
        _mockHerbRepo.Setup(r => r.GetByIdAsync(baseHerbId)).ReturnsAsync((Herb?)null);

        var result = await _controller.Resolve(new ResolvePotionEffectRequest { BaseHerbId = baseHerbId });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что при известной основе и модификаторе возвращается 200 с корректно вычисленным результатом.
    /// </summary>
    [Fact]
    public async Task Resolve_ValidBaseAndModifier_ReturnsOkWithComputedResult()
    {
        var baseHerb = CreateBaseHerb();
        var modifier = CreateUpgradeDiceModifier();
        _mockHerbRepo.Setup(r => r.GetByIdAsync(baseHerb.Id)).ReturnsAsync(baseHerb);
        _mockHerbRepo.Setup(r => r.GetByIdAsync(modifier.Id)).ReturnsAsync(modifier);

        var result = await _controller.Resolve(new ResolvePotionEffectRequest
        {
            BaseHerbId = baseHerb.Id,
            ModifierHerbIds = [modifier.Id],
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var effectResult = Assert.IsType<PotionEffectResult>(okResult.Value);
        Assert.Equal(DiceTypeEnum.D6, effectResult.DiceType);
    }
}
