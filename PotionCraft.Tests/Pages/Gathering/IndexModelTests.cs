using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Models;
using PotionCraft.Pages.Gathering;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PotionCraft.Tests.Pages.Gathering
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
        private static IHttpClientFactory CreateHttpClientFactory(HttpResponseMessage response)
        {
            var handler = new MockHttpMessageHandler(response);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7088") };
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.CreateClient("PotionCraftApi")).Returns(httpClient);
            return mockFactory.Object;
        }

        private static IHttpClientFactory CreateUnusedHttpClientFactory()
            => CreateHttpClientFactory(new HttpResponseMessage(HttpStatusCode.OK));

        [Fact]
        public void OnGet_InitializesDefaults()
        {
            // Arrange
            var model = new IndexModel(CreateUnusedHttpClientFactory());

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
            var model = new IndexModel(CreateUnusedHttpClientFactory());
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
            var rollResults = new List<int> { 15, 12, 8 };
            var apiResponse = new
            {
                characterName = "Тест Персонаж",
                rollResults = rollResults,
                totalSuccesses = 2,
                gatheredHerbs = new Dictionary<string, object>()
            };
            var responseBody = JsonSerializer.Serialize(apiResponse);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
            var model = new IndexModel(CreateHttpClientFactory(response));
            model.Input.CharacterId = characterId;
            model.Input.Difficulty = 5;
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

        [Fact]
        public void GetClipboardText_GroupsAndSortsProperly()
        {
            // Arrange
            var model = new IndexModel(CreateUnusedHttpClientFactory());

            var herb1 = new Herb { Id = Guid.NewGuid(), Name = "Ромашка", Rarity = RarityEnum.Common };
            var herb2 = new Herb { Id = Guid.NewGuid(), Name = "Шалфей", Rarity = RarityEnum.Common };
            var herb3 = new Herb { Id = Guid.NewGuid(), Name = "Женьшень", Rarity = RarityEnum.Rare };

            model.GatheredHerbs = new Dictionary<Guid, GatheringResult>
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
            var model = new IndexModel(CreateUnusedHttpClientFactory());
            model.GatheredHerbs = new Dictionary<Guid, GatheringResult>();

            // Act
            var clipboardText = model.GetClipboardText();

            // Assert
            Assert.Equal(string.Empty, clipboardText);
        }
    }
}
