using System.Xml.Linq;
using System.Text.RegularExpressions;
using BibleApi.Core.Models;
using BibleApi.Core.Utilities;

namespace BibleApi.Core.Utilities
{
    /// <summary>
    /// Utilities for parsing Bible XML content and extracting translation metadata
    /// </summary>
    public static class XmlParsingUtilities
    {
        /// <summary>
        /// Extract translation metadata from XML content
        /// </summary>
        /// <param name="xmlContent">XML content to parse</param>
        /// <param name="identifier">Translation identifier</param>
        /// <returns>Translation object with extracted metadata</returns>
        public static Translation ParseXmlForTranslationInfo(string xmlContent, string identifier)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;

                // Default values
                string name = identifier.ToUpper();
                string license = "Public Domain";
                string language = "english";
                string languageCode = "en";

                if (root != null)
                {
                    // Check for OSIS format
                    var osisNamespace = XNamespace.Get("http://www.bibletechnologies.net/2003/OSIS/namespace");
                    if (root.Name.LocalName == "osis")
                    {
                        var workElement = root.Descendants(osisNamespace + "work").FirstOrDefault();
                        if (workElement != null)
                        {
                            var titleElement = workElement.Element(osisNamespace + "title");
                            if (titleElement != null)
                                name = titleElement.Value;

                            var rightsElement = workElement.Element(osisNamespace + "rights");
                            if (rightsElement != null)
                                license = rightsElement.Value;
                        }
                    }
                    // Check for other XML formats
                    else if (root.Attribute("title") != null)
                    {
                        name = root.Attribute("title")?.Value ?? name;
                    }
                    else if (root.Attribute("name") != null)
                    {
                        name = root.Attribute("name")?.Value ?? name;
                    }
                }

                // Determine language from identifier
                if (identifier.ToLower().Contains("romanian") || identifier.ToLower().Contains("ro-"))
                {
                    language = "romanian";
                    languageCode = "ro";
                }
                else if (identifier.ToLower().Contains("spanish") || identifier.ToLower().Contains("es-"))
                {
                    language = "spanish";
                    languageCode = "es";
                }
                else if (identifier.ToLower().Contains("french") || identifier.ToLower().Contains("fr-"))
                {
                    language = "french";
                    languageCode = "fr";
                }

                return new Translation
                {
                    Identifier = identifier,
                    Name = name,
                    License = license,
                    Language = language,
                    LanguageCode = languageCode
                };
            }
            catch (Exception)
            {
                // Return default translation info if parsing fails
                return new Translation
                {
                    Identifier = identifier,
                    Name = identifier.ToUpper(),
                    License = "Public Domain",
                    Language = "english",
                    LanguageCode = "en"
                };
            }
        }

        /// <summary>
        /// Parse USFX XML content and extract verses for a specific book and chapter
        /// </summary>
        /// <param name="xmlContent">USFX XML content</param>
        /// <param name="bookCode">Book code to search for</param>
        /// <param name="chapter">Chapter number</param>
        /// <param name="verseStart">Optional start verse number</param>
        /// <param name="verseEnd">Optional end verse number</param>
        /// <returns>List of verses</returns>
        public static List<Verse> ParseVersesFromXml(string xmlContent, string bookCode, int chapter, int? verseStart = null, int? verseEnd = null)
        {
            var verses = new List<Verse>();
            
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;

                if (root == null) return verses;

                // Find the book element
                var bookElement = root.Descendants("book")
                    .FirstOrDefault(b => string.Equals(b.Attribute("code")?.Value, bookCode, StringComparison.OrdinalIgnoreCase));

                if (bookElement == null) return verses;

                // Find the chapter element
                var chapterElement = bookElement.Descendants("c")
                    .FirstOrDefault(c => c.Attribute("id")?.Value == chapter.ToString());

                if (chapterElement == null) return verses;

                // Extract verses
                var verseElements = chapterElement.Descendants("v").Where(v => v.Attribute("id") != null);

                foreach (var verseElement in verseElements)
                {
                    if (int.TryParse(verseElement.Attribute("id")?.Value, out int verseNumber))
                    {
                        // Apply verse range filter if specified
                        if (verseStart.HasValue && verseNumber < verseStart.Value) continue;
                        if (verseEnd.HasValue && verseNumber > verseEnd.Value) continue;

                        var verseText = ExtractVerseText(verseElement);
                        var bookName = BookMetadata.GetName(bookCode);

                        verses.Add(new Verse
                        {
                            BookId = bookCode.ToUpper(),
                            Book = bookName,
                            Chapter = chapter,
                            VerseNumber = verseNumber,
                            Text = verseText
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if parsing fails
            }

            return verses.OrderBy(v => v.VerseNumber).ToList();
        }

        /// <summary>
        /// Extract clean text from a verse element
        /// </summary>
        private static string ExtractVerseText(XElement verseElement)
        {
            var text = verseElement.Value;
            
            // Clean up common XML artifacts
            text = Regex.Replace(text, @"\s+", " "); // Normalize whitespace
            text = text.Trim();
            
            return text;
        }

        /// <summary>
        /// Get all available chapters for a book from XML content
        /// </summary>
        /// <param name="xmlContent">XML content to parse</param>
        /// <param name="bookCode">Book code</param>
        /// <returns>List of chapter numbers</returns>
        public static List<int> GetChaptersFromXml(string xmlContent, string bookCode)
        {
            var chapters = new List<int>();
            
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;

                if (root == null) return chapters;

                // Find the book element
                var bookElement = root.Descendants("book")
                    .FirstOrDefault(b => string.Equals(b.Attribute("code")?.Value, bookCode, StringComparison.OrdinalIgnoreCase));

                if (bookElement == null) return chapters;

                // Find all chapter elements
                var chapterElements = bookElement.Descendants("c").Where(c => c.Attribute("id") != null);

                foreach (var chapterElement in chapterElements)
                {
                    if (int.TryParse(chapterElement.Attribute("id")?.Value, out int chapterNumber))
                    {
                        chapters.Add(chapterNumber);
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if parsing fails
            }

            return chapters.OrderBy(c => c).ToList();
        }
    }
}