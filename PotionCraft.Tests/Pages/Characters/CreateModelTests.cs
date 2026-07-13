using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using PotionCraft.Pages.Characters;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PotionCraft.Tests.Pages.Characters
{
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public MockHttpMessageHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    public class CreateModelTests
    {
        private static IHttpClientFactory CreateHttpClientFactory(HttpResponseMessage response)
        {
            var handler = new MockHttpMessageHandler(response);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7088") };
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.CreateClient("PotionCraftApi")).Returns(httpClient);
            return mockFactory.Object;
        }

        [Fact]
        public async Task OnPostAsync_InvalidModelState_ReturnsPageResult()
        {
            // Arrange
            var factory = CreateHttpClientFactory(new HttpResponseMessage(HttpStatusCode.OK));
            var model = new CreateModel(factory);
            model.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_ValidModel_RedirectsToSelectWithAutoSelect()
        {
            // Arrange
            var newCharacterId = Guid.NewGuid();
            var responseBody = JsonSerializer.Serialize(new { Id = newCharacterId });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
            var factory = CreateHttpClientFactory(response);
            var model = new CreateModel(factory);
            model.Name = "Test Character";
            model.Intelligence = 15;
            model.Wisdom = 14;
            model.ProficiencyBonus = 3;
            model.HerbalismProficiencyLevel = 2;
            model.HerbalismToolModifier = 2;
            model.AlchemistProficiencyLevel = 1;
            model.AlchemistToolModifier = -1;
            model.PoisonerProficiencyLevel = 0;
            model.PoisonerToolModifier = 0;

            // Act
            var result = await model.OnPostAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Select", redirectResult.PageName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues!.ContainsKey("autoSelect"));
        }

        [Fact]
        public async Task OnPostAsync_ServerReturnsBadRequest_ReturnsPageResultWithModelError()
        {
            // Arrange
            var errorMessage = "Character already exists";
            var responseBody = JsonSerializer.Serialize(new { Message = errorMessage });
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
            var factory = CreateHttpClientFactory(response);
            var model = new CreateModel(factory);
            model.Name = "Duplicate name";

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.True(model.ModelState.ContainsKey("Name"));
            Assert.Equal(errorMessage, model.ModelState["Name"]!.Errors[0].ErrorMessage);
        }
    }
}
