using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public class Review_UserDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = null!;

    public List<Review_ReviewDbModel> GotReviews { get; set; } = [];

    public List<Review_ReviewDbModel> GivenLikes { get; set; } = [];
    public List<Review_ReviewDbModel> GivenThumbsUps { get; set; } = [];
    public List<Review_ReviewDbModel> GivenThumbsDowns { get; set; } = [];
    public List<Review_RateDbModel> GivenRates { get; set; } = [];
    public List<Review_CommentDbModel> GivenComments { get; set; } = [];
}
public class Review_ReviewDbModel
{
    public int Id { get; set; }
    public string SubjectGuid { get; set; } = null!;
    public Review_UserDbModel SubjectOwner { get; set; } = null!;
    public List<Review_UserDbModel> LikedBy { get; set; } = [];
    public List<Review_UserDbModel> ThumbsUpBy { get; set; } = [];
    public List<Review_UserDbModel> ThumbsDownBy { get; set; } = [];
    public List<Review_RateDbModel> Rates { get; set; } = [];
    public List<Review_CommentDbModel> Comments { get; set; } = [];
}
public class Review_RateDbModel
{
    public int Id { get; set; }
    public Review_ReviewDbModel ParentReview { get; set; } = null!;
    public Review_UserDbModel Voter { get; set; } = null!;
    public int Value { get; set; }
}
public class Review_CommentDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Review_ReviewDbModel ParentReview { get; set; } = null!;
    public Review_UserDbModel Writer { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Review_DbContext : DbContext
{
    public Review_DbContext(DbContextOptions<Review_DbContext> options) : base(options) { }

    public DbSet<Review_UserDbModel> Users { get; set; } = null!;
    public DbSet<Review_ReviewDbModel> Reviews { get; set; } = null!;
    public DbSet<Review_RateDbModel> Rates { get; set; } = null!;
    public DbSet<Review_CommentDbModel> Comments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //************* Review_UserDbModel *************
        //************* One-to-Many User-to-GotReviews *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GotReviews)
        .WithOne(sub => sub.SubjectOwner)
        .IsRequired(true);

        //************* Many-to-Many Users-to-GivenLikes *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GivenLikes)
        .WithMany(r => r.LikedBy);

        //************* Many-to-Many Users-to-GivenThumbsUps *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GivenThumbsUps)
        .WithMany(r => r.ThumbsUpBy);

        //************* Many-to-Many Users-to-GivenThumbsDowns *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GivenThumbsDowns)
        .WithMany(r => r.ThumbsDownBy);

        //************* One-to-Many Voter-to-GivenRates *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GivenRates)
        .WithOne(rate => rate.Voter)
        .IsRequired(true);

        //************* One-to-Many Writer-to-GivenComments *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GivenComments)
        .WithOne(c => c.Writer)
        .IsRequired(true);

        //************* Review_ReviewDbModel *************
        //************* One-to-Many Review-to-Rates *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasMany(r => r.Rates)
        .WithOne(rate => rate.ParentReview)
        .IsRequired(true);

        //************* One-to-Many Review-to-Comments *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasMany(r => r.Comments)
        .WithOne(c => c.ParentReview)
        .IsRequired(true);

        //************* Index Columns *************
        //************* Review_UserDbModel *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasIndex(u => u.Guid)
        .IsUnique(true);

        //************* Review_ReviewDbModel *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasIndex(r => r.SubjectGuid)
        .IsUnique(true);
    }
}
