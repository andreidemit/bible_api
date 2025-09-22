using System.ComponentModel.DataAnnotations;
using BibleApi.Core;

namespace BibleApi.Validation
{
    /// <summary>
    /// Validates that a book ID is a valid Bible book
    /// </summary>
    public class ValidBookIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string bookId)
            {
                var normalized = BookMetadata.Normalize(bookId);
                return BibleConstants.IsValidBookId(normalized);
            }
            return value == null; // Allow null values - use [Required] for mandatory fields
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be a valid Bible book identifier.";
        }
    }

    /// <summary>
    /// Validates that a chapter number is within valid range for the book
    /// </summary>
    public class ValidChapterAttribute : ValidationAttribute
    {
        public string BookIdProperty { get; }

        public ValidChapterAttribute(string bookIdProperty)
        {
            BookIdProperty = bookIdProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not int chapter)
            {
                return new ValidationResult("Chapter must be a valid integer.");
            }

            if (chapter < 1)
            {
                return new ValidationResult("Chapter number must be greater than 0.");
            }

            // Try to get the book ID from the same object
            var bookIdProperty = validationContext.ObjectType.GetProperty(BookIdProperty);
            if (bookIdProperty != null)
            {
                var bookId = bookIdProperty.GetValue(validationContext.ObjectInstance) as string;
                if (!string.IsNullOrEmpty(bookId))
                {
                    var normalized = BookMetadata.Normalize(bookId);
                    var maxChapter = BookMetadata.GetChapterCount(normalized);
                    
                    if (chapter > maxChapter)
                    {
                        return new ValidationResult($"Chapter {chapter} is not valid for {BookMetadata.GetName(normalized)}. Maximum chapter is {maxChapter}.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Validates that a verse number is within a reasonable range
    /// </summary>
    public class ValidVerseAttribute : ValidationAttribute
    {
        public int MinVerse { get; set; } = 1;
        public int MaxVerse { get; set; } = 176; // Psalm 119 is the longest chapter

        public override bool IsValid(object? value)
        {
            if (value is int verse)
            {
                return verse >= MinVerse && verse <= MaxVerse;
            }
            return value == null; // Allow null values for optional parameters
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be between {MinVerse} and {MaxVerse}.";
        }
    }

    /// <summary>
    /// Validates that a translation ID is not empty and follows expected format
    /// </summary>
    public class ValidTranslationIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string translationId)
            {
                return !string.IsNullOrWhiteSpace(translationId) && 
                       translationId.Length >= 2 && 
                       translationId.Length <= 20 &&
                       translationId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be a valid translation identifier (2-20 characters, letters, numbers, hyphens, and underscores only).";
        }
    }

    /// <summary>
    /// Validates that a Bible reference string has proper format
    /// </summary>
    public class ValidBibleReferenceAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string reference && !string.IsNullOrWhiteSpace(reference))
            {
                // Basic format validation - could be enhanced with regex
                return reference.Length <= 100 && // reasonable length limit
                       !reference.Contains("..") && // no double dots
                       !reference.StartsWith(" ") && // no leading spaces
                       !reference.EndsWith(" "); // no trailing spaces
            }
            return value == null; // Allow null for optional fields
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be a properly formatted Bible reference.";
        }
    }
}