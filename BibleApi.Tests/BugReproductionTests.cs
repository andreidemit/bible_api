using BibleApi.Core;
using BibleApi.Tests.TestDoubles;
using Xunit;

namespace BibleApi.Tests
{
    public class BugReproductionTests
    {
        [Fact]
        public void BookMetadata_Normalize_NullInput_ShouldNotThrow()
        {
            // This should not throw NullReferenceException
            var result = BookMetadata.Normalize(null);
            Assert.NotNull(result);
        }

        [Fact]
        public void BookMetadata_Normalize_EmptyInput_ShouldNotThrow()
        {
            var result = BookMetadata.Normalize("");
            Assert.Equal("", result);
        }

        [Fact]
        public void BookMetadata_Normalize_WhitespaceInput_ShouldNotThrow()
        {
            var result = BookMetadata.Normalize("   ");
            Assert.Equal("", result);
        }

        [Fact]
        public async Task MockService_GetTranslationInfoAsync_ShouldReturnNullableCorrectly()
        {
            var service = new MockAzureXmlBibleService();
            var result = await service.GetTranslationInfoAsync("nonexistent");
            Assert.NotNull(result); // This test validates the current behavior
        }

        [Fact]
        public async Task MockService_GetVersesByReferenceAsync_NegativeChapter_ShouldReturnEmpty()
        {
            // Arrange
            var service = new MockAzureXmlBibleService();

            // Act
            var result = await service.GetVersesByReferenceAsync("kjv", "GEN", -1);

            // Assert - Should return empty for invalid input
            Assert.Empty(result);
        }

        [Fact]
        public async Task MockService_GetVersesByReferenceAsync_ZeroChapter_ShouldReturnEmpty()
        {
            // Arrange  
            var service = new MockAzureXmlBibleService();

            // Act
            var result = await service.GetVersesByReferenceAsync("kjv", "GEN", 0);

            // Assert - Should return empty for invalid input
            Assert.Empty(result);
        }
    }
}