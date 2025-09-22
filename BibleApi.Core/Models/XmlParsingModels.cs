namespace BibleApi.Core.Models
{
    /// <summary>
    /// Container for parsed Bible data
    /// </summary>
    public class ParsedBibleData
    {
        public TranslationInfo Translation { get; set; } = new();
        public List<BookData> Books { get; set; } = new();
    }

    /// <summary>
    /// Translation information from XML parsing
    /// </summary>
    public class TranslationInfo
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
    }

    /// <summary>
    /// Book data with verses
    /// </summary>
    public class BookData
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Testament { get; set; }
        public List<VerseData> Verses { get; set; } = new();
    }

    /// <summary>
    /// Individual verse data
    /// </summary>
    public class VerseData
    {
        public short Chapter { get; set; }
        public short Verse { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}