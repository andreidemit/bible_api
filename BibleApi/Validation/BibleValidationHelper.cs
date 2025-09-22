using System.ComponentModel.DataAnnotations;

namespace BibleApi.Validation
{
    /// <summary>
    /// Custom exceptions for Bible data validation
    /// </summary>
    public class BibleValidationException : ValidationException
    {
        public string Parameter { get; }

        public BibleValidationException(string parameter, string message) : base(message)
        {
            Parameter = parameter;
        }

        public BibleValidationException(string parameter, string message, Exception innerException) 
            : base(message, innerException)
        {
            Parameter = parameter;
        }
    }

    /// <summary>
    /// Validation helpers for Bible API services
    /// </summary>
    public static class BibleValidationHelper
    {
        /// <summary>
        /// Validates a translation ID
        /// </summary>
        public static void ValidateTranslationId(string? translationId, string parameterName = "translationId")
        {
            if (string.IsNullOrWhiteSpace(translationId))
            {
                throw new BibleValidationException(parameterName, "Translation ID cannot be null or empty.");
            }

            if (translationId.Length < 2 || translationId.Length > 20)
            {
                throw new BibleValidationException(parameterName, "Translation ID must be between 2 and 20 characters.");
            }

            if (!translationId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
            {
                throw new BibleValidationException(parameterName, "Translation ID can only contain letters, numbers, hyphens, and underscores.");
            }
        }

        /// <summary>
        /// Validates and normalizes a book ID
        /// </summary>
        public static string ValidateAndNormalizeBookId(string? bookId, string parameterName = "bookId")
        {
            if (string.IsNullOrWhiteSpace(bookId))
            {
                throw new BibleValidationException(parameterName, "Book ID cannot be null or empty.");
            }

            var normalized = BibleApi.Core.BookMetadata.Normalize(bookId);
            
            if (!BibleApi.Core.BibleConstants.IsValidBookId(normalized))
            {
                throw new BibleValidationException(parameterName, $"'{bookId}' is not a valid Bible book identifier.");
            }

            return normalized;
        }

        /// <summary>
        /// Validates a chapter number against the book's chapter count
        /// </summary>
        public static void ValidateChapter(string normalizedBookId, int chapter, string parameterName = "chapter")
        {
            if (chapter < 1)
            {
                throw new BibleValidationException(parameterName, "Chapter number must be greater than 0.");
            }

            var maxChapter = BibleApi.Core.BookMetadata.GetChapterCount(normalizedBookId);
            if (chapter > maxChapter)
            {
                var bookName = BibleApi.Core.BookMetadata.GetName(normalizedBookId);
                throw new BibleValidationException(parameterName, 
                    $"Chapter {chapter} is not valid for {bookName}. Maximum chapter is {maxChapter}.");
            }
        }

        /// <summary>
        /// Validates a verse number range
        /// </summary>
        public static void ValidateVerse(int verse, string parameterName = "verse")
        {
            if (verse < 1)
            {
                throw new BibleValidationException(parameterName, "Verse number must be greater than 0.");
            }

            if (verse > 176) // Psalm 119 is the longest chapter
            {
                throw new BibleValidationException(parameterName, "Verse number exceeds maximum expected verse count (176).");
            }
        }

        /// <summary>
        /// Validates a verse range
        /// </summary>
        public static void ValidateVerseRange(int? verseStart, int? verseEnd, string startParamName = "verseStart", string endParamName = "verseEnd")
        {
            if (verseStart.HasValue)
            {
                ValidateVerse(verseStart.Value, startParamName);
            }

            if (verseEnd.HasValue)
            {
                ValidateVerse(verseEnd.Value, endParamName);
            }

            if (verseStart.HasValue && verseEnd.HasValue && verseStart.Value > verseEnd.Value)
            {
                throw new BibleValidationException(endParamName, "End verse cannot be less than start verse.");
            }
        }

        /// <summary>
        /// Validates an array of book IDs
        /// </summary>
        public static string[] ValidateAndNormalizeBookIds(string[]? books, string parameterName = "books")
        {
            if (books == null || books.Length == 0)
            {
                throw new BibleValidationException(parameterName, "At least one book must be specified.");
            }

            var normalizedBooks = new string[books.Length];
            for (int i = 0; i < books.Length; i++)
            {
                normalizedBooks[i] = ValidateAndNormalizeBookId(books[i], $"{parameterName}[{i}]");
            }

            return normalizedBooks;
        }
    }
}