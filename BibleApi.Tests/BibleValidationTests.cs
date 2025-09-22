using Xunit;
using BibleApi.Validation;
using System.ComponentModel.DataAnnotations;

namespace BibleApi.Tests
{
    public class BibleValidationTests
    {
        #region ValidBookIdAttribute Tests

        [Theory]
        [InlineData("GEN", true)]
        [InlineData("REV", true)]
        [InlineData("gen", true)] // case insensitive
        [InlineData("Genesis", true)] // full name
        [InlineData("1 Samuel", true)] // with space
        [InlineData("XYZ", false)]
        [InlineData("", false)]
        [InlineData(null, true)] // null is allowed for optional fields
        public void ValidBookIdAttribute_ValidatesCorrectly(string bookId, bool expected)
        {
            var attribute = new ValidBookIdAttribute();
            Assert.Equal(expected, attribute.IsValid(bookId));
        }

        #endregion

        #region ValidChapterAttribute Tests

        [Fact]
        public void ValidChapterAttribute_ValidatesPositiveNumbers()
        {
            var attribute = new ValidChapterAttribute("BookId");
            var context = new ValidationContext(new { BookId = "GEN" });

            var result = attribute.GetValidationResult(1, context);
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void ValidChapterAttribute_RejectsZeroAndNegative()
        {
            var attribute = new ValidChapterAttribute("BookId");
            var context = new ValidationContext(new { BookId = "GEN" });

            var result = attribute.GetValidationResult(0, context);
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Contains("greater than 0", result?.ErrorMessage);
        }

        [Fact]
        public void ValidChapterAttribute_ValidatesChapterRange()
        {
            var attribute = new ValidChapterAttribute("BookId");
            var testObject = new { BookId = "OBA" }; // Obadiah has only 1 chapter
            var context = new ValidationContext(testObject);

            // Valid chapter
            var result1 = attribute.GetValidationResult(1, context);
            Assert.Equal(ValidationResult.Success, result1);

            // Invalid chapter
            var result2 = attribute.GetValidationResult(2, context);
            Assert.NotEqual(ValidationResult.Success, result2);
            Assert.Contains("Maximum chapter is 1", result2?.ErrorMessage);
        }

        #endregion

        #region ValidVerseAttribute Tests

        [Theory]
        [InlineData(1, true)]
        [InlineData(50, true)]
        [InlineData(176, true)] // Psalm 119 is the longest
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(177, false)]
        [InlineData(null, true)] // null is allowed for optional parameters
        public void ValidVerseAttribute_ValidatesRange(int? verse, bool expected)
        {
            var attribute = new ValidVerseAttribute();
            Assert.Equal(expected, attribute.IsValid(verse));
        }

        #endregion

        #region ValidTranslationIdAttribute Tests

        [Theory]
        [InlineData("kjv", true)]
        [InlineData("asv", true)]
        [InlineData("niv-2011", true)]
        [InlineData("esv_study", true)]
        [InlineData("x", false)] // too short
        [InlineData("", false)]
        [InlineData("a-very-long-translation-id-that-exceeds-limit", false)] // too long
        [InlineData("kjv@2011", false)] // invalid characters
        [InlineData("kjv 2011", false)] // spaces not allowed
        [InlineData(null, false)] // null not allowed for translation IDs
        public void ValidTranslationIdAttribute_ValidatesFormat(string translationId, bool expected)
        {
            var attribute = new ValidTranslationIdAttribute();
            Assert.Equal(expected, attribute.IsValid(translationId));
        }

        #endregion

        #region ValidBibleReferenceAttribute Tests

        [Theory]
        [InlineData("John 3:16", true)]
        [InlineData("Genesis 1:1-31", true)]
        [InlineData("Psalm 23", true)]
        [InlineData("", false)]
        [InlineData(" John 3:16", false)] // leading space
        [InlineData("John 3:16 ", false)] // trailing space
        [InlineData("John..3:16", false)] // double dots
        [InlineData(null, true)] // null allowed for optional fields
        public void ValidBibleReferenceAttribute_ValidatesFormat(string reference, bool expected)
        {
            var attribute = new ValidBibleReferenceAttribute();
            Assert.Equal(expected, attribute.IsValid(reference));
        }

        #endregion
    }

    public class BibleValidationHelperTests
    {
        #region ValidateTranslationId Tests

        [Fact]
        public void ValidateTranslationId_ValidId_DoesNotThrow()
        {
            var exception = Record.Exception(() => BibleValidationHelper.ValidateTranslationId("kjv"));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateTranslationId_NullOrEmpty_ThrowsException(string translationId)
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateTranslationId(translationId));
            Assert.Equal("translationId", exception.Parameter);
            Assert.Contains("cannot be null or empty", exception.Message);
        }

