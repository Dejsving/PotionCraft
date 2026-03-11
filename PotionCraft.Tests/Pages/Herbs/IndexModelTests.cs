using Moq;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Pages.Herbs;
using PotionCraft.Repository.Abstraction;

namespace PotionCraft.Tests.Pages.Herbs
{
    /// <summary>
    /// Тесты для модели страницы списка трав (IndexModel).
    /// </summary>
    public class IndexModelTests
    {
        /// <summary>
        /// Мок-репозиторий трав.
        /// </summary>
        private readonly Mock<IHerbRepository> _mockRepository;

        /// <summary>
        /// Тестируемая модель страницы.
        /// </summary>
        private readonly IndexModel _model;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IndexModelTests"/>.
        /// </summary>
        public IndexModelTests()
        {
            _mockRepository = new Mock<IHerbRepository>();
            _model = new IndexModel(_mockRepository.Object);
        }

        /// <summary>
        /// Создаёт тестовый набор трав.
        /// </summary>
        private static List<Herb> CreateTestHerbs()
        {
            return new List<Herb>
            {
                new Herb
                {
                    Name = "Мандрагора",
                    Description = "Корень в форме человека",
                    HerbType = HerbTypeEnum.Potion,
                    Rarity = RarityEnum.Rare,
                    Effect = "Восстанавливает здоровье",
                    Difficulty = 15,
                    Habitats = new Dictionary<TerrainEnum, int>
                    {
                        { TerrainEnum.Forest, 10 },
                        { TerrainEnum.Hills, 12 }
                    }
                },
                new Herb
                {
                    Name = "Болиголов",
                    Description = "Ядовитое растение",
                    HerbType = HerbTypeEnum.Poison,
                    Rarity = RarityEnum.Common,
                    Effect = "Наносит урон ядом",
                    Difficulty = 10,
                    Habitats = new Dictionary<TerrainEnum, int>
                    {
                        { TerrainEnum.Swamp, 8 }
                    }
                },
                new Herb
                {
                    Name = "Лунный цветок",
                    Description = "Светится в темноте",
                    HerbType = HerbTypeEnum.Magic | HerbTypeEnum.Potion,
                    Rarity = RarityEnum.VeryRare,
                    Effect = "Даёт ночное зрение",
                    Difficulty = 20,
                    Habitats = new Dictionary<TerrainEnum, int>
                    {
                        { TerrainEnum.Underdark, 15 },
                        { TerrainEnum.Forest, 18 }
                    }
                }
            };
        }

        /// <summary>
        /// Проверяет, что OnGetAsync загружает все травы без фильтров.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_NoFilters_ReturnsAllHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Equal(3, _model.Herbs.Count);
        }

        /// <summary>
        /// Проверяет фильтрацию по названию травы.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByName_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterName = "Мандрагора";

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Мандрагора", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по описанию травы.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByDescription_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterDescription = "Ядовитое";

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Болиголов", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по типу травы.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByHerbType_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterHerbType = HerbTypeEnum.Poison;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Болиголов", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по типу Magic — должен вернуть травы, содержащие флаг Magic.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByHerbTypeMagic_ReturnsHerbsWithMagicFlag()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterHerbType = HerbTypeEnum.Magic;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Лунный цветок", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по редкости травы.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByRarity_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterRarity = RarityEnum.Common;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Болиголов", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по эффекту травы.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByEffect_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterEffect = "ночное зрение";

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Лунный цветок", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по сложности.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByDifficulty_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterDifficulty = 15;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Мандрагора", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет фильтрацию по среде обитания.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByHabitat_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterHabitat = TerrainEnum.Swamp;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Single(_model.Herbs);
            Assert.Equal("Болиголов", _model.Herbs[0].Name);
        }

        /// <summary>
        /// Проверяет, что фильтрация по среде обитания Forest возвращает все травы с лесом.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_FilterByForestHabitat_ReturnsMultipleHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterHabitat = TerrainEnum.Forest;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Equal(2, _model.Herbs.Count);
        }

        /// <summary>
        /// Проверяет комбинированную фильтрацию по нескольким полям.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_MultipleFilters_ReturnsMatchingHerbs()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterHerbType = HerbTypeEnum.Potion;
            _model.FilterHabitat = TerrainEnum.Forest;

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Equal(2, _model.Herbs.Count);
        }

        /// <summary>
        /// Проверяет, что если ни одна трава не подходит под фильтр, возвращается пустой список.
        /// </summary>
        [Fact]
        public async Task OnGetAsync_NoMatchingFilters_ReturnsEmptyList()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(herbs);
            _model.FilterName = "Несуществующая трава";

            // Act
            await _model.OnGetAsync();

            // Assert
            Assert.Empty(_model.Herbs);
        }
    }
}
