using BibleApi.Core.Models;

namespace BibleApi.Core.Interfaces
{
    /// <summary>
    /// Interface for Bible data services that read from various sources (XML, database, etc.)
    /// </summary>
    public interface IBibleService
    {
        /// <summary>
        /// List all available Bible translations
        /// </summary>
        Task<List<Translation>> ListTranslationsAsync();

        /// <summary>
        /// Get translation metadata by identifier
        /// </summary>
        Task<Translation?> GetTranslationInfoAsync(string identifier);

        /// <summary>
        /// Get verses by book, chapter, and optional verse range
        /// </summary>
        Task<List<Verse>> GetVersesByReferenceAsync(string translationId, string book, int chapter, int? verseStart = null, int? verseEnd = null);

        /// <summary>
        /// Get all chapters for a specific book
        /// </summary>
        Task<List<BookChapter>> GetChaptersForBookAsync(string translationId, string bookId);

        /// <summary>
        /// Get a random verse from specified books
        /// </summary>
        Task<Verse?> GetRandomVerseAsync(string translationId, string[] books);
    }

    /// <summary>
    /// Interface for XML-based Bible services
    /// </summary>
    public interface IXmlBibleService : IBibleService
    {
        // Specific methods for XML-based services can be added here
    }

    /// <summary>
    /// Interface for database-based Bible services
    /// </summary>
    public interface IDatabaseBibleService : IBibleService
    {
        // Specific methods for database-based services can be added here
    }
}