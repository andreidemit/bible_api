using Microsoft.AspNetCore.Mvc;
using BibleApi.Models;
using BibleApi.Services;
using BibleApi.Core;
using Microsoft.AspNetCore.Cors;

namespace BibleApi.Controllers
{
    /// <summary>
    /// Bible API controller (versioned) - equivalent to Python bible router
    /// </summary>
    [ApiController]
    [Route("v1")]
    [EnableCors]
    [Produces("application/json")]
    public class BibleController : ControllerBase
    {
        private readonly IAzureXmlBibleService _azureService;
        private readonly ILogger<BibleController> _logger;

        public BibleController(IAzureXmlBibleService azureService, ILogger<BibleController> logger)
        {
            _azureService = azureService;
            _logger = logger;
        }

        /// <summary>
        /// Helper method to get translation or use first available
        /// </summary>
        private async Task<Translation> GetTranslationAsync(string? identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                var translations = await _azureService.ListTranslationsAsync();
                if (translations.Any())
                {
                    identifier = translations.First().Identifier;
                }
                else
                {
                    throw new InvalidOperationException("No translations available");
                }
            }

            var translation = await _azureService.GetTranslationInfoAsync(identifier.ToLower());
            if (translation == null)
            {
                throw new KeyNotFoundException("Translation not found");
            }

            return translation;
        }

        /// <summary>
        /// List all available Bible translations
        /// GET /v1/data
        /// </summary>
        [HttpGet("data")]
        [ProducesResponseType(typeof(TranslationsResponse), 200)]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // Cache for 1 hour
        public async Task<ActionResult<TranslationsResponse>> ListTranslations()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var translations = await _azureService.ListTranslationsAsync();

                var translationsWithUrls = translations.Select(t => new TranslationWithUrl
                {
                    Identifier = t.Identifier,
                    Name = t.Name,
                    Language = t.Language,
                    LanguageCode = t.LanguageCode,
                    License = t.License,
                    Url = $"{baseUrl}/v1/data/{t.Identifier}"
                }).ToList();

