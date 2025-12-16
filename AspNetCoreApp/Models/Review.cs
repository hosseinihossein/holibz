using System.ComponentModel.DataAnnotations;
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
    public Review_ReviewDbModel? ParentReview { get; set; } = null;
    public string WriterGuid { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Review_CommentDbModel? ReplyTo { get; set; } = null;
    public List<Review_CommentDbModel> Replies { get; set; } = [];
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
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Cascade);

        //************* One-to-Many Review-to-Comments *************
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasMany(c => c.Replies)
        .WithOne(c => c.ReplyTo)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Cascade);

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

//*********************** Data Models **************************
public class Review_NewCommentFormModel
{
    [StringLength(32)]
    public string ParentSubjectGuid { get; set; } = null!;

    [StringLength(1000)]
    public string Text { get; set; } = null!;
}
public class Review_NewReplyFormModel
{
    [StringLength(32)]
    public string ParentCommentGuid { get; set; } = null!;

    [StringLength(1000)]
    public string Text { get; set; } = null!;
}
public class Review_CommentModel
{
    public string Guid { get; set; } = null!;
    public string WriterGuid { get; set; } = null!;
    public bool IsReply { get; set; } = false;
    public string ReplyToGuid { get; set; } = "";
    public string ReplyToBrief { get; set; } = "";
    public string ReplyToUsername { get; set; } = "";
    public string Text { get; set; } = null!;
    public bool AmIThumbsUp { get; set; } = false;
    public bool AmIThumbsDown { get; set; } = false;
    public int NumberOfThumbsUps { get; set; } = 0;
    public int NumberOfThumbsDowns { get; set; } = 0;
    public int NumberOfReplies { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
}
public class Review_ReviewModel
{
    public bool AmILiked { get; set; } = false;
    public int NumberOfLikes { get; set; } = 0;
    public int TotalNumberOfComments { get; set; } = 0;
    public Review_CommentModel[] Comments { get; set; } = [];
}


//*********************** Process **************************
public class Review_Process
{
    readonly DirectoryInfo Storage_Reviews;
    readonly DirectoryInfo Storage_Comments;
    readonly string SeedFileName;

    public Review_Process(IWebHostEnvironment _env, IConfiguration config)
    {
        SeedFileName = config["SeedFileName"] ?? "data.json";
        Storage_Reviews = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Reviews"));
        Storage_Comments = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Comments"));
    }

    public async Task CreateNewReview(Review_DbContext reviewDb, string subjectGuid,
    string ownerGuid)
    {
        Review_ReviewDbModel reviewDbModel = new()
        {
            SubjectGuid = subjectGuid,
            SubjectOwnerGuid = ownerGuid,
        };

        await reviewDb.Reviews.AddAsync(reviewDbModel);
        await reviewDb.SaveChangesAsync();

        //seed
        _ = Update_ReviewSeed(reviewDbModel.SubjectGuid, reviewDb);
    }
    public async Task DeleteReview(Review_DbContext reviewDb, string subjectGuid)
    {

    }


    //************************************ seed Review data **********************************
    public async Task Update_ReviewSeed(string subjectGuid, Review_DbContext reviewDb)
    {
        Review_ReviewSeedModel? seedModel = await Review_ReviewSeedModel.Factory(subjectGuid, reviewDb);
        if (seedModel is null) return;

        string json = JsonSerializer.Serialize(seedModel);
        DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Reviews.FullName, subjectGuid));
        string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
        await File.WriteAllTextAsync(seedPath, json);
    }
    public void Delete_ReviewSeed(string subjectGuid)
    {
        string seedPath = Path.Combine(Storage_Reviews.FullName, subjectGuid, SeedFileName);
        if (File.Exists(seedPath))
        {
            try
            {
                File.Delete(seedPath);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** {e.Message} *****");
            }
        }
    }
    public async Task Seed_ReviewsToDb(Review_DbContext reviewDb)
    {
        foreach (var seedDirectory in Storage_Reviews.EnumerateDirectories())
        {
            var dbModelExist = await reviewDb.Reviews
            .AnyAsync(o => o.SubjectGuid == seedDirectory.Name);
            if (dbModelExist)
            {
                continue;
            }

            string seedPath = Path.Combine(Storage_Reviews.FullName, seedDirectory.Name, SeedFileName);
            if (!File.Exists(seedPath))
            {
                continue;
            }

            string json = await File.ReadAllTextAsync(seedPath);
            Review_ReviewSeedModel? seedModel;
            try
            {
                seedModel = JsonSerializer.Deserialize<Review_ReviewSeedModel>(json);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** an exception occured during deserializing Review seed data! guid: '{seedDirectory.Name}'");
                Console.WriteLine($"\n     ***** {e.Message} *****");
                continue;
            }
            if (seedModel is not null)
            {
                Review_ReviewDbModel? dbModel = seedModel.GetDbModel();
                if (dbModel is not null)
                {
                    await reviewDb.Reviews.AddAsync(dbModel);
                    await reviewDb.SaveChangesAsync();
                }
            }
        }
    }

    //************************************ seed Comment data **********************************
    public async Task Update_CommentSeed(string dbModelGuid, Review_DbContext reviewDb)
    {
        Review_CommentSeedModel? seedModel = await Review_CommentSeedModel.Factory(dbModelGuid, reviewDb);
        if (seedModel is null) return;

        string json = JsonSerializer.Serialize(seedModel);
        DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Comments.FullName, dbModelGuid));
        string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
        await File.WriteAllTextAsync(seedPath, json);
    }
    public void Delete_CommentSeed(string dbModelGuid)
    {
        string seedPath = Path.Combine(Storage_Comments.FullName, dbModelGuid, SeedFileName);
        if (File.Exists(seedPath))
        {
            try
            {
                File.Delete(seedPath);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** {e.Message} *****");
            }
        }
    }
    public async Task Seed_CommentsToDb(Review_DbContext reviewDb)
    {
        foreach (var seedDirectory in Storage_Comments.EnumerateDirectories())
        {
            var dbModelExist = await reviewDb.Comments
            .AnyAsync(o => o.Guid == seedDirectory.Name);
            if (dbModelExist)
            {
                continue;
            }

            string seedPath = Path.Combine(Storage_Comments.FullName, seedDirectory.Name, SeedFileName);
            if (!File.Exists(seedPath))
            {
                continue;
            }

            string json = await File.ReadAllTextAsync(seedPath);
            Review_CommentSeedModel? seedModel;
            try
            {
                seedModel = JsonSerializer.Deserialize<Review_CommentSeedModel>(json);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** an exception occured during deserializing Comment seed data! guid: '{seedDirectory.Name}'");
                Console.WriteLine($"\n     ***** {e.Message} *****");
                continue;
            }
            if (seedModel is not null)
            {
                Review_CommentDbModel? dbModel = await seedModel.GetDbModel(reviewDb);
                if (dbModel is not null)
                {
                    await reviewDb.Comments.AddAsync(dbModel);
                    await reviewDb.SaveChangesAsync();
                }
            }
        }
    }

}
//*********************** Seed Models **************************
public class Review_ReviewSeedModel
{
    public string SubjectGuid { get; set; } = null!;
    public string SubjectOwnerGuid { get; set; } = null!;
    public string[] LikedByGuids { get; set; } = [];

