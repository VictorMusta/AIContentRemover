using Microsoft.AspNetCore.Mvc;
using AIContentRemover.Services;
using AIContentRemover.DTOs;
using FluentValidation;

namespace AIContentRemover.Controllers;

/// <summary>
/// Contrôleur pour la gestion des tweets taggés IA
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TweetsController : ControllerBase
{
    private readonly ITweetTagService _tweetTagService;
    private readonly ILogger<TweetsController> _logger;
    private readonly IValidator<TagTweetRequest> _tagValidator;
    private readonly IValidator<BatchCheckRequest> _batchValidator;

    public TweetsController(
        ITweetTagService tweetTagService,
        ILogger<TweetsController> logger,
        IValidator<TagTweetRequest> tagValidator,
        IValidator<BatchCheckRequest> batchValidator)
    {
        _tweetTagService = tweetTagService;
        _logger = logger;
        _tagValidator = tagValidator;
        _batchValidator = batchValidator;
    }

    /// <summary>
    /// Vérifier si un tweet est taggé comme contenu IA
    /// </summary>
    /// <param name="tweetId">ID du tweet Twitter</param>
    /// <returns>Informations sur le statut du tweet</returns>
    /// <response code="200">Tweet trouvé</response>
    /// <response code="400">TweetId invalide</response>
    [HttpGet("check/{tweetId}")]
    [ProducesResponseType(typeof(TweetCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TweetCheckResponse>> CheckTweet(string tweetId)
    {
        if (string.IsNullOrWhiteSpace(tweetId))
        {
            return BadRequest(new ErrorResponse 
            { 
                Error = "TweetId requis",
                Details = "Le paramètre tweetId ne peut pas être vide"
            });
        }

        var result = await _tweetTagService.CheckTweetAsync(tweetId);
        
        if (result == null)
        {
            return BadRequest(new ErrorResponse 
            { 
                Error = "TweetId invalide" 
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Vérifier plusieurs tweets en une seule requête (optimisé)
    /// </summary>
    /// <param name="request">Liste des IDs de tweets à vérifier</param>
    /// <returns>Liste des résultats pour chaque tweet</returns>
    /// <response code="200">Tweets vérifiés</response>
    /// <response code="400">Requête invalide</response>
    [HttpPost("check/batch")]
    [ProducesResponseType(typeof(BatchCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchCheckResponse>> CheckTweetsBatch([FromBody] BatchCheckRequest request)
    {
        var validation = await _batchValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Validation échouée",
                Details = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))
            });
        }

        var result = await _tweetTagService.CheckTweetsBatchAsync(request.TweetIds);
        return Ok(result);
    }

    /// <summary>
    /// Tagger un tweet comme contenu IA ou Not IA
    /// </summary>
    /// <param name="request">Données du vote</param>
    /// <returns>Résultat du vote</returns>
    /// <response code="200">Vote enregistré</response>
    /// <response code="400">Requête invalide</response>
    [HttpPost("tag")]
    [ProducesResponseType(typeof(TagTweetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TagTweetResponse>> TagTweet([FromBody] TagTweetRequest request)
    {
        var validation = await _tagValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Validation échouée",
                Details = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))
            });
        }

        // Récupérer l'IP de l'utilisateur pour anti-spam
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _tweetTagService.TagTweetAsync(
            request.TweetId, 
            request.UserIdentifier, 
            request.IsAiVote,
            ipAddress);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.Message
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtenir les statistiques globales de l'API
    /// </summary>
    /// <returns>Statistiques globales</returns>
    /// <response code="200">Statistiques récupérées</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(StatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatsResponse>> GetStats()
    {
        var result = await _tweetTagService.GetStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtenir les tweets les plus taggés comme IA
    /// </summary>
    /// <param name="limit">Nombre maximum de résultats (max 100)</param>
    /// <returns>Liste des tweets les plus taggés</returns>
    /// <response code="200">Liste récupérée</response>
    [HttpGet("top")]
    [ProducesResponseType(typeof(List<TweetCheckResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TweetCheckResponse>>> GetTopAiTweets([FromQuery] int limit = 50)
    {
        if (limit < 1 || limit > 100)
            limit = 50;

        var result = await _tweetTagService.GetTopAiTweetsAsync(limit);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint de santé pour vérifier que l'API fonctionne
    /// </summary>
    /// <returns>Status de l'API</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}

