using BibleApi.Configuration;
using BibleApi.Services;
using BibleApi.Middleware;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Add services to the container
builder.Services.AddControllers();

// Configure memory cache with size limits
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000; // Limit cache entries
});

// Register AzureXmlBibleService as Singleton for better performance
// since it manages its own caching and doesn't hold per-request state
builder.Services.AddSingleton<IAzureXmlBibleService, AzureXmlBibleService>();

// Add rate limiting
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Add response caching
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .WithMethods("GET", "OPTIONS")
              .WithHeaders("Content-Type");
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureStorageHealthCheck>("azure_storage");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Bible API", 
        Version = "v1",
        Description = "A JSON API for Bible verses and passages"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add performance monitoring (should be early in pipeline)
app.UsePerformanceMonitoring();

// Add rate limiting middleware
app.UseIpRateLimiting();

// Add response caching middleware
app.UseResponseCaching();

// Add CORS middleware
app.UseCors();

// Health check endpoint with detailed checks
app.MapHealthChecks("/healthz");

// Favicon endpoint
app.MapGet("/favicon.ico", () => Results.StatusCode(204));

app.UseAuthorization();

// Configure response caching for static endpoints
app.Use(async (context, next) =>
{
    // Cache translations list for 5 minutes
    if (context.Request.Path.StartsWithSegments("/v1/data") && 
        context.Request.Path.Value?.Count(c => c == '/') == 2)
    {
        context.Response.Headers.CacheControl = "public, max-age=300";
    }
    // Cache book lists for 1 hour
    else if (context.Request.Path.StartsWithSegments("/v1/data") && 
             context.Request.Path.Value?.Count(c => c == '/') == 3)
    {
        context.Response.Headers.CacheControl = "public, max-age=3600";
    }
    
    await next();
});

app.MapControllers();

app.Run();
