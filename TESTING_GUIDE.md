# Performance Testing Guide

This guide helps you test the performance improvements implemented in the Bible API.

## Prerequisites

1. **Start the API**: 
   ```bash
   cd BibleApi
   dotnet run --urls=http://localhost:5000
   ```

2. **Configure Azure Storage** (for full functionality):
   ```bash
   export AppSettings__AzureStorageConnectionString="your-azure-connection-string"
   export AppSettings__AzureContainerName="bible-translations"
   ```

## Running Performance Tests

### 1. Basic Performance Test
```bash
./test_performance.sh
```

This script tests:
- Response time improvements from caching
- HTTP cache headers
- Performance monitoring headers

### 2. Rate Limiting Test
```bash
./test_rate_limiting.sh
```

This script validates:
- Rate limiting functionality (100 requests/minute)
- HTTP 429 responses for exceeded limits

### 3. Manual Testing

**Health Check:**
```bash
curl http://localhost:5000/healthz
```

**Performance Headers:**
```bash
curl -I http://localhost:5000/v1/data
# Look for: X-Response-Time-Ms, Cache-Control headers
```

**Swagger Documentation:**
Open: http://localhost:5000/swagger

## Expected Results

### Performance Improvements:
- **First Request**: Baseline response time
- **Second Request**: Significantly faster (cached response)
- **Memory Usage**: Controlled through cache size limits
- **Rate Limiting**: Requests blocked after limit exceeded

### Headers to Observe:
- `X-Response-Time-Ms`: Request processing time
- `Cache-Control`: HTTP caching directives
- `Retry-After`: Rate limiting information (on 429 responses)

### Logging Output:
- Debug: All request timings
- Warning: Slow requests (>1 second)
- Error: Health check failures, Azure Storage issues

## Production Considerations

### 1. Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Production
AppSettings__AzureStorageConnectionString="your-connection"
AppSettings__AzureContainerName="bible-translations"
```

### 2. Monitoring
- Monitor `/healthz` endpoint for uptime
- Track `X-Response-Time-Ms` headers for performance
- Monitor rate limiting metrics for abuse detection

### 3. Scaling
- Use horizontal pod autoscaling based on CPU/memory
- Consider Azure CDN for global content distribution
- Implement distributed caching (Redis) for multi-instance deployments

## Troubleshooting

### Common Issues:
1. **Health Check Fails**: Verify Azure Storage connection string
2. **Rate Limiting Not Working**: Check rate limiting configuration in appsettings.json
3. **Slow Performance**: Verify cache is working with debug logging

### Debug Commands:
```bash
# Check cache hit rates (look for debug logs)
dotnet run --verbosity diagnostic

# Monitor memory usage
dotnet-counters monitor --process-id $(pgrep dotnet)

# Test specific endpoints
curl -v http://localhost:5000/v1/data
```

This comprehensive testing approach validates all performance improvements and ensures the API is production-ready.