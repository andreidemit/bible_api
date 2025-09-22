using System.ComponentModel.DataAnnotations;
using BibleApi.Validation;

namespace BibleApi.Models
{
    /// <summary>
    /// Represents a Bible translation (equivalent to Python Translation schema)
    /// </summary>
    public class Translation
    {
        [Required]
        [ValidTranslationId]
        public string Identifier { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Translation name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Language must be between 1 and 50 characters.")]
        public string Language { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Language code must be between 2 and 10 characters.")]
        public string LanguageCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500, ErrorMessage = "License text cannot exceed 500 characters.")]
        public string License { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a single Bible verse (equivalent to Python Verse schema)
    /// </summary>
    public class Verse
    {
        [Required]
        [ValidBookId]
        public string BookId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Book name must be between 1 and 50 characters.")]
        public string Book { get; set; } = string.Empty;
        
        [Required]
        [ValidChapter("BookId")]
        public int Chapter { get; set; }
        
        [Required]
        [ValidVerse]
        public int VerseNumber { get; set; }
        
        [Required]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Verse text must be between 1 and 2000 characters.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for verse queries (equivalent to Python VerseResponse schema)
    /// </summary>
    public class VerseResponse
    {
        [Required]
        [ValidBibleReference]
        public string Reference { get; set; } = string.Empty;
        
        [Required]
        public List<Verse> Verses { get; set; } = new();
        
        [Required]
        [StringLength(10000, ErrorMessage = "Combined text cannot exceed 10000 characters.")]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        [ValidTranslationId]
        public string TranslationId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Translation name must be between 1 and 100 characters.")]
        public string TranslationName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500, ErrorMessage = "Translation note cannot exceed 500 characters.")]
        public string TranslationNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a book chapter reference (equivalent to Python BookChapter schema)
    /// </summary>
    public class BookChapter
    {
        [Required]
        [ValidBookId]
        public string BookId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Book name must be between 1 and 50 characters.")]
        public string Book { get; set; } = string.Empty;
        
        [Required]
        [ValidChapter("BookId")]
        public int Chapter { get; set; }
        
        [Url(ErrorMessage = "URL must be a valid URL format.")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Response model for chapters queries (equivalent to Python ChaptersResponse schema)
    /// </summary>
    public class ChaptersResponse
    {
        [Required]
        public Translation Translation { get; set; } = new();
        
        [Required]
        public List<BookChapter> Chapters { get; set; } = new();
    }

    /// <summary>
    /// Response model for verses in a chapter (equivalent to Python VersesInChapterResponse schema)
    /// </summary>
    public class VersesInChapterResponse
    {
        [Required]
        public Translation Translation { get; set; } = new();
        
        [Required]
        public List<Verse> Verses { get; set; } = new();
    }

    /// <summary>
    /// Response model for random verse queries (equivalent to Python RandomVerseResponse schema)
    /// </summary>
    public class RandomVerseResponse
    {
        [Required]
        public Translation Translation { get; set; } = new();
        
        [Required]
        public Verse RandomVerse { get; set; } = new();
    }

    /// <summary>
    /// Response model for translation listings
    /// </summary>
    public class TranslationsResponse
    {
        [Required]
        public List<TranslationWithUrl> Translations { get; set; } = new();
    }

    /// <summary>
    /// Translation with URL for API navigation
    /// </summary>
    public class TranslationWithUrl : Translation
    {
        public string? Url { get; set; }
    }

    /// <summary>
    /// Response model for book listings
    /// </summary>
    public class BooksResponse
    {
        [Required]
        public Translation Translation { get; set; } = new();
        
        [Required]
        public List<BookWithUrl> Books { get; set; } = new();
    }

    /// <summary>
    /// Book with URL for API navigation
    /// </summary>
    public class BookWithUrl
    {
        [Required]
        [ValidBookId]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Book name must be between 1 and 50 characters.")]
        public string Name { get; set; } = string.Empty;
        
        [Url(ErrorMessage = "URL must be a valid URL format.")]
        public string? Url { get; set; }
    }
}