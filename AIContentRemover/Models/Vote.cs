using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIContentRemover.Models;

/// <summary>
/// Représente un vote individuel d'un utilisateur sur un tweet
/// </summary>
public class Vote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// ID du tweet taggé
    /// </summary>
    public long TweetTagId { get; set; }
    
    /// <summary>
    /// Navigation vers le tweet taggé
    /// </summary>
    [ForeignKey(nameof(TweetTagId))]
    public TweetTag TweetTag { get; set; } = null!;
    
    /// <summary>
    /// Identifiant anonyme de l'utilisateur (hash de l'IP ou UUID extension)
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string UserIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// Type de vote : true = AI, false = Not AI
    /// </summary>
    public bool IsAiVote { get; set; }
    
    /// <summary>
    /// Date du vote
    /// </summary>
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// IP address de l'utilisateur (optionnel, pour anti-spam)
    /// </summary>
    [MaxLength(45)] // IPv6
    public string? IpAddress { get; set; }
}

