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

        /// <summary>
        /// Проверяет сортировку по названию в прямом порядке (по алфавиту).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByNameAsc_ReturnsSortedByNameAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Name";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderBy(h => h.Name).Select(h => h.Name).ToList();
            Assert.Equal(expected, result.Select(h => h.Name).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по названию в обратном порядке (по алфавиту убыванием).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByNameDesc_ReturnsSortedByNameDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Name";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderByDescending(h => h.Name).Select(h => h.Name).ToList();
            Assert.Equal(expected, result.Select(h => h.Name).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по описанию в прямом порядке (по алфавиту).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByDescriptionAsc_ReturnsSortedByDescriptionAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Description";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderBy(h => h.Description).Select(h => h.Description).ToList();
            Assert.Equal(expected, result.Select(h => h.Description).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по описанию в обратном порядке.
        /// </summary>
        [Fact]
        public void ApplySorting_SortByDescriptionDesc_ReturnsSortedByDescriptionDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Description";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderByDescending(h => h.Description).Select(h => h.Description).ToList();
            Assert.Equal(expected, result.Select(h => h.Description).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по типу травы в прямом порядке (по числовому значению enum).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByHerbTypeAsc_ReturnsSortedByHerbTypeAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "HerbType";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderBy(h => (int)h.HerbType).Select(h => (int)h.HerbType).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.HerbType).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по типу травы в обратном порядке (по числовому значению enum убыванием).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByHerbTypeDesc_ReturnsSortedByHerbTypeDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "HerbType";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderByDescending(h => (int)h.HerbType).Select(h => (int)h.HerbType).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.HerbType).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по редкости в прямом порядке (по числовому значению enum).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByRarityAsc_ReturnsSortedByRarityAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Rarity";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderBy(h => (int)h.Rarity).Select(h => (int)h.Rarity).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.Rarity).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по редкости в обратном порядке (по числовому значению enum убыванием).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByRarityDesc_ReturnsSortedByRarityDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Rarity";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderByDescending(h => (int)h.Rarity).Select(h => (int)h.Rarity).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.Rarity).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по сложности в прямом порядке.
        /// </summary>
        [Fact]
        public void ApplySorting_SortByDifficultyAsc_ReturnsSortedByDifficultyAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Difficulty";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderBy(h => h.Difficulty).Select(h => h.Difficulty).ToList();
            Assert.Equal(expected, result.Select(h => h.Difficulty).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по сложности в обратном порядке.
        /// </summary>
        [Fact]
        public void ApplySorting_SortByDifficultyDesc_ReturnsSortedByDifficultyDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Difficulty";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderByDescending(h => h.Difficulty).Select(h => h.Difficulty).ToList();
            Assert.Equal(expected, result.Select(h => h.Difficulty).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по эффекту в прямом порядке (по алфавиту).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByEffectAsc_ReturnsSortedByEffectAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Effect";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderBy(h => h.Effect).Select(h => h.Effect).ToList();
            Assert.Equal(expected, result.Select(h => h.Effect).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по эффекту в обратном порядке.
        /// </summary>
        [Fact]
        public void ApplySorting_SortByEffectDesc_ReturnsSortedByEffectDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Effect";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            var expected = herbs.OrderByDescending(h => h.Effect).Select(h => h.Effect).ToList();
            Assert.Equal(expected, result.Select(h => h.Effect).ToList());
        }

        /// <summary>
        /// Проверяет сортировку по среде обитания в прямом порядке
        /// (по минимальному числовому значению первой среды обитания).
        /// </summary>
        [Fact]
        public void ApplySorting_SortByHabitatAsc_ReturnsSortedByMinHabitatKeyAscending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Habitat";
            _model.SortDesc = false;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            // Болиголов: Swamp=4, Мандрагора: Forest=3 → min=3, Лунный цветок: Underdark=2 → min=2
            // Ожидаемый порядок по возрастанию: Лунный цветок (2), Мандрагора (3), Болиголов (4)
            Assert.Equal("Лунный цветок", result[0].Name);
            Assert.Equal("Мандрагора", result[1].Name);
            Assert.Equal("Болиголов", result[2].Name);
        }

        /// <summary>
        /// Проверяет сортировку по среде обитания в обратном порядке.
        /// </summary>
        [Fact]
        public void ApplySorting_SortByHabitatDesc_ReturnsSortedByMinHabitatKeyDescending()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = "Habitat";
            _model.SortDesc = true;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            // Ожидаемый порядок по убыванию: Болиголов (4), Мандрагора (3), Лунный цветок (2)
            Assert.Equal("Болиголов", result[0].Name);
            Assert.Equal("Мандрагора", result[1].Name);
            Assert.Equal("Лунный цветок", result[2].Name);
        }

        /// <summary>
        /// Проверяет, что при отсутствии SortBy список возвращается без изменений.
        /// </summary>
        [Fact]
        public void ApplySorting_NoSortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var herbs = CreateTestHerbs();
            _model.SortBy = null;

            // Act
            var result = _model.ApplySorting(herbs);

            // Assert
            Assert.Equal(herbs.Select(h => h.Name), result.Select(h => h.Name));
        }
    }
}
