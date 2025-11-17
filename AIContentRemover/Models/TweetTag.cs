using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIContentRemover.Models;

/// <summary>
/// Représente un tweet taggé comme contenu IA par la communauté
/// </summary>
public class TweetTag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// ID unique du tweet Twitter (ex: "1234567890123456789")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string TweetId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de votes "contenu IA"
    /// </summary>
    public int AiVotes { get; set; } = 0;
    
    /// <summary>
    /// Nombre de votes "pas IA" (downvotes)
    /// </summary>
    public int NotAiVotes { get; set; } = 0;
    
    /// <summary>
    /// Date de création du premier tag
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date de dernière mise à jour
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Score calculé (AiVotes - NotAiVotes)
    /// </summary>
    [NotMapped]
    public int Score => AiVotes - NotAiVotes;
    
    /// <summary>
    /// Indique si le tweet dépasse le seuil de consensus
    /// </summary>
    [NotMapped]
    public bool IsAiContent => Score >= 3; // Seuil configurable
    
    /// <summary>
    /// Collection des votes individuels
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

