using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public class Review_UserDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = null!;
    public string NormalizedUserName { get; set; } = null!;
    public List<Review_UserDbModel> Followers { get; set; } = [];
    public List<Review_UserDbModel> Followings { get; set; } = [];
    public List<Review_ReviewDbModel> GotReviews { get; set; } = [];
    public List<Review_CommentDbModel> GiveComments { get; set; } = [];
    public List<Review_ReviewDbModel> GiveLikes { get; set; } = [];
    public List<Review_CommentDbModel> GiveThumbsUps { get; set; } = [];
    public List<Review_CommentDbModel> GiveThumbsDowns { get; set; } = [];
}
public class Review_ReviewDbModel
{
    public int Id { get; set; }
    public string SubjectGuid { get; set; } = null!;
    public Review_UserDbModel Owner { get; set; } = null!;
    public List<Review_UserDbModel> LikedBy { get; set; } = [];
    public List<Review_CommentDbModel> Comments { get; set; } = [];
}
public class Review_CommentDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Review_ReviewDbModel ParentReview { get; set; } = null!;
    public Review_UserDbModel Writer { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public List<Review_UserDbModel> ThumbsUps { get; set; } = [];
    public List<Review_UserDbModel> ThumbsDowns { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Review_CommentDbModel? ReplyTo { get; set; } = null;
    public List<Review_CommentDbModel> Replies { get; set; } = [];
}

public class Review_DbContext : DbContext
{
    public Review_DbContext(DbContextOptions<Review_DbContext> options) : base(options) { }

    public DbSet<Review_UserDbModel> Users { get; set; } = null!;
    public DbSet<Review_ReviewDbModel> Reviews { get; set; } = null!;
    public DbSet<Review_CommentDbModel> Comments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //************* Review_UserDbModel *************
        //************* One-to-Many User-to-Review *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GotReviews)
        .WithOne(r => r.Owner)
        .IsRequired(true);

        //************* One-to-Many User-to-Review *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GiveComments)
        .WithOne(r => r.Writer)
        .IsRequired(true);

        //************* Many-to-Many User-to-Review_Likes *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GiveLikes)
        .WithMany(r => r.LikedBy);

        //************* Many-to-Many User-to-Comment_ThumbsUp *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GiveThumbsUps)
        .WithMany(r => r.ThumbsUps);

        //************* Many-to-Many User-to-Comment_ThumbsDown *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasMany(u => u.GiveThumbsDowns)
        .WithMany(r => r.ThumbsDowns);

        //************* Review_ReviewDbModel *************
        //************* One-to-Many Review-to-Comments *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasMany(r => r.Comments)
        .WithOne(c => c.ParentReview)
        .IsRequired(true);

        //************* Review_ReviewDbModel *************
        //************* One-to-Many Comment-to-Replies *************
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasMany(c => c.Replies)
        .WithOne(c => c.ReplyTo)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Cascade);

        //************* Index Columns *************
        //************* Review_ReviewDbModel *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasIndex(u => u.Guid)
        .IsUnique(true);

        //************* Review_ReviewDbModel *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasIndex(r => r.SubjectGuid)
        .IsUnique(true);

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
    readonly DirectoryInfo Storage_Users;
    readonly DirectoryInfo Storage_Reviews;
    readonly DirectoryInfo Storage_Comments;
    readonly string SeedFileName;

    public Review_Process(IWebHostEnvironment _env, IConfiguration config)
    {
        SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
        Storage_Users = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Users"));
        Storage_Reviews = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Reviews"));
        Storage_Comments = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Comments"));
    }

    public async Task CreateNewUser(Review_DbContext reviewDb, string userGuid)
    {
        Review_UserDbModel? userDbModel = await reviewDb.Users.FirstOrDefaultAsync(u => u.Guid == userGuid);
        if (userDbModel is not null) return;
        //here userDbModel is null
        userDbModel = new() { Guid = userGuid };
        await reviewDb.Users.AddAsync(userDbModel);
        await reviewDb.SaveChangesAsync();

        //seed
        await Update_UserSeed(userGuid, reviewDb);
    }
    public async Task CreateNewReview(Review_DbContext reviewDb, string subjectGuid,
    string ownerGuid)
    {
        Review_ReviewDbModel? reviewDbModel =
        await reviewDb.Reviews.FirstOrDefaultAsync(r => r.SubjectGuid == subjectGuid);
        if (reviewDbModel is not null) return;
        //here reviewDbModel is null
        Review_UserDbModel? ownerDbModel = await reviewDb.Users.FirstOrDefaultAsync(u => u.Guid == ownerGuid);
        if (ownerDbModel is null)
        {
            //log
            Console.WriteLine($"\n     ***** Couldn't find Review_UserDbModel with guid {ownerGuid} *****");
            return;
        }
        reviewDbModel = new()
        {
            SubjectGuid = subjectGuid,
            Owner = ownerDbModel,
        };

        await reviewDb.Reviews.AddAsync(reviewDbModel);
        await reviewDb.SaveChangesAsync();

        //seed
        await Update_ReviewSeed(reviewDbModel.SubjectGuid, reviewDb);
    }

    //public async Task DeleteUser(Review_DbContext reviewDb, string userGuid) { }
    public async Task DeleteReviewAndCommentsDirectories(Review_DbContext reviewDb, string subjectGuid,
    Notification_Process notifProcess, Notification_DbContext notifDb)
    {
        List<string> commentsGuids = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid)
        .Include(r => r.Comments)
        .SelectMany(r => r.Comments)
        .Select(c => c.Guid)
        .ToListAsync();

        foreach (string commentGuid in commentsGuids)
        {
            await DeleteCommentsDirectoriesRecursively(reviewDb, commentGuid, notifProcess, notifDb);
        }

        Delete_ReviewDirectory(subjectGuid);
    }
    public async Task DeleteCommentsDirectoriesRecursively(Review_DbContext reviewDb,
    string parentCommentGuid, Notification_Process notifProcess, Notification_DbContext notifDb)
    {
        List<string> deleteList = [parentCommentGuid];
        for (int i = 0; i < deleteList.Count; i++)
        {
            string commentGuid = deleteList[i];
            deleteList.AddRange(await GetRepliesGuids(reviewDb, commentGuid));
        }

        foreach (string commentGuid in deleteList)
        {
            Delete_CommentDirectory(commentGuid);
            await notifProcess.DeleteNotification(notifDb, commentGuid);
        }
    }
    private async Task<List<string>> GetRepliesGuids(Review_DbContext reviewDb,
    string parentCommentGuid)
    {
        List<string> repliesGuids = await reviewDb.Comments
        .Where(c => c.Guid == parentCommentGuid)
        .Include(c => c.Replies)
        .SelectMany(c => c.Replies)
        .Select(r => r.Guid)
        .ToListAsync();

        return repliesGuids;
    }


    //************************************ seed User data **********************************
    public async Task Update_UserSeed(string userGuid, Review_DbContext reviewDb)
    {
        Review_UserSeedModel? seedModel = Review_UserSeedModel.Factory(userGuid);
        if (seedModel is null) return;

        string json = JsonSerializer.Serialize(seedModel);
        DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Users.FullName, userGuid));
        string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
        await File.WriteAllTextAsync(seedPath, json);
    }
    public void Delete_UserDirectory(string userGuid)
    {
        string directoryPath = Path.Combine(Storage_Users.FullName, userGuid);
        if (Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** {e.Message} *****");
            }
        }
    }
    public async Task Seed_UserToDb(Review_DbContext reviewDb)
    {
        foreach (var seedDirectory in Storage_Users.EnumerateDirectories())
        {
            var dbModelExist = await reviewDb.Users
            .AnyAsync(o => o.Guid == seedDirectory.Name);
            if (dbModelExist)
            {
                continue;
            }

            string seedPath = Path.Combine(Storage_Users.FullName, seedDirectory.Name, SeedFileName);
            if (!File.Exists(seedPath))
            {
                continue;
            }

            string json = await File.ReadAllTextAsync(seedPath);
            Review_UserSeedModel? seedModel;
            try
            {
                seedModel = JsonSerializer.Deserialize<Review_UserSeedModel>(json);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** an exception occured during deserializing User seed data! guid: '{seedDirectory.Name}'");
                Console.WriteLine($"\n     ***** {e.Message} *****");
                continue;
            }
            if (seedModel is not null)
            {
                Review_UserDbModel dbModel = seedModel.GetDbModel();
                if (dbModel is not null)
                {
                    await reviewDb.Users.AddAsync(dbModel);
                    await reviewDb.SaveChangesAsync();
                }
            }
        }
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
    public void Delete_ReviewDirectory(string subjectGuid)
    {
        string directoryPath = Path.Combine(Storage_Reviews.FullName, subjectGuid);
        if (Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, true);
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
                Review_ReviewDbModel? dbModel = await seedModel.GetDbModel(reviewDb);
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
    public void Delete_CommentDirectory(string dbModelGuid)
    {
        string directoryPath = Path.Combine(Storage_Comments.FullName, dbModelGuid);
        if (Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, true);
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
public class Review_UserSeedModel
{
    public string Guid { get; set; } = null!;

    public static Review_UserSeedModel Factory(string userGuid)
    {
        Review_UserSeedModel seedModel = new() { Guid = userGuid };
        return seedModel;
    }

    public Review_UserDbModel GetDbModel()
    {
        Review_UserDbModel reviewDbModel = new()
        {
            Guid = Guid,
        };

        return reviewDbModel;
    }
}
public class Review_ReviewSeedModel
{
    public string SubjectGuid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string[] LikedByGuids { get; set; } = [];

    public static async Task<Review_ReviewSeedModel?> Factory(string subjectGuid,
    Review_DbContext reviewDb)
    {
        Review_ReviewSeedModel? seedModel = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid)
        .Include(r => r.Owner)
        .Include(r => r.LikedBy)
        .Select(r => new Review_ReviewSeedModel()
        {
            LikedByGuids = r.LikedBy.Select(u => u.Guid).ToArray(),
            SubjectGuid = r.SubjectGuid,
            OwnerGuid = r.Owner.Guid,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Review_ReviewDbModel?> GetDbModel(Review_DbContext reviewDb)
    {
        List<Review_UserDbModel> likes = await reviewDb.Users
        .Where(u => LikedByGuids.Contains(u.Guid))
        .ToListAsync();
        Review_UserDbModel? owner = await reviewDb.Users.FirstOrDefaultAsync(u => u.Guid == OwnerGuid);
        if (owner is null)
        {
            //log
            Console.WriteLine($"\n     ***** Review's owner cannot be null, ownerGuid {OwnerGuid} *****");
            return null;
        }
        Review_ReviewDbModel reviewDbModel = new()
        {
            LikedBy = likes,
            SubjectGuid = SubjectGuid,
            Owner = owner,
        };

        return reviewDbModel;
    }
}
public class Review_CommentSeedModel
{
    public string Guid { get; set; } = null!;
    public string ParentReviewGuid { get; set; } = null!;
    public string WriterGuid { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public string[] ThumbsUpsGuids { get; set; } = [];
    public string[] ThumbsDownsGuids { get; set; } = [];
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
        .Include(c => c.ThumbsUps)
        .Include(c => c.ThumbsDowns)
        .Include(c => c.Writer)
        .Select(c => new Review_CommentSeedModel()
        {
            Guid = c.Guid,
            CreatedAt = c.CreatedAt,
            ParentReviewGuid = c.ParentReview.SubjectGuid,
            RepliesGuids = c.Replies.Select(r => r.Guid).ToArray(),
            ReplyToGuid = c.ReplyTo == null ? null : c.ReplyTo.Guid,
            Text = c.Text,
            ThumbsDownsGuids = c.ThumbsDowns.Select(u => u.Guid).ToArray(),
            ThumbsUpsGuids = c.ThumbsUps.Select(u => u.Guid).ToArray(),
            WriterGuid = c.Writer.Guid,
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

        Review_ReviewDbModel? parentReview = await reviewDb.Reviews
        .FirstOrDefaultAsync(c => c.SubjectGuid == ParentReviewGuid);
        if (parentReview == null)
        {
            //log
            Console.WriteLine($"\n     ***** parent review Not found with guid '{ParentReviewGuid}'!");
            return null;
        }

        List<Review_UserDbModel> thumbsDowns = await reviewDb.Users
        .Where(u => ThumbsDownsGuids.Contains(u.Guid)).ToListAsync();

        List<Review_UserDbModel> thumbsUps = await reviewDb.Users
        .Where(u => ThumbsUpsGuids.Contains(u.Guid)).ToListAsync();

        Review_UserDbModel? writer = await reviewDb.Users.FirstOrDefaultAsync(u => u.Guid == WriterGuid);
        if (writer is null)
        {
            //log
            Console.WriteLine($"\n     ***** writer can not be null, writer guid: {WriterGuid} *****");
            return null;
        }

        Review_CommentDbModel commentDbModel = new()
        {
            CreatedAt = CreatedAt,
            Guid = Guid,
            ParentReview = parentReview,
            Replies = replies,
            ReplyTo = replyTo,
            Text = Text,
            ThumbsDowns = thumbsDowns,
            ThumbsUps = thumbsUps,
            Writer = writer,
        };

        return commentDbModel;
    }
}

