using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using AspNetCoreRateLimit;
using AIContentRemover.Data;
using AIContentRemover.Services;
using AIContentRemover.Middleware;
using AIContentRemover.Validators;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION DE LA BASE DE DONNÉES =====
var isProduction = builder.Environment.IsProduction();
var connectionString = isProduction
    ? builder.Configuration.GetConnectionString("PostgreSQL")
    : builder.Configuration.GetConnectionString("SQLite");

if (isProduction)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}

// ===== CONFIGURATION CORS POUR EXTENSIONS NAVIGATEUR =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("BrowserExtensionPolicy", policy =>
    {
        policy.WithOrigins(
                "chrome-extension://*",
                "moz-extension://*",
                "safari-extension://*",
                "http://localhost:*",
                "https://localhost:*"
            )
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    // Policy permissive pour développement
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ===== CONFIGURATION RATE LIMITING =====
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/tweets/tag",
            Period = "1m",
            Limit = 10 // 10 votes par minute max
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100 // 100 requêtes par minute max
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// ===== SERVICES =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Service métier
builder.Services.AddScoped<ITweetTagService, TweetTagService>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<TagTweetRequestValidator>();

// ===== CONFIGURATION SWAGGER =====
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Content Remover API",
        Version = "v1",
        Description = "API REST pour détecter et tagger les tweets contenant du contenu IA",
        Contact = new OpenApiContact
        {
            Name = "AI Content Remover",
            Email = "support@aicontentremover.com"
        }
    });

    // Support pour API Key
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key nécessaire pour accéder à l'API. Format: X-API-Key: {votre_clé}",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    // Documentation XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ===== MIGRATIONS AUTOMATIQUES EN DÉVELOPPEMENT =====
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// ===== MIDDLEWARE PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Content Remover API v1");
        c.RoutePrefix = string.Empty; // Swagger en page d'accueil
    });
    
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("BrowserExtensionPolicy");
}

app.UseHttpsRedirection();

// Rate Limiting
app.UseIpRateLimiting();

// API Key Authentication
app.UseApiKeyAuthentication();

// Routing
app.MapControllers();

// Health check simple
app.MapGet("/health", () => Results.Ok(new 
{ 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Version = "1.0.0"
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
