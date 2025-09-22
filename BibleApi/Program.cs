using BibleApi.Configuration;
using BibleApi.Services;
using BibleApi.TestDoubles;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Add services to the container
builder.Services.AddControllers();

// Add service based on configuration
var azureConnectionString = builder.Configuration.GetValue<string>("AppSettings:AzureStorageConnectionString");
if (string.IsNullOrWhiteSpace(azureConnectionString) && builder.Environment.IsDevelopment())
{
    // Use mock service in development when no Azure connection string is provided
    builder.Services.AddScoped<IAzureXmlBibleService, MockAzureXmlBibleService>();
}
else
{
    builder.Services.AddScoped<IAzureXmlBibleService, AzureXmlBibleService>();
}

// Add response caching
builder.Services.AddResponseCaching();

// Add compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Bible API", 
        Version = "v1",
        Description = "A JSON API for Bible verses and passages with search capabilities"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add compression middleware
app.UseResponseCompression();

// Add response caching middleware
app.UseResponseCaching();

// Add CORS middleware
app.UseCors();

// Health check endpoint
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Favicon endpoint
app.MapGet("/favicon.ico", () => Results.StatusCode(204));

app.UseAuthorization();

app.MapControllers();

app.Run();
