using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BibleApi.Controllers;
using BibleApi.Models;
using BibleApi.Tests.TestDoubles;
using BibleApi.Core;

namespace BibleApi.Tests;

public class SearchEndpointTests
{
    private readonly BibleController _controller;
    private readonly MockAzureXmlBibleService _mockService;

    public SearchEndpointTests()
    {
        _mockService = new MockAzureXmlBibleService();
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BibleController>();
        _controller = new BibleController(_mockService, logger);
    }

    [Fact]
    public async Task SearchVerses_WithValidQuery_ReturnsSearchResponse()
    {
        // Arrange
        var translationId = "kjv";
        var query = "love";

        // Act
        var result = await _controller.SearchVerses(translationId, query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var searchResponse = Assert.IsType<SearchResponse>(okResult.Value);
        
        Assert.Equal(query, searchResponse.Query);
        Assert.Equal(translationId, searchResponse.Translation.Identifier);
        Assert.NotEmpty(searchResponse.Results);
        Assert.True(searchResponse.TotalResults > 0);
        Assert.Equal(searchResponse.Results.Count, searchResponse.ReturnedResults);
    }

    [Fact]
    public async Task SearchVerses_WithEmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var translationId = "kjv";
        var query = "";

        // Act
        var result = await _controller.SearchVerses(translationId, query);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = badRequestResult.Value;
        Assert.NotNull(error);
    }

    [Fact]
    public async Task SearchVerses_WithInvalidLimit_ReturnsBadRequest()
    {
        // Arrange
        var translationId = "kjv";
        var query = "love";
        var invalidLimit = 1000;

        // Act
        var result = await _controller.SearchVerses(translationId, query, limit: invalidLimit);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = badRequestResult.Value;
        Assert.NotNull(error);
    }

    [Fact]
    public async Task SearchVerses_WithBooksFilter_ReturnsFilteredResults()
    {
        // Arrange
        var translationId = "kjv";
        var query = "love";
        var books = "GEN,PSA";

        // Act
        var result = await _controller.SearchVerses(translationId, query, books);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var searchResponse = Assert.IsType<SearchResponse>(okResult.Value);
        
        Assert.NotNull(searchResponse.BooksSearched);
        Assert.Contains("GEN", searchResponse.BooksSearched);
        Assert.Contains("PSA", searchResponse.BooksSearched);
    }

    [Fact]
    public async Task SearchVerses_WithOTFilter_ReturnsOldTestamentResults()
    {
        // Arrange
        var translationId = "kjv";
        var query = "love";
        var books = "OT";

        // Act
        var result = await _controller.SearchVerses(translationId, query, books);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var searchResponse = Assert.IsType<SearchResponse>(okResult.Value);
        
        Assert.NotNull(searchResponse.BooksSearched);
        Assert.Equal(BibleConstants.OldTestamentBooks, searchResponse.BooksSearched);
    }

    [Fact]
    public async Task SearchVerses_WithNTFilter_ReturnsNewTestamentResults()
    {
        // Arrange
        var translationId = "kjv";
        var query = "love";
        var books = "NT";

        // Act
        var result = await _controller.SearchVerses(translationId, query, books);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var searchResponse = Assert.IsType<SearchResponse>(okResult.Value);
        
        Assert.NotNull(searchResponse.BooksSearched);
        Assert.Equal(BibleConstants.NewTestamentBooks, searchResponse.BooksSearched);
    }

    [Fact]
    public async Task GetDailyVerse_ReturnsConsistentVerseForSameDay()
    {
        // Arrange
        var translationId = "kjv";

        // Act
        var result1 = await _controller.GetDailyVerse(translationId);
        var result2 = await _controller.GetDailyVerse(translationId);

        // Assert
        var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
        var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
        
        var response1 = Assert.IsType<RandomVerseResponse>(okResult1.Value);
        var response2 = Assert.IsType<RandomVerseResponse>(okResult2.Value);
        
        // The daily verse should be the same for multiple calls on the same day
        Assert.Equal(response1.RandomVerse.BookId, response2.RandomVerse.BookId);
        Assert.Equal(response1.RandomVerse.Chapter, response2.RandomVerse.Chapter);
        Assert.Equal(response1.RandomVerse.VerseNumber, response2.RandomVerse.VerseNumber);
    }
}