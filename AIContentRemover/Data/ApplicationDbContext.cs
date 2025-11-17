using Microsoft.EntityFrameworkCore;
using AIContentRemover.Models;

namespace AIContentRemover.Data;

/// <summary>
/// DbContext principal pour la gestion des tweets taggés IA
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TweetTag> TweetTags { get; set; } = null!;
    public DbSet<Vote> Votes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration TweetTag
        modelBuilder.Entity<TweetTag>(entity =>
        {
            entity.ToTable("TweetTags");
            
            entity.HasKey(e => e.Id);
            
            // Index unique sur TweetId pour performance et unicité
            entity.HasIndex(e => e.TweetId)
                .IsUnique()
                .HasDatabaseName("IX_TweetTags_TweetId");
            
            // Index sur Score pour requêtes de classement
            entity.HasIndex(e => new { e.AiVotes, e.NotAiVotes })
                .HasDatabaseName("IX_TweetTags_Votes");
            
            // Index sur UpdatedAt pour requêtes temporelles
            entity.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_TweetTags_UpdatedAt");

            entity.Property(e => e.TweetId)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.AiVotes)
                .HasDefaultValue(0);

            entity.Property(e => e.NotAiVotes)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configuration Vote
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("Votes");
            
            entity.HasKey(e => e.Id);
            
            // Index composite pour éviter les votes multiples
            entity.HasIndex(e => new { e.TweetTagId, e.UserIdentifier })
                .IsUnique()
                .HasDatabaseName("IX_Votes_TweetTag_User");
            
            // Index sur IpAddress pour rate limiting
            entity.HasIndex(e => e.IpAddress)
                .HasDatabaseName("IX_Votes_IpAddress");

            entity.Property(e => e.UserIdentifier)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.VotedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation avec TweetTag
            entity.HasOne(v => v.TweetTag)
                .WithMany(t => t.Votes)
                .HasForeignKey(v => v.TweetTagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

