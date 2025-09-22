# BibleApi.Core

A shared library for Bible API functionality, providing reusable components, utilities, and data models for building Bible-related applications.

## Overview

BibleApi.Core extracts common functionality from Bible API implementations to enable code reuse across multiple projects. It provides a standardized set of models, constants, utilities, and interfaces for working with Bible data.

## Components

### Models (`BibleApi.Core.Models`)

Core data models for Bible API operations:

- **`Translation`** - Represents a Bible translation with metadata
- **`Verse`** - Represents a single Bible verse
- **`BookChapter`** - Represents a book chapter reference
- **Response Models** - Standard response formats for API endpoints:
  - `VerseResponse`
  - `ChaptersResponse`
  - `VersesInChapterResponse`
  - `RandomVerseResponse`
  - `TranslationsResponse`
  - `BooksResponse`

#### Database Models

Entity models for database operations:
- **`DbTranslation`** - Database entity for translations
- **`DbBook`** - Database entity for books
- **`DbVerse`** - Database entity for verses

#### XML Parsing Models

Models specific to XML data processing:
- **`ParsedBibleData`** - Container for parsed Bible data
- **`TranslationInfo`** - Translation information from XML
- **`BookData`** - Book data with verses
- **`VerseData`** - Individual verse data

### Constants (`BibleApi.Core.Constants`)

- **`BibleConstants`** - Canonical book lists, validation utilities, and Bible structure constants

### Utilities (`BibleApi.Core.Utilities`)

- **`BookMetadata`** - Book name normalization, chapter counts, and metadata utilities
- **`XmlParsingUtilities`** - XML parsing and translation metadata extraction utilities

### Interfaces (`BibleApi.Core.Interfaces`)

Service contracts for Bible data access:
- **`IBibleService`** - Base interface for Bible data services
- **`IXmlBibleService`** - Interface for XML-based Bible services
- **`IDatabaseBibleService`** - Interface for database-based Bible services

### Configuration (`BibleApi.Core.Configuration`)

Shared configuration models:
- **`BibleApiConfiguration`** - Complete API configuration
- **`AzureBlobConfiguration`** - Azure Blob Storage settings
- **`DatabaseConfiguration`** - Database connection settings
- **`CorsConfiguration`** - CORS policy settings

## Usage

### Adding to Your Project

Add a reference to the BibleApi.Core library:

```xml
<PackageReference Include="BibleApi.Core" Version="1.0.0" />
```

Or add a project reference:

```xml
<ProjectReference Include="../BibleApi.Core/BibleApi.Core.csproj" />
```

### Using the Models

```csharp
using BibleApi.Core.Models;

var verse = new Verse
{
    BookId = "GEN",
    Book = "Genesis",
    Chapter = 1,
    VerseNumber = 1,
    Text = "In the beginning God created the heaven and the earth."
};
```

### Using the Utilities

```csharp
using BibleApi.Core.Utilities;
using BibleApi.Core.Constants;

// Normalize book names
var bookCode = BookMetadata.Normalize("Genesis"); // Returns "GEN"
var bookName = BookMetadata.GetName("GEN"); // Returns "Genesis"
var chapterCount = BookMetadata.GetChapterCount("GEN"); // Returns 50

// Validate book IDs
bool isValid = BibleConstants.IsValidBookId("GEN"); // Returns true
bool isOT = BibleConstants.IsOldTestament("GEN"); // Returns true
```

### Implementing a Bible Service

```csharp
using BibleApi.Core.Interfaces;
using BibleApi.Core.Models;

public class MyBibleService : IBibleService
{
    public async Task<List<Translation>> ListTranslationsAsync()
    {
        // Implementation
    }
    
    public async Task<Translation?> GetTranslationInfoAsync(string identifier)
    {
        // Implementation
    }
    
    // ... other interface methods
}
```

## Package Information

- **Package ID**: BibleApi.Core
- **Version**: 1.0.0
- **Target Framework**: .NET 8.0
- **Authors**: Bible API Team
- **License**: [Specify License]

## Contributing

This library is designed to be consumed by Bible API implementations. When adding new functionality, ensure it's:

1. **Reusable** across different Bible API implementations
2. **Well-documented** with XML documentation comments
3. **Tested** with appropriate unit tests
4. **Consistent** with existing naming conventions and patterns

## Dependencies

- System.ComponentModel.Annotations (5.0.0)

## License

[Specify License Information]