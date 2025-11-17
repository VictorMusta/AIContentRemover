using Microsoft.EntityFrameworkCore;
using AIContentRemover.Data;
using AIContentRemover.Models;
using AIContentRemover.DTOs;

namespace AIContentRemover.Services;

/// <summary>
/// Service métier pour la gestion des tweets taggés IA
/// </summary>
public interface ITweetTagService
{
    Task<TweetCheckResponse?> CheckTweetAsync(string tweetId);
    Task<BatchCheckResponse> CheckTweetsBatchAsync(List<string> tweetIds);
    Task<TagTweetResponse> TagTweetAsync(string tweetId, string userIdentifier, bool isAiVote, string? ipAddress);
    Task<StatsResponse> GetStatsAsync();
    Task<List<TweetCheckResponse>> GetTopAiTweetsAsync(int limit = 50);
}

public class TweetTagService : ITweetTagService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TweetTagService> _logger;
    private readonly int _aiThreshold;

    public TweetTagService(
        ApplicationDbContext context, 
        ILogger<TweetTagService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _aiThreshold = configuration.GetValue<int>("AiContentThreshold", 3);
    }

    /// <summary>
    /// Vérifie si un tweet est taggé comme IA
    /// </summary>
    public async Task<TweetCheckResponse?> CheckTweetAsync(string tweetId)
    {
        if (string.IsNullOrWhiteSpace(tweetId))
            return null;

        var tweetTag = await _context.TweetTags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TweetId == tweetId);

        if (tweetTag == null)
        {
            return new TweetCheckResponse
            {
                TweetId = tweetId,
                IsAiContent = false,
                AiVotes = 0,
                NotAiVotes = 0,
                Score = 0,
                LastUpdated = null
            };
        }

        return new TweetCheckResponse
        {
            TweetId = tweetTag.TweetId,
            IsAiContent = tweetTag.Score >= _aiThreshold,
            AiVotes = tweetTag.AiVotes,
            NotAiVotes = tweetTag.NotAiVotes,
            Score = tweetTag.Score,
            LastUpdated = tweetTag.UpdatedAt
        };
    }

    /// <summary>
    /// Vérifie plusieurs tweets en une seule requête (optimisé)
    /// </summary>
    public async Task<BatchCheckResponse> CheckTweetsBatchAsync(List<string> tweetIds)
    {
        if (tweetIds == null || !tweetIds.Any())
            return new BatchCheckResponse();

        // Limiter à 100 tweets max par requête batch
        var limitedIds = tweetIds.Take(100).ToList();

        var tweetTags = await _context.TweetTags
            .AsNoTracking()
            .Where(t => limitedIds.Contains(t.TweetId))
            .ToDictionaryAsync(t => t.TweetId);

        var results = limitedIds.Select(tweetId =>
        {
            if (tweetTags.TryGetValue(tweetId, out var tag))
            {
                return new TweetCheckResponse
                {
                    TweetId = tag.TweetId,
                    IsAiContent = tag.Score >= _aiThreshold,
                    AiVotes = tag.AiVotes,
                    NotAiVotes = tag.NotAiVotes,
                    Score = tag.Score,
                    LastUpdated = tag.UpdatedAt
                };
            }

            return new TweetCheckResponse
            {
                TweetId = tweetId,
                IsAiContent = false,
                AiVotes = 0,
                NotAiVotes = 0,
                Score = 0,
                LastUpdated = null
            };
        }).ToList();

        return new BatchCheckResponse { Results = results };
    }

    /// <summary>
    /// Tagger un tweet comme IA ou Not IA (gère les votes multiples)
    /// </summary>
    public async Task<TagTweetResponse> TagTweetAsync(string tweetId, string userIdentifier, bool isAiVote, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(tweetId) || string.IsNullOrWhiteSpace(userIdentifier))
        {
            return new TagTweetResponse
            {
                Success = false,
                Message = "TweetId et UserIdentifier sont requis"
            };
        }

        try
        {
            // Récupérer ou créer le TweetTag
            var tweetTag = await _context.TweetTags
                .Include(t => t.Votes)
                .FirstOrDefaultAsync(t => t.TweetId == tweetId);

            if (tweetTag == null)
            {
                tweetTag = new TweetTag
                {
                    TweetId = tweetId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.TweetTags.Add(tweetTag);
                await _context.SaveChangesAsync(); // Sauvegarder pour obtenir l'ID
            }

            // Vérifier si l'utilisateur a déjà voté
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.TweetTagId == tweetTag.Id && v.UserIdentifier == userIdentifier);

            if (existingVote != null)
            {
                // Mettre à jour le vote existant
                var oldIsAiVote = existingVote.IsAiVote;
                existingVote.IsAiVote = isAiVote;
                existingVote.VotedAt = DateTime.UtcNow;

                // Ajuster les compteurs
                if (oldIsAiVote != isAiVote)
                {
                    if (isAiVote)
                    {
                        tweetTag.AiVotes++;
                        tweetTag.NotAiVotes--;
                    }
                    else
                    {
                        tweetTag.AiVotes--;
                        tweetTag.NotAiVotes++;
                    }
                }
            }
            else
            {
                // Nouveau vote
                var vote = new Vote
                {
                    TweetTagId = tweetTag.Id,
                    UserIdentifier = userIdentifier,
                    IsAiVote = isAiVote,
                    IpAddress = ipAddress,
                    VotedAt = DateTime.UtcNow
                };
                _context.Votes.Add(vote);

                // Incrémenter le compteur approprié
                if (isAiVote)
                    tweetTag.AiVotes++;
                else
                    tweetTag.NotAiVotes++;
            }

            tweetTag.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tweet {TweetId} tagged by {User}. Vote: {VoteType}", 
                tweetId, userIdentifier, isAiVote ? "AI" : "Not AI");

            var response = await CheckTweetAsync(tweetId);

            return new TagTweetResponse
            {
                Success = true,
                Message = existingVote != null ? "Vote mis à jour" : "Vote enregistré",
                TweetData = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du tag du tweet {TweetId}", tweetId);
            return new TagTweetResponse
            {
                Success = false,
                Message = "Erreur lors de l'enregistrement du vote"
            };
        }
    }

    /// <summary>
    /// Obtenir les statistiques globales
    /// </summary>
    public async Task<StatsResponse> GetStatsAsync()
    {
        var totalTweets = await _context.TweetTags.CountAsync();
        var totalVotes = await _context.Votes.CountAsync();
        var tweetsMarkedAsAI = await _context.TweetTags
            .CountAsync(t => (t.AiVotes - t.NotAiVotes) >= _aiThreshold);

        var lastUpdate = await _context.TweetTags
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => t.UpdatedAt)
            .FirstOrDefaultAsync();

        return new StatsResponse
        {
            TotalTweets = totalTweets,
            TotalVotes = totalVotes,
            TweetsMarkedAsAI = tweetsMarkedAsAI,
            LastUpdate = lastUpdate
        };
    }

    /// <summary>
    /// Obtenir les tweets les plus taggés comme IA
    /// </summary>
    public async Task<List<TweetCheckResponse>> GetTopAiTweetsAsync(int limit = 50)
    {
        var topTweets = await _context.TweetTags
            .AsNoTracking()
            .OrderByDescending(t => t.AiVotes - t.NotAiVotes)
            .Take(limit)
            .Select(t => new TweetCheckResponse
            {
                TweetId = t.TweetId,
                IsAiContent = (t.AiVotes - t.NotAiVotes) >= _aiThreshold,
                AiVotes = t.AiVotes,
                NotAiVotes = t.NotAiVotes,
                Score = t.AiVotes - t.NotAiVotes,
                LastUpdated = t.UpdatedAt
            })
            .ToListAsync();

        return topTweets;
    }
}

