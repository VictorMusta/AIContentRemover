namespace AIContentRemover.DTOs;

/// <summary>
/// DTO pour vérifier si un tweet est taggé IA
/// </summary>
public class TweetCheckResponse
{
    public string TweetId { get; set; } = string.Empty;
    public bool IsAiContent { get; set; }
    public int AiVotes { get; set; }
    public int NotAiVotes { get; set; }
    public int Score { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// DTO pour checker plusieurs tweets à la fois (batch)
/// </summary>
public class BatchCheckRequest
{
    public List<string> TweetIds { get; set; } = new();
}

/// <summary>
/// DTO pour la réponse batch
/// </summary>
public class BatchCheckResponse
{
    public List<TweetCheckResponse> Results { get; set; } = new();
}

/// <summary>
/// DTO pour tagger un tweet
/// </summary>
public class TagTweetRequest
{
    public string TweetId { get; set; } = string.Empty;
    public string UserIdentifier { get; set; } = string.Empty; // UUID de l'extension ou hash
    public bool IsAiVote { get; set; } = true; // true = AI, false = Not AI
}

/// <summary>
/// DTO pour la réponse de tag
/// </summary>
public class TagTweetResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TweetCheckResponse? TweetData { get; set; }
}

/// <summary>
/// DTO pour les statistiques globales
/// </summary>
public class StatsResponse
{
    public long TotalTweets { get; set; }
    public long TotalVotes { get; set; }
    public long TweetsMarkedAsAI { get; set; }
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// DTO pour les erreurs standardisées
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

