using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using PotionCraft.Contracts;
using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;
using PotionCraft.Contracts.Models;
using PotionCraft.Pages.Gathering;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Tests.Pages.Gathering
{
    public class IndexModelTests
    {
        /// <summary>
        /// Создаёт мок IDiceRoller с предсказуемым значением для D20.
        /// </summary>
        private static Mock<IDiceRoller> CreateDiceRollerMock(int d20Result = 10)
        {
            var mock = new Mock<IDiceRoller>();
            mock.Setup(d => d.Roll(It.Is<DiceRoll>(dr => dr.Sides == 20 && dr.Count == 1)))
                .Returns(d20Result);
            return mock;
        }

        [Fact]
        public void OnGet_InitializesDefaults()
        {
            // Arrange
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            gatheringServiceMock.Setup(s => s.GatherHerbAsync(It.IsAny<PotionCraft.Contracts.Models.GatheringRequest>())).ReturnsAsync(new PotionCraft.Contracts.Models.GatheringResult { Herb = new PotionCraft.Contracts.Models.Herb { Name = "Трава" }, Quantity = 1 });
            var diceRollerMock = CreateDiceRollerMock();
            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, diceRollerMock.Object);

            // Act
            model.OnGet();

            // Assert
            Assert.True(model.Input.IsDay);
            Assert.False(model.Input.IsRain);
            Assert.False(model.Input.IsCave);
            Assert.Equal(20, model.Input.Difficulty);
            Assert.Equal(1, model.Input.RollsCount);
            Assert.True(model.Input.IsProvisionsUsed);
            Assert.NotNull(model.TerrainOptions);
        }

        [Fact]
        public async Task OnPostAsync_WithoutCharacterId_AddsModelError()
        {
            // Arrange
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            gatheringServiceMock.Setup(s => s.GatherHerbAsync(It.IsAny<PotionCraft.Contracts.Models.GatheringRequest>())).ReturnsAsync(new PotionCraft.Contracts.Models.GatheringResult { Herb = new PotionCraft.Contracts.Models.Herb { Name = "Трава" }, Quantity = 1 });
            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);
            model.Input.CharacterId = null;

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.False(model.ModelState.IsValid);
            Assert.Contains(model.ModelState.Values,
                v => v.Errors.Any(e => e.ErrorMessage.Contains("выберите персонажа")));
        }

        [Fact]
        public async Task OnPostAsync_WithValidCharacter_CalculatesSuccesses()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new PlayerCharacter
            {
                Id = characterId,
                Name = "РўРµСЃС‚ РџРµСЂСЃРѕРЅР°Р¶",
                Wisdom = 20, // Mod = +5
                ProficiencyBonus = 4,
                HerbalismTool = new Tool { Proficiency = true } // +9 total modifier
            };
            
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(characterId)).ReturnsAsync(character);
            
            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            gatheringServiceMock.Setup(s => s.GatherHerbAsync(It.IsAny<PotionCraft.Contracts.Models.GatheringRequest>())).ReturnsAsync(new PotionCraft.Contracts.Models.GatheringResult { Herb = new PotionCraft.Contracts.Models.Herb { Name = "Трава" }, Quantity = 1 });
            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);
            model.Input.CharacterId = characterId;
            model.Input.Difficulty = 5; // Very low to guarantee successes usually
            model.Input.RollsCount = 3;

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.True(model.ModelState.IsValid);
            Assert.Equal("РўРµСЃС‚ РџРµСЂСЃРѕРЅР°Р¶", model.CharacterName);
            Assert.NotNull(model.TotalSuccesses);
            Assert.Equal(3, model.RollResults.Count);
        }

        [Fact]
        public void GetClipboardText_GroupsAndSortsProperly()
        {
            // Arrange
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);

            var herb1 = new PotionCraft.Contracts.Models.Herb
            {
                Id = Guid.NewGuid(), Name = "Ромашка", Rarity = RarityEnum.Common
            };
            var herb2 = new PotionCraft.Contracts.Models.Herb
            {
                Id = Guid.NewGuid(), Name = "Шалфей", Rarity = RarityEnum.Common
            };
            var herb3 = new PotionCraft.Contracts.Models.Herb
            {
                Id = Guid.NewGuid(), Name = "Женьшень", Rarity = RarityEnum.Rare
            };

            model.GatheredHerbs = new Dictionary<Guid, PotionCraft.Contracts.Models.GatheringResult>
            {
                { herb1.Id, new() { Herb = herb1, Quantity = 2 } },
                { herb2.Id, new() { Herb = herb2, Quantity = 3 } },
                { herb3.Id, new() { Herb = herb3, Quantity = 1 } }
            };

            // Act
            var clipboardText = model.GetClipboardText();

            // Assert
            var expected = "Обычный:\n- Ромашка 2\n- Шалфей 3\nРедкий:\n- Женьшень 1";
            
            Assert.Equal(expected, clipboardText.Replace("\r\n", "\n"));
        }

        [Fact]
        public void GetClipboardText_ReturnsEmpty_WhenNoHerbs()
        {
            // Arrange
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);

            model.GatheredHerbs = new Dictionary<Guid, PotionCraft.Contracts.Models.GatheringResult>();

            // Act
            var clipboardText = model.GetClipboardText();

            // Assert
            Assert.Equal(string.Empty, clipboardText);
        }

        [Fact]
        public async Task OnPostAsync_WithSuccesses_AddsHerbsToBag()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new PlayerCharacter
            {
                Id = characterId,
                Name = "Тест Персонаж",
                Wisdom = 20,
                ProficiencyBonus = 4,
                HerbalismTool = new Tool { Proficiency = true }
            };

            var herb = new Herb
            {
                Id = Guid.NewGuid(),
                Name = "Кровьтрава",
                Rarity = RarityEnum.Common
            };

            var mockRepo = new Mock<IPlayerCharacterRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(characterId)).ReturnsAsync(character);
            mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            gatheringServiceMock
                .Setup(s => s.GatherHerbAsync(It.IsAny<GatheringRequest>()))
                .ReturnsAsync(new GatheringResult { Herb = herb, Quantity = 1 });

            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);
            model.Input.CharacterId = characterId;
            model.Input.Difficulty = 1;
            model.Input.RollsCount = 1;

            // Act
            await model.OnPostAsync();

            // Assert
            Assert.True(character.Bag.Herbs.ContainsKey(herb.Id));
            Assert.Equal(character.Bag.Herbs[herb.Id].Quantity, model.GatheredHerbs[herb.Id].Quantity);
            mockRepo.Verify(repo => repo.UpdateAsync(character), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_WithSuccesses_AddsToExistingHerbsInBag()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var herb = new Herb
            {
                Id = Guid.NewGuid(),
                Name = "Кровьтрава",
                Rarity = RarityEnum.Common
            };

            var character = new PlayerCharacter
            {
                Id = characterId,
                Name = "Тест Персонаж",
                Wisdom = 20,
                ProficiencyBonus = 4,
                HerbalismTool = new Tool { Proficiency = true },
                Bag = new CharacterBag
                {
                    Herbs = new Dictionary<Guid, GatheringResult>
                    {
                        { herb.Id, new GatheringResult { Herb = herb, Quantity = 7 } }
                    }
                }
            };

            var mockRepo = new Mock<IPlayerCharacterRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(characterId)).ReturnsAsync(character);
            mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<PlayerCharacter>())).Returns(Task.CompletedTask);

            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            gatheringServiceMock
                .Setup(s => s.GatherHerbAsync(It.IsAny<GatheringRequest>()))
                .ReturnsAsync(new GatheringResult { Herb = herb, Quantity = 7 });

            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);
            model.Input.CharacterId = characterId;
            model.Input.Difficulty = 1;
            model.Input.RollsCount = 1;

            // Act
            await model.OnPostAsync();

            // Assert — было 7, выпало 7, должно стать 14
            Assert.Equal(14, character.Bag.Herbs[herb.Id].Quantity);
            mockRepo.Verify(repo => repo.UpdateAsync(character), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_WithNoSuccesses_DoesNotUpdateBag()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new PlayerCharacter
            {
                Id = characterId,
                Name = "Тест Персонаж",
                Wisdom = 6,
                ProficiencyBonus = 0,
                HerbalismTool = new Tool { Proficiency = false }
            };

            var mockRepo = new Mock<IPlayerCharacterRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(characterId)).ReturnsAsync(character);

            var gatheringServiceMock = new Mock<PotionCraft.Services.Gathering.IGatheringService>();
            var model = new IndexModel(mockRepo.Object, gatheringServiceMock.Object, CreateDiceRollerMock().Object);
            model.Input.CharacterId = characterId;
            model.Input.Difficulty = 30;
            model.Input.RollsCount = 1;

            // Act
            await model.OnPostAsync();

            // Assert
            Assert.Empty(character.Bag.Herbs);
            mockRepo.Verify(repo => repo.UpdateAsync(It.IsAny<PlayerCharacter>()), Times.Never);
        }
    }
}