                return Ok(new TranslationsResponse { Translations = translationsWithUrls });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing translations");
                return StatusCode(500, new { error = "Error listing translations" });
            }
        }

        /// <summary>
        /// Get books for a specific translation
        /// GET /v1/data/{translation_id}
        /// </summary>
        [HttpGet("data/{translationId}")]
        [ProducesResponseType(typeof(BooksResponse), 200)]
        [ProducesResponseType(404)]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding")]
        public async Task<ActionResult<BooksResponse>> GetTranslationBooks(string translationId)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var translation = await GetTranslationAsync(translationId);

                // For now, return all Protestant books - in future this could be dynamic based on translation content
                var books = BibleConstants.ProtestantBooks.Select(bookId => new BookWithUrl
                {
                    Id = bookId,
                    Name = BookMetadata.GetName(bookId),
                    Url = $"{baseUrl}/v1/data/{translation.Identifier}/{bookId}"
                }).ToList();

                return Ok(new BooksResponse 
                { 
                    Translation = translation, 
                    Books = books 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting translation books for {TranslationId}", translationId);
                return StatusCode(500, new { error = "Error getting translation books" });
            }
        }

        /// <summary>
        /// Get chapters for a specific book in a translation
        /// GET /v1/data/{translation_id}/{book_id}
        /// </summary>
        [HttpGet("data/{translationId}/{bookId}")]
        [ProducesResponseType(typeof(ChaptersResponse), 200)]
        [ProducesResponseType(404)]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding")]
        public async Task<ActionResult<ChaptersResponse>> GetBookChapters(string translationId, string bookId)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var translation = await GetTranslationAsync(translationId);
                var chapters = await _azureService.GetChaptersForBookAsync(translationId, bookId);

                if (!chapters.Any())
                {
                    return NotFound(new { error = "book not found" });
                }

                // Add URLs to chapters
                foreach (var chapter in chapters)
                {
                    chapter.Url = $"{baseUrl}/v1/data/{translation.Identifier}/{chapter.BookId}/{chapter.Chapter}";
                }

                return Ok(new ChaptersResponse 
                { 
                    Translation = translation, 
                    Chapters = chapters 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chapters for {TranslationId}/{BookId}", translationId, bookId);
                return StatusCode(500, new { error = "Error getting book chapters" });
            }
        }

        /// <summary>
        /// Get verses for a specific chapter
        /// GET /v1/data/{translation_id}/{book_id}/{chapter}
        /// </summary>
        [HttpGet("data/{translationId}/{bookId}/{chapter:int}")]
        [ProducesResponseType(typeof(VersesInChapterResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VersesInChapterResponse>> GetChapterVerses(string translationId, string bookId, int chapter)
        {
            try
            {
                var translation = await GetTranslationAsync(translationId);
                var verses = await _azureService.GetVersesByReferenceAsync(translationId, bookId, chapter);

                if (!verses.Any())
                {
                    return NotFound(new { error = "book/chapter not found" });
                }

                return Ok(new VersesInChapterResponse 
                { 
                    Translation = translation, 
                    Verses = verses 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verses for {TranslationId}/{BookId}/{Chapter}", translationId, bookId, chapter);
                return StatusCode(500, new { error = "Error getting chapter verses" });
            }
        }

        /// <summary>
        /// Get specific verse range within a chapter
        /// GET /v1/data/{translation_id}/{book_id}/{chapter}/{verse_start}-{verse_end}
        /// GET /v1/data/{translation_id}/{book_id}/{chapter}/{verse_start}
        /// </summary>
        [HttpGet("data/{translationId}/{bookId}/{chapter:int}/{verseRange}")]
        [ProducesResponseType(typeof(VerseResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VerseResponse>> GetVerseRange(string translationId, string bookId, int chapter, string verseRange)
        {
            try
            {
                var translation = await GetTranslationAsync(translationId);
                
                // Parse verse range (e.g., "16", "16-18", "1-5")
                int verseStart, verseEnd;
                if (verseRange.Contains('-'))
                {
                    var parts = verseRange.Split('-');
                    if (parts.Length != 2 || !int.TryParse(parts[0], out verseStart) || !int.TryParse(parts[1], out verseEnd))
                    {
                        return BadRequest(new { error = "Invalid verse range format. Use format: {verse} or {start}-{end}" });
                    }
                    
                    if (verseStart > verseEnd || verseStart < 1 || verseEnd > 200) // Reasonable verse limits
                    {
                        return BadRequest(new { error = "Invalid verse range. Start must be <= end and within reasonable limits" });
                    }
                }
                else
                {
                    if (!int.TryParse(verseRange, out verseStart) || verseStart < 1)
                    {
                        return BadRequest(new { error = "Invalid verse number" });
                    }
                    verseEnd = verseStart;
                }

                var verses = await _azureService.GetVersesByReferenceAsync(translationId, bookId, chapter, verseStart, verseEnd);

                if (!verses.Any())
                {
                    return NotFound(new { error = "verses not found" });
                }

                // Create reference string
                var bookName = BookMetadata.GetName(bookId);
                var reference = verseStart == verseEnd 
                    ? $"{bookName} {chapter}:{verseStart}"
                    : $"{bookName} {chapter}:{verseStart}-{verseEnd}";

                var combinedText = string.Join(" ", verses.Select(v => v.Text));

                return Ok(new VerseResponse
                {
                    Reference = reference,
                    Verses = verses,
                    Text = combinedText,
                    TranslationId = translation.Identifier,
                    TranslationName = translation.Name,
                    TranslationNote = translation.License
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verse range for {TranslationId}/{BookId}/{Chapter}/{VerseRange}", translationId, bookId, chapter, verseRange);
                return StatusCode(500, new { error = "Error getting verse range" });
            }
        }

        /// <summary>
        /// Get random verse from specified books
        /// GET /v1/data/{translation_id}/random/{book_id}
        /// </summary>
        [HttpGet("data/{translationId}/random/{bookId}")]
        [ProducesResponseType(typeof(RandomVerseResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RandomVerseResponse>> GetRandomVerseByBook(string translationId, string bookId)
        {
            try
            {
                var translation = await GetTranslationAsync(translationId);
                
                string[] books;
                var upperBookId = bookId.ToUpper();
                
                if (upperBookId == "OT")
                {
                    books = BibleConstants.OldTestamentBooks;
                }
                else if (upperBookId == "NT")
                {
                    books = BibleConstants.NewTestamentBooks;
                }
                else
                {
                    books = upperBookId.Split(',', StringSplitOptions.RemoveEmptyEntries);
                }

                var randomVerse = await _azureService.GetRandomVerseAsync(translation.Identifier, books);
                if (randomVerse == null)
                {
                    return NotFound(new { error = "error getting verse" });
                }

                return Ok(new RandomVerseResponse 
                { 
                    Translation = translation, 
                    RandomVerse = randomVerse 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random verse for {TranslationId}/{BookId}", translationId, bookId);
                return StatusCode(500, new { error = "Error getting random verse" });
            }
        }

        /// <summary>
        /// Search for verses containing specific text
        /// GET /v1/data/{translation_id}/search?q={query}&books={books}&limit={limit}
        /// </summary>
        [HttpGet("data/{translationId}/search")]
        [ProducesResponseType(typeof(SearchResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SearchResponse>> SearchVerses(
            string translationId, 
            [FromQuery] string q, 
            [FromQuery] string? books = null, 
            [FromQuery] int limit = 50)
        {
            try
            {
                // Validate query parameter
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { error = "Query parameter 'q' is required" });
                }

                // Validate limit
                if (limit < 1 || limit > 500)
                {
                    return BadRequest(new { error = "Limit must be between 1 and 500" });
                }

                var translation = await GetTranslationAsync(translationId);
                
                // Parse books parameter
                string[]? bookArray = null;
                if (!string.IsNullOrWhiteSpace(books))
                {
                    var upperBooks = books.ToUpper();
                    if (upperBooks == "OT")
                    {
                        bookArray = BibleConstants.OldTestamentBooks;
                    }
                    else if (upperBooks == "NT")
                    {
                        bookArray = BibleConstants.NewTestamentBooks;
                    }
                    else
                    {
                        bookArray = upperBooks.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Where(b => BibleConstants.IsValidBookId(b))
                            .ToArray();
                        
                        if (bookArray.Length == 0)
                        {
                            return BadRequest(new { error = "No valid book IDs provided" });
                        }
                    }
                }

                var results = await _azureService.SearchVersesAsync(translation.Identifier, q, bookArray, limit);

                return Ok(new SearchResponse
                {
                    Translation = translation,
                    Results = results,
                    Query = q,
                    TotalResults = results.Count, // In a real implementation, this would be the total count before limiting
                    ReturnedResults = results.Count,
                    BooksSearched = bookArray
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching verses for {TranslationId} with query '{Query}'", translationId, q);
                return StatusCode(500, new { error = "Error searching verses" });
            }
        }

        /// <summary>
        /// Get the daily verse (verse of the day)
        /// GET /v1/data/{translation_id}/daily
        /// </summary>
        [HttpGet("data/{translationId}/daily")]
        [ProducesResponseType(typeof(RandomVerseResponse), 200)]
        [ProducesResponseType(404)]
        [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any)] // Cache for 12 hours
        public async Task<ActionResult<RandomVerseResponse>> GetDailyVerse(string translationId)
        {
            try
            {
                var translation = await GetTranslationAsync(translationId);
                
                // Use current date as seed for consistent daily verse
                var today = DateTime.UtcNow.Date;
                var dayOfYear = today.DayOfYear;
                var year = today.Year;
                
                // Create deterministic "random" selection based on date
                var seed = year * 1000 + dayOfYear;
                var random = new Random(seed);
                
                // Select from a curated list of well-known verses for daily reading
                var dailyVerseReferences = new[]
                {
                    ("JHN", 3, 16), ("PSA", 23, 1), ("ROM", 8, 28), ("PHP", 4, 13), ("1CO", 13, 4),
                    ("PSA", 119, 105), ("PRO", 3, 5), ("ISA", 40, 31), ("JER", 29, 11), ("MAT", 28, 20),
                    ("2TI", 3, 16), ("HEB", 11, 1), ("JAM", 1, 17), ("1PE", 5, 7), ("1JN", 4, 19),
                    ("REV", 21, 4), ("PSA", 46, 10), ("ECC", 3, 1), ("GAL", 5, 22), ("EPH", 2, 8)
                };
                
                var selectedReference = dailyVerseReferences[dayOfYear % dailyVerseReferences.Length];
                var verses = await _azureService.GetVersesByReferenceAsync(
                    translation.Identifier, 
                    selectedReference.Item1, 
                    selectedReference.Item2, 
                    selectedReference.Item3, 
                    selectedReference.Item3
                );

                Verse? dailyVerse = null;
                if (verses.Any())
                {
                    dailyVerse = verses.First();
                }
                else
                {
                    // Fallback to a random verse if specific verse not found
                    dailyVerse = await _azureService.GetRandomVerseAsync(translation.Identifier, BibleConstants.ProtestantBooks);
                }

                if (dailyVerse == null)
                {
                    return NotFound(new { error = "error getting daily verse" });
                }

                return Ok(new RandomVerseResponse 
                { 
                    Translation = translation, 
                    RandomVerse = dailyVerse 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "translation not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily verse for {TranslationId}", translationId);
                return StatusCode(500, new { error = "Error getting daily verse" });
            }
        }

        /// <summary>
        /// Helper method to get human-readable book names
        /// </summary>
        // Removed duplicated GetBookName; now using BookMetadata.GetName
    }
}