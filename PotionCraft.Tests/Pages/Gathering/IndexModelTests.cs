using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using PotionCraft.Contracts;
using PotionCraft.Contracts.Enums;
using PotionCraft.Pages.Gathering;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Tests.Pages.Gathering
{
    public class IndexModelTests
    {
        [Fact]
        public void OnGet_InitializesDefaults()
        {
            // Arrange
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            var model = new IndexModel(mockRepo.Object);

            // Act
            model.OnGet();

            // Assert
            Assert.True(model.Input.IsDay);
            Assert.False(model.Input.IsRain);
            Assert.False(model.Input.IsCave);
            Assert.Equal(20, model.Input.Difficulty);
            Assert.Equal(1, model.Input.RollsCount);
            Assert.NotNull(model.TerrainOptions);
        }

        [Fact]
        public async Task OnPostAsync_WithoutCharacterId_AddsModelError()
        {
            // Arrange
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            var model = new IndexModel(mockRepo.Object);
            model.Input.CharacterId = null;

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.False(model.ModelState.IsValid);
            Assert.Contains(model.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("выберите персонажа")));
        }

        [Fact]
        public async Task OnPostAsync_WithValidCharacter_CalculatesSuccesses()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new PlayerCharacter
            {
                Id = characterId,
                Name = "Тест Персонаж",
                Wisdom = 20, // Mod = +5
                ProficiencyBonus = 4,
                HerbalismTool = new Tool { Proficiency = true } // +9 total modifier
            };
            
            var mockRepo = new Mock<IPlayerCharacterRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(characterId)).ReturnsAsync(character);
            
            var model = new IndexModel(mockRepo.Object);
            model.Input.CharacterId = characterId;
            model.Input.Difficulty = 5; // Very low to guarantee successes usually
            model.Input.RollsCount = 3;

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.True(model.ModelState.IsValid);
            Assert.Equal("Тест Персонаж", model.CharacterName);
            Assert.NotNull(model.TotalSuccesses);
            Assert.Equal(3, model.RollResults.Count);
        }
    }
}