    public static async Task<Review_ReviewSeedModel?> Factory(string subjectGuid,
    Review_DbContext reviewDb)
    {
        Review_ReviewSeedModel? seedModel = await reviewDb.Reviews
        .Where(o => o.SubjectGuid == subjectGuid)
        .Select(o => new Review_ReviewSeedModel()
        {
            LikedByGuids = o.LikedByGuids.ToArray(),
            SubjectGuid = o.SubjectGuid,
            SubjectOwnerGuid = o.SubjectOwnerGuid,
        })
        //.AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public Review_ReviewDbModel? GetDbModel()
    {
        Review_ReviewDbModel reviewDbModel = new()
        {
            LikedByGuids = LikedByGuids.ToList(),
            SubjectGuid = SubjectGuid,
            SubjectOwnerGuid = SubjectOwnerGuid,
        };

        return reviewDbModel;
    }
}
public class Review_CommentSeedModel
{
    public string Guid { get; set; } = null!;
    public string? ParentReviewGuid { get; set; } = null;
    public string WriterGuid { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public string[] ThumbsUpBy { get; set; } = [];
    public string[] ThumbsDownBy { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string? ReplyToGuid { get; set; } = null;
    public string[] RepliesGuids { get; set; } = [];

    public static async Task<Review_CommentSeedModel?> Factory(string dbModel_Guid,
    Review_DbContext reviewDb)
    {
        Review_CommentSeedModel? seedModel = await reviewDb.Comments
        .Where(c => c.Guid == dbModel_Guid)
        .Include(c => c.ParentReview)
        .Include(c => c.ReplyTo)
        .Include(c => c.Replies)
        .Select(c => new Review_CommentSeedModel()
        {
            Guid = c.Guid,
            CreatedAt = c.CreatedAt,
            ParentReviewGuid = c.ParentReview == null ? null : c.ParentReview.SubjectGuid,
            RepliesGuids = c.Replies.Select(r => r.Guid).ToArray(),
            ReplyToGuid = c.ReplyTo == null ? null : c.ReplyTo.Guid,
            Text = c.Text,
            ThumbsDownBy = c.ThumbsDownBy.ToArray(),
            ThumbsUpBy = c.ThumbsUpBy.ToArray(),
            WriterGuid = c.WriterGuid,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Review_CommentDbModel?> GetDbModel(Review_DbContext reviewDb)
    {
        List<Review_CommentDbModel> replies = [];
        if (RepliesGuids.Length > 0)
        {
            replies = await reviewDb.Comments
            .Where(c => RepliesGuids.Contains(c.Guid))
            .ToListAsync();
        }

        Review_CommentDbModel? replyTo = null;
        if (ReplyToGuid is not null)
        {
            replyTo = await reviewDb.Comments
            .FirstOrDefaultAsync(c => c.Guid == ReplyToGuid);
            if (replyTo is null)
            {
                //log
                Console.WriteLine($"\n     ***** replyTo comment Not found with guid '{ReplyToGuid}'!");
                return null;
            }
        }

        Review_ReviewDbModel? parentReview = null;
        if (ParentReviewGuid is not null)
        {
            parentReview = await reviewDb.Reviews
            .FirstOrDefaultAsync(c => c.SubjectGuid == ParentReviewGuid);
            if (parentReview == null)
            {
                //log
                Console.WriteLine($"\n     ***** parent review Not found with guid '{ParentReviewGuid}'!");
                return null;
            }
        }

        Review_CommentDbModel commentDbModel = new()
        {
            CreatedAt = CreatedAt,
            Guid = Guid,
            ParentReview = parentReview,
            Replies = replies,
            ReplyTo = replyTo,
            Text = Text,
            ThumbsDownBy = ThumbsDownBy.ToList(),
            ThumbsUpBy = ThumbsUpBy.ToList(),
            WriterGuid = WriterGuid,
        };

        return commentDbModel;
    }
}

