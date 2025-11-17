namespace AIContentRemover.Middleware;

/// <summary>
/// Middleware pour valider les API Keys
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Autoriser Swagger sans API Key en développement
        if (context.Request.Path.StartsWithSegments("/swagger") || 
            context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/api/tweets/health"))
        {
            await _next(context);
            return;
        }

        // Vérifier si l'authentification API Key est activée
        var requireApiKey = _configuration.GetValue<bool>("Security:RequireApiKey", false);
        
        if (!requireApiKey)
        {
            await _next(context);
            return;
        }

        // Vérifier la présence de l'API Key
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            _logger.LogWarning("API Key manquante pour {Path} depuis {IP}", 
                context.Request.Path, 
                context.Connection.RemoteIpAddress);
            
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "API Key requise",
                Details = $"Veuillez fournir une API Key dans le header '{API_KEY_HEADER}'"
            });
            return;
        }

        // Valider l'API Key
        var validApiKeys = _configuration.GetSection("Security:ApiKeys").Get<string[]>() ?? Array.Empty<string>();
        
        if (!validApiKeys.Contains(extractedApiKey.ToString()))
        {
            _logger.LogWarning("API Key invalide pour {Path} depuis {IP}", 
                context.Request.Path, 
                context.Connection.RemoteIpAddress);
            
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "API Key invalide"
            });
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension pour ajouter facilement le middleware
/// </summary>
public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}

