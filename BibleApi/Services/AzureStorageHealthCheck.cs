using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using BibleApi.Configuration;

namespace BibleApi.Services;

/// <summary>
/// Health check for Azure Storage connectivity
/// </summary>
public class AzureStorageHealthCheck : IHealthCheck
{
    private readonly AppSettings _settings;
    private readonly ILogger<AzureStorageHealthCheck> _logger;

    public AzureStorageHealthCheck(IOptions<AppSettings> settings, ILogger<AzureStorageHealthCheck> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.AzureStorageConnectionString))
            {
                return HealthCheckResult.Unhealthy("Azure Storage connection string not configured");
            }

            var blobServiceClient = new BlobServiceClient(_settings.AzureStorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.AzureContainerName);

            // Test connectivity with a simple operation
            var exists = await containerClient.ExistsAsync(cancellationToken);
            
            if (!exists.Value)
            {
                return HealthCheckResult.Degraded($"Azure Storage container '{_settings.AzureContainerName}' does not exist");
            }

            // Test by listing a few blobs
            var blobCount = 0;
            await foreach (var blob in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                blobCount++;
                if (blobCount >= 5) break; // Limit to first 5 blobs
            }

            var data = new Dictionary<string, object>
            {
                ["container"] = _settings.AzureContainerName,
                ["sample_blob_count"] = blobCount,
                ["connection_test"] = "successful"
            };

            return HealthCheckResult.Healthy("Azure Storage is accessible", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Storage health check failed");
            return HealthCheckResult.Unhealthy("Azure Storage health check failed", ex);
        }
    }
}