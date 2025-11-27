using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public class Review_ReviewDbModel
{
    public int Id { get; set; }
    public string SubjectGuid { get; set; } = null!;
    public string SubjectOwnerGuid { get; set; } = null!;

    public string _likedByGuids { get; set; } = JsonSerializer.Serialize(new List<string>());
    [NotMapped]
    public List<string> LikedByGuids
    {
        get
        {
            try { return JsonSerializer.Deserialize<List<string>>(_likedByGuids) ?? []; }
            //log
            catch (Exception e) { Console.WriteLine(e.Message); return []; }
        }
        set
        {
            value ??= [];
            try { _likedByGuids = JsonSerializer.Serialize(value); }
            //log
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }

    //public List<Review_RateDbModel> Rates { get; set; } = [];

    public List<Review_CommentDbModel> Comments { get; set; } = [];
}
/*public class Review_RateDbModel
{
    public int Id { get; set; }
    public Review_ReviewDbModel ParentReview { get; set; } = null!;
    public Review_UserDbModel Voter { get; set; } = null!;
    public int Value { get; set; }
}*/
public class Review_CommentDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Review_ReviewDbModel ParentReview { get; set; } = null!;
    public string WriterGuid { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string _thumbsUpBy { get; set; } = JsonSerializer.Serialize(new List<string>());
    [NotMapped]
    public List<string> ThumbsUpBy
    {
        get
        {
            try { return JsonSerializer.Deserialize<List<string>>(_thumbsUpBy) ?? []; }
            //log
            catch (Exception e) { Console.WriteLine(e.Message); return []; }
        }
        set
        {
            value ??= [];
            try { _thumbsUpBy = JsonSerializer.Serialize(value); }
            //log
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }

    public string _thumbsDownBy { get; set; } = JsonSerializer.Serialize(new List<string>());
    [NotMapped]
    public List<string> ThumbsDownBy
    {
        get
        {
            try { return JsonSerializer.Deserialize<List<string>>(_thumbsDownBy) ?? []; }
            //log
            catch (Exception e)
            { Console.WriteLine(e.Message); return []; }
        }
        set
        {
            value ??= [];
            try { _thumbsDownBy = JsonSerializer.Serialize(value); }
            //log
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }

    public List<Review_CommentDbModel> Replies { get; set; } = [];
    public Review_CommentDbModel? ReplyTo { get; set; } = null;
}

public class Review_DbContext : DbContext
{
    public Review_DbContext(DbContextOptions<Review_DbContext> options) : base(options) { }

    public DbSet<Review_ReviewDbModel> Reviews { get; set; } = null!;
    //public DbSet<Review_RateDbModel> Rates { get; set; } = null!;
    public DbSet<Review_CommentDbModel> Comments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //************* Review_ReviewDbModel *************
        //************* One-to-Many Review-to-Rates *************
        /*modelBuilder.Entity<Review_ReviewDbModel>()
        .HasMany(r => r.Rates)
        .WithOne(rate => rate.ParentReview)
        .IsRequired(true);*/

        //************* One-to-Many Review-to-Comments *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasMany(r => r.Comments)
        .WithOne(c => c.ParentReview)
        .IsRequired(true);

        //************* One-to-Many Comment-to-Replies *************
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasMany(c => c.Replies)
        .WithOne(reply => reply.ReplyTo)
        .IsRequired(false);

        //************* Index Columns *************
        //************* Review_ReviewDbModel *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasIndex(r => r.SubjectGuid)
        .IsUnique(true);
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasIndex(r => r.SubjectOwnerGuid)
        .IsUnique(false);

        //************* Review_CommentDbModel *************
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasIndex(c => c.Guid)
        .IsUnique(true);
    }
}