        [Theory]
        [InlineData("x")] // too short
        [InlineData("this-is-a-very-long-translation-identifier-that-exceeds-limit")] // too long
        public void ValidateTranslationId_InvalidLength_ThrowsException(string translationId)
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateTranslationId(translationId));
            Assert.Equal("translationId", exception.Parameter);
        }

        [Theory]
        [InlineData("kjv@2011")]
        [InlineData("kjv 2011")]
        [InlineData("kjv!")]
        public void ValidateTranslationId_InvalidCharacters_ThrowsException(string translationId)
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateTranslationId(translationId));
            Assert.Equal("translationId", exception.Parameter);
            Assert.Contains("letters, numbers, hyphens, and underscores", exception.Message);
        }

        #endregion

        #region ValidateAndNormalizeBookId Tests

        [Theory]
        [InlineData("gen", "GEN")]
        [InlineData("Genesis", "GEN")]
        [InlineData("1 Samuel", "1SA")]
        public void ValidateAndNormalizeBookId_ValidBook_ReturnsNormalized(string bookId, string expected)
        {
            var result = BibleValidationHelper.ValidateAndNormalizeBookId(bookId);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateAndNormalizeBookId_NullOrEmpty_ThrowsException(string bookId)
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateAndNormalizeBookId(bookId));
            Assert.Equal("bookId", exception.Parameter);
        }

        [Fact]
        public void ValidateAndNormalizeBookId_InvalidBook_ThrowsException()
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateAndNormalizeBookId("XYZ"));
            Assert.Equal("bookId", exception.Parameter);
            Assert.Contains("not a valid Bible book", exception.Message);
        }

        #endregion

        #region ValidateChapter Tests

        [Fact]
        public void ValidateChapter_ValidChapter_DoesNotThrow()
        {
            var exception = Record.Exception(() => 
                BibleValidationHelper.ValidateChapter("GEN", 1));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateChapter_ZeroOrNegative_ThrowsException()
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateChapter("GEN", 0));
            Assert.Equal("chapter", exception.Parameter);
            Assert.Contains("greater than 0", exception.Message);
        }

        [Fact]
        public void ValidateChapter_ExceedsBookLimit_ThrowsException()
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateChapter("OBA", 2)); // Obadiah has only 1 chapter
            Assert.Equal("chapter", exception.Parameter);
            Assert.Contains("Maximum chapter is 1", exception.Message);
        }

        #endregion

        #region ValidateVerse Tests

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(176)]
        public void ValidateVerse_ValidVerse_DoesNotThrow(int verse)
        {
            var exception = Record.Exception(() => BibleValidationHelper.ValidateVerse(verse));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateVerse_ZeroOrNegative_ThrowsException(int verse)
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateVerse(verse));
            Assert.Equal("verse", exception.Parameter);
            Assert.Contains("greater than 0", exception.Message);
        }

        [Fact]
        public void ValidateVerse_ExceedsMaximum_ThrowsException()
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateVerse(200));
            Assert.Equal("verse", exception.Parameter);
            Assert.Contains("exceeds maximum", exception.Message);
        }

        #endregion

        #region ValidateVerseRange Tests

        [Fact]
        public void ValidateVerseRange_ValidRange_DoesNotThrow()
        {
            var exception = Record.Exception(() => 
                BibleValidationHelper.ValidateVerseRange(1, 10));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateVerseRange_StartGreaterThanEnd_ThrowsException()
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateVerseRange(10, 5));
            Assert.Equal("verseEnd", exception.Parameter);
            Assert.Contains("cannot be less than start verse", exception.Message);
        }

        #endregion

        #region ValidateAndNormalizeBookIds Tests

        [Fact]
        public void ValidateAndNormalizeBookIds_ValidBooks_ReturnsNormalized()
        {
            var books = new[] { "gen", "exo", "lev" };
            var result = BibleValidationHelper.ValidateAndNormalizeBookIds(books);
            
            Assert.Equal(3, result.Length);
            Assert.Equal("GEN", result[0]);
            Assert.Equal("EXO", result[1]);
            Assert.Equal("LEV", result[2]);
        }

        [Theory]
        [InlineData(null)]
        public void ValidateAndNormalizeBookIds_NullOrEmpty_ThrowsException(string[] books)
        {
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateAndNormalizeBookIds(books));
            Assert.Equal("books", exception.Parameter);
            Assert.Contains("At least one book", exception.Message);
        }

        [Fact]
        public void ValidateAndNormalizeBookIds_EmptyArray_ThrowsException()
        {
            var books = Array.Empty<string>();
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateAndNormalizeBookIds(books));
            Assert.Equal("books", exception.Parameter);
            Assert.Contains("At least one book", exception.Message);
        }

        [Fact]
        public void ValidateAndNormalizeBookIds_InvalidBook_ThrowsExceptionWithIndex()
        {
            var books = new[] { "gen", "xyz", "lev" };
            var exception = Assert.Throws<BibleValidationException>(() => 
                BibleValidationHelper.ValidateAndNormalizeBookIds(books));
            Assert.Equal("books[1]", exception.Parameter);
        }

        #endregion
    }
}