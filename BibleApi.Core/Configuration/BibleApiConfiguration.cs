using System.ComponentModel.DataAnnotations;

namespace BibleApi.Core.Configuration
{
    /// <summary>
    /// Base configuration for Bible API services
    /// </summary>
    public class BibleApiConfigurationBase
    {
        /// <summary>
        /// Application environment (development, production, etc.)
        /// </summary>
        public string Environment { get; set; } = "development";

        /// <summary>
        /// Base URL for the API (used for generating URLs in responses)
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Configuration for Azure Blob Storage access
    /// </summary>
    public class AzureBlobConfiguration
    {
        /// <summary>
        /// Azure Storage connection string for Bible XML files
        /// </summary>
        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Azure container name containing Bible translations
        /// </summary>
        public string ContainerName { get; set; } = "bible-translations";
    }

    /// <summary>
    /// Configuration for database access
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// SQL Server connection string
        /// </summary>
        [Required]
        public string ConnectionString { get; set; } = string.Empty;
    }

    /// <summary>
    /// CORS configuration settings
    /// </summary>
    public class CorsConfiguration
    {
        /// <summary>
        /// CORS allowed origins
        /// </summary>
        public string[] AllowedOrigins { get; set; } = new[] { "*" };

        /// <summary>
        /// CORS allowed methods
        /// </summary>
        public string[] AllowedMethods { get; set; } = new[] { "GET", "OPTIONS" };

        /// <summary>
        /// CORS allowed headers
        /// </summary>
        public string[] AllowedHeaders { get; set; } = new[] { "Content-Type" };
    }

    /// <summary>
    /// Complete Bible API configuration including all service options
    /// </summary>
    public class BibleApiConfiguration : BibleApiConfigurationBase
    {
        /// <summary>
        /// Azure Blob Storage configuration
        /// </summary>
        public AzureBlobConfiguration AzureBlob { get; set; } = new();

        /// <summary>
        /// Database configuration
        /// </summary>
        public DatabaseConfiguration Database { get; set; } = new();

        /// <summary>
        /// CORS configuration
        /// </summary>
        public CorsConfiguration Cors { get; set; } = new();
    }
}