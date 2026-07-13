using Moq;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Pages.Herbs;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PotionCraft.Tests.Pages.Herbs
{
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public MockHttpMessageHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    public class IndexModelTests
    {
        private static IndexModel CreateModel(List<Herb> herbs)
        {
            var json = JsonSerializer.Serialize(herbs);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var handler = new MockHttpMessageHandler(response);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7088") };
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.CreateClient("PotionCraftApi")).Returns(httpClient);
            return new IndexModel(mockFactory.Object);
        }

        private static List<Herb> CreateTestHerbs()
        {
            return new List<Herb>
            {
                new Herb
                {
                    Name = "Мандрагора",
                    Description = "Корень в форме человека",
                    HerbType = HerbTypeEnum.HealingBase,
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
                    HerbType = HerbTypeEnum.PoisonBase,
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
                    HerbType = HerbTypeEnum.Magic | HerbTypeEnum.HealingBase,
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

        [Fact]
        public async Task OnGetAsync_NoFilters_ReturnsAllHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            await model.OnGetAsync();
            Assert.Equal(3, model.Herbs.Count);
        }

        [Fact]
        public async Task OnGetAsync_FilterByName_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterName = "Мандрагора";
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Мандрагора", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByDescription_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterDescription = "Ядовитое";
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Болиголов", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByHerbType_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterHerbType = HerbTypeEnum.PoisonBase;
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Болиголов", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByHerbTypeMagic_ReturnsHerbsWithMagicFlag()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterHerbType = HerbTypeEnum.Magic;
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Лунный цветок", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByRarity_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterRarity = RarityEnum.Common;
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Болиголов", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByEffect_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterEffect = "ночное зрение";
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Лунный цветок", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByDifficulty_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterDifficulty = 15;
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Мандрагора", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByHabitat_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterHabitat = TerrainEnum.Swamp;
            await model.OnGetAsync();
            Assert.Single(model.Herbs);
            Assert.Equal("Болиголов", model.Herbs[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_FilterByForestHabitat_ReturnsMultipleHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterHabitat = TerrainEnum.Forest;
            await model.OnGetAsync();
            Assert.Equal(2, model.Herbs.Count);
        }

        [Fact]
        public async Task OnGetAsync_MultipleFilters_ReturnsMatchingHerbs()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterHerbType = HerbTypeEnum.HealingBase;
            model.FilterHabitat = TerrainEnum.Forest;
            await model.OnGetAsync();
            Assert.Equal(2, model.Herbs.Count);
        }

        [Fact]
        public async Task OnGetAsync_NoMatchingFilters_ReturnsEmptyList()
        {
            var model = CreateModel(CreateTestHerbs());
            model.FilterName = "Несуществующая трава";
            await model.OnGetAsync();
            Assert.Empty(model.Herbs);
        }

        // ─── Sorting (pure method tests, no HTTP) ────────────────────────────

        private static IndexModel CreateSortModel()
            => CreateModel(new List<Herb>());

        [Fact]
        public void ApplySorting_SortByNameAsc_ReturnsSortedByNameAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Name";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderBy(h => h.Name).Select(h => h.Name).ToList();
            Assert.Equal(expected, result.Select(h => h.Name).ToList());
        }

        [Fact]
        public void ApplySorting_SortByNameDesc_ReturnsSortedByNameDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Name";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderByDescending(h => h.Name).Select(h => h.Name).ToList();
            Assert.Equal(expected, result.Select(h => h.Name).ToList());
        }

        [Fact]
        public void ApplySorting_SortByDescriptionAsc_ReturnsSortedByDescriptionAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Description";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderBy(h => h.Description).Select(h => h.Description).ToList();
            Assert.Equal(expected, result.Select(h => h.Description).ToList());
        }

        [Fact]
        public void ApplySorting_SortByDescriptionDesc_ReturnsSortedByDescriptionDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Description";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderByDescending(h => h.Description).Select(h => h.Description).ToList();
            Assert.Equal(expected, result.Select(h => h.Description).ToList());
        }

        [Fact]
        public void ApplySorting_SortByHerbTypeAsc_ReturnsSortedByHerbTypeAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "HerbType";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderBy(h => (int)h.HerbType).Select(h => (int)h.HerbType).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.HerbType).ToList());
        }

        [Fact]
        public void ApplySorting_SortByHerbTypeDesc_ReturnsSortedByHerbTypeDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "HerbType";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderByDescending(h => (int)h.HerbType).Select(h => (int)h.HerbType).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.HerbType).ToList());
        }

        [Fact]
        public void ApplySorting_SortByRarityAsc_ReturnsSortedByRarityAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Rarity";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderBy(h => (int)h.Rarity).Select(h => (int)h.Rarity).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.Rarity).ToList());
        }

        [Fact]
        public void ApplySorting_SortByRarityDesc_ReturnsSortedByRarityDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Rarity";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderByDescending(h => (int)h.Rarity).Select(h => (int)h.Rarity).ToList();
            Assert.Equal(expected, result.Select(h => (int)h.Rarity).ToList());
        }

        [Fact]
        public void ApplySorting_SortByDifficultyAsc_ReturnsSortedByDifficultyAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Difficulty";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderBy(h => h.Difficulty).Select(h => h.Difficulty).ToList();
            Assert.Equal(expected, result.Select(h => h.Difficulty).ToList());
        }

        [Fact]
        public void ApplySorting_SortByDifficultyDesc_ReturnsSortedByDifficultyDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Difficulty";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderByDescending(h => h.Difficulty).Select(h => h.Difficulty).ToList();
            Assert.Equal(expected, result.Select(h => h.Difficulty).ToList());
        }

        [Fact]
        public void ApplySorting_SortByEffectAsc_ReturnsSortedByEffectAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Effect";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderBy(h => h.Effect).Select(h => h.Effect).ToList();
            Assert.Equal(expected, result.Select(h => h.Effect).ToList());
        }

        [Fact]
        public void ApplySorting_SortByEffectDesc_ReturnsSortedByEffectDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Effect";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            var expected = herbs.OrderByDescending(h => h.Effect).Select(h => h.Effect).ToList();
            Assert.Equal(expected, result.Select(h => h.Effect).ToList());
        }

        [Fact]
        public void ApplySorting_SortByHabitatAsc_ReturnsSortedByMinHabitatKeyAscending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Habitat";
            model.SortDesc = false;
            var result = model.ApplySorting(herbs);
            Assert.Equal("Лунный цветок", result[0].Name);
            Assert.Equal("Мандрагора", result[1].Name);
            Assert.Equal("Болиголов", result[2].Name);
        }

        [Fact]
        public void ApplySorting_SortByHabitatDesc_ReturnsSortedByMinHabitatKeyDescending()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = "Habitat";
            model.SortDesc = true;
            var result = model.ApplySorting(herbs);
            Assert.Equal("Болиголов", result[0].Name);
            Assert.Equal("Мандрагора", result[1].Name);
            Assert.Equal("Лунный цветок", result[2].Name);
        }

        [Fact]
        public void ApplySorting_NoSortBy_ReturnsOriginalOrder()
        {
            var herbs = CreateTestHerbs();
            var model = CreateSortModel();
            model.SortBy = null;
            var result = model.ApplySorting(herbs);
            Assert.Equal(herbs.Select(h => h.Name), result.Select(h => h.Name));
        }
    }
}
