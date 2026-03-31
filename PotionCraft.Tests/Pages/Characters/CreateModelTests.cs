using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using PotionCraft.Contracts;
using PotionCraft.Pages.Characters;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Tests.Pages.Characters
{
    /// <summary>
    /// Тесты для меню создания персонажа (CreateModel).
    /// </summary>
    public class CreateModelTests
    {
        /// <summary>
        /// Макет хранилища персонажей.
        /// </summary>
        private readonly Mock<IPlayerCharacterRepository> _mockRepository;

        /// <summary>
        /// Тестируемый экземпляр модели.
        /// </summary>
        private readonly CreateModel _model;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CreateModelTests"/>.
        /// </summary>
        public CreateModelTests()
        {
            _mockRepository = new Mock<IPlayerCharacterRepository>();
            _model = new CreateModel(_mockRepository.Object);
        }

        /// <summary>
        /// Проверяет, что OnPostAsync возвращает PageResult,
        /// когда состояние модели является недопустимым.
        /// </summary>
        [Fact]
        public async Task OnPostAsync_InvalidModelState_ReturnsPageResult()
        {
            // Arrange
            _model.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
        }

        /// <summary>
        /// Проверяет, что OnPostAsync добавляет персонажа и перенаправляет на страницу выбора
        /// с параметром autoSelect, содержащим идентификатор созданного персонажа.
        /// </summary>
        [Fact]
        public async Task OnPostAsync_ValidModel_AddsCharacterAndRedirectsToSelectWithAutoSelect()
        {
            // Arrange
            _model.Name = "Test Character";
            _model.Intelligence = 15;
            _model.Wisdom = 14;
            _model.ProficiencyBonus = 3;
            _model.HerbalismProficiencyLevel = 2; // Expertise
            _model.HerbalismToolModifier = 2;
            _model.AlchemistProficiencyLevel = 1; // Proficiency
            _model.AlchemistToolModifier = -1;
            _model.PoisonerProficiencyLevel = 0;  // None
            _model.PoisonerToolModifier = 0;

            Guid capturedId = Guid.Empty;
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<PlayerCharacter>()))
                           .Callback<PlayerCharacter>(c => capturedId = c.Id)
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _model.OnPostAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Select", redirectResult.PageName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues!.ContainsKey("autoSelect"));
            Assert.Equal(capturedId, redirectResult.RouteValues["autoSelect"]);
            Assert.NotEqual(Guid.Empty, capturedId);

            _mockRepository.Verify(repo => repo.AddAsync(It.Is<PlayerCharacter>(c =>
                c.Name == "Test Character" &&
                c.Intelligence == 15 &&
                c.Wisdom == 14 &&
                c.ProficiencyBonus == 3 &&
                c.HerbalismTool.Proficiency == true &&
                c.HerbalismTool.Expertise == true &&
                c.HerbalismTool.Modifier == 2 &&
                c.AlchemistTool.Proficiency == true &&
                c.AlchemistTool.Expertise == false &&
                c.AlchemistTool.Modifier == -1 &&
                c.PoisonerTool.Proficiency == false &&
                c.PoisonerTool.Expertise == false &&
                c.PoisonerTool.Modifier == 0
            )), Times.Once);
        }

        /// <summary>
        /// Проверяет, что OnPostAsync возвращает PageResult с ошибкой,
        /// когда репозиторий выдает исключение InvalidOperationException.
        /// </summary>
        [Fact]
        public async Task OnPostAsync_RepositoryThrowsInvalidOperationException_ReturnsPageResultWithModelError()
        {
            // Arrange
            _model.Name = "Duplicate name";
            var exceptionMessage = "Character already exists";
            
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<PlayerCharacter>()))
                           .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.True(_model.ModelState.ContainsKey("Name"));
            Assert.Equal(exceptionMessage, _model.ModelState["Name"]!.Errors[0].ErrorMessage);
        }
    }
}
