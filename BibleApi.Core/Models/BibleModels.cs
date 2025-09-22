using System.ComponentModel.DataAnnotations;

namespace BibleApi.Core.Models
{
    /// <summary>
    /// Represents a Bible translation
    /// </summary>
    public class Translation
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Language { get; set; } = string.Empty;
        
        [Required]
        public string LanguageCode { get; set; } = string.Empty;
        
        [Required]
        public string License { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a single Bible verse
    /// </summary>
    public class Verse
    {
        [Required]
        public string BookId { get; set; } = string.Empty;
        
        [Required]
        public string Book { get; set; } = string.Empty;
        
        [Required]
        public int Chapter { get; set; }
        
        [Required]
        public int VerseNumber { get; set; }
        
        [Required]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a book chapter reference
    /// </summary>
    public class BookChapter
    {
        [Required]
        public string BookId { get; set; } = string.Empty;
        
        [Required]
        public string Book { get; set; } = string.Empty;
        
        [Required]
        public int Chapter { get; set; }
        
        public string? Url { get; set; }
    }

    /// <summary>
    /// Response model for verse queries
    /// </summary>
    public class VerseResponse
    {
        [Required]
        public string Reference { get; set; } = string.Empty;
        
        [Required]
        public List<Verse> Verses { get; set; } = new();
        
        [Required]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        public string TranslationId { get; set; } = string.Empty;
        
        [Required]
        public string TranslationName { get; set; } = string.Empty;
        
        [Required]
        public string TranslationNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for chapters queries
    /// </summary>
    public class ChaptersResponse
    {
        [Required]
        public Translation Translation { get; set; } = new();
        
        [Required]
        public List<BookChapter> Chapters { get; set; } = new();
    }
}