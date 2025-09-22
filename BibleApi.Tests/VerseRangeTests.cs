using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BibleApi.Controllers;
using BibleApi.Models;
using BibleApi.Tests.TestDoubles;

namespace BibleApi.Tests;

public class VerseRangeTests
{
    private readonly BibleController _controller;
    private readonly MockAzureXmlBibleService _mockService;

    public VerseRangeTests()
    {
        _mockService = new MockAzureXmlBibleService();
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BibleController>();
        _controller = new BibleController(_mockService, logger);
    }

    [Fact]
    public async Task GetVerseRange_WithSingleVerse_ReturnsVerseResponse()
    {
        // Arrange
        var translationId = "kjv";
        var bookId = "JHN";
        var chapter = 3;
        var verseRange = "16";

        // Act
        var result = await _controller.GetVerseRange(translationId, bookId, chapter, verseRange);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var verseResponse = Assert.IsType<VerseResponse>(okResult.Value);
        
        Assert.Contains("John 3:16", verseResponse.Reference);
        Assert.Single(verseResponse.Verses);
        Assert.Equal(16, verseResponse.Verses[0].VerseNumber);
        Assert.Equal(3, verseResponse.Verses[0].Chapter);
        Assert.Equal("JHN", verseResponse.Verses[0].BookId);
    }

    [Fact]
    public async Task GetVerseRange_WithVerseRange_ReturnsMultipleVerses()
    {
        // Arrange
        var translationId = "kjv";
        var bookId = "JHN";
        var chapter = 3;
        var verseRange = "16-18";

        // Act
        var result = await _controller.GetVerseRange(translationId, bookId, chapter, verseRange);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var verseResponse = Assert.IsType<VerseResponse>(okResult.Value);
        
        Assert.Contains("John 3:16-18", verseResponse.Reference);
        Assert.Equal(3, verseResponse.Verses.Count); // Verses 16, 17, 18
        Assert.Equal(16, verseResponse.Verses[0].VerseNumber);
        Assert.Equal(18, verseResponse.Verses[2].VerseNumber);
    }

    [Fact]
    public async Task GetVerseRange_WithInvalidRange_ReturnsBadRequest()
    {
        // Arrange
        var translationId = "kjv";
        var bookId = "JHN";
        var chapter = 3;
        var verseRange = "18-16"; // End before start

        // Act
        var result = await _controller.GetVerseRange(translationId, bookId, chapter, verseRange);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetVerseRange_WithInvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var translationId = "kjv";
        var bookId = "JHN";
        var chapter = 3;
        var verseRange = "abc";

        // Act
        var result = await _controller.GetVerseRange(translationId, bookId, chapter, verseRange);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Theory]
    [InlineData("16")]
    [InlineData("1-5")]
    [InlineData("10-15")]
    public async Task GetVerseRange_WithValidFormats_ReturnsOk(string verseRange)
    {
        // Arrange
        var translationId = "kjv";
        var bookId = "JHN";
        var chapter = 3;

        // Act
        var result = await _controller.GetVerseRange(translationId, bookId, chapter, verseRange);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var verseResponse = Assert.IsType<VerseResponse>(okResult.Value);
        
        Assert.NotNull(verseResponse.Reference);
        Assert.NotEmpty(verseResponse.Verses);
        Assert.NotEmpty(verseResponse.Text);
        Assert.Equal(translationId, verseResponse.TranslationId);
    }
}