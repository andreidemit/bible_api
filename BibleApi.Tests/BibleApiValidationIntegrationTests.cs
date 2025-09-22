using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BibleApi.Services;
using BibleApi.Tests.TestDoubles;
using System.Net;
using System.Text.Json;

namespace BibleApi.Tests
{
    public class BibleApiValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BibleApiValidationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the Azure service with our mock for testing
                    services.AddSingleton<IAzureXmlBibleService, MockAzureXmlBibleService>();
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetChapterVerses_InvalidBook_ReturnsValidationError()
        {
            // Act
            var response = await _client.GetAsync("/v1/data/kjv/INVALID_BOOK/1");
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            Assert.Equal("Validation failed", json.RootElement.GetProperty("error").GetString());
            Assert.Equal("bookId", json.RootElement.GetProperty("parameter").GetString());
            Assert.Contains("not a valid Bible book", json.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetChapterVerses_InvalidChapter_ReturnsValidationError()
        {
            // Act - Try to get chapter 999 from Obadiah (which only has 1 chapter)
            var response = await _client.GetAsync("/v1/data/kjv/OBA/999");
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            Assert.Equal("Validation failed", json.RootElement.GetProperty("error").GetString());
            Assert.Equal("chapter", json.RootElement.GetProperty("parameter").GetString());
            Assert.Contains("Maximum chapter is 1", json.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetBookChapters_ValidRequest_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/v1/data/kjv/GEN");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            // Should contain translation and chapters
            Assert.True(json.RootElement.TryGetProperty("translation", out _));
            Assert.True(json.RootElement.TryGetProperty("chapters", out _));
        }

        [Fact]
        public async Task GetRandomVerse_InvalidBooks_ReturnsValidationError()
        {
            // Act - Try to get random verse from invalid books
            var response = await _client.GetAsync("/v1/data/kjv/random/INVALID1,INVALID2");
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            Assert.Equal("Validation failed", json.RootElement.GetProperty("error").GetString());
            Assert.Contains("books", json.RootElement.GetProperty("parameter").GetString());
        }

        [Fact]
        public async Task HealthCheck_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/healthz");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            Assert.Equal("healthy", json.RootElement.GetProperty("status").GetString());
        }
    }
}