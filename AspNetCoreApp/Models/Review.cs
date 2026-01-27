using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public class Review_UserDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; }
    [MaxLength(60)]
    public string NormalizedUserName { get; set; } = null!;
    public ICollection<Review_FollowerFollowing_DbModel> Followers { get; set; } = [];
    public ICollection<Review_FollowerFollowing_DbModel> Followings { get; set; } = [];
    public ICollection<Review_ReviewDbModel> GotReviews { get; set; } = [];
    public ICollection<Review_CommentDbModel> GiveComments { get; set; } = [];
    public ICollection<Review_UserLike_DbModel> GiveLikes { get; set; } = [];
    public ICollection<Review_UserThumbsUp_DbModel> GiveThumbsUps { get; set; } = [];
    public ICollection<Review_UserThumbsDown_DbModel> GiveThumbsDowns { get; set; } = [];
}
public class Review_ReviewDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid SubjectGuid { get; set; }
    public Review_UserDbModel Owner { get; set; } = null!;
    public ICollection<Review_UserLike_DbModel> LikedBy { get; set; } = [];
    public ICollection<Review_CommentDbModel> Comments { get; set; } = [];
}
public class Review_CommentDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Review_ReviewDbModel ParentReview { get; set; } = null!;
    public Review_UserDbModel Writer { get; set; } = null!;
    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;
    public ICollection<Review_UserThumbsUp_DbModel> ThumbsUpsBy { get; set; } = [];
    public ICollection<Review_UserThumbsDown_DbModel> ThumbsDownsBy { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? ReplyToId { get; set; } = null;
    public Review_CommentDbModel? ReplyTo { get; set; } = null;
    public ICollection<Review_CommentDbModel> Replies { get; set; } = [];
}

//***************** join tables ****************
public class Review_FollowerFollowing_DbModel
{
    public int FollowerId { get; set; }
    public Review_UserDbModel Follower { get; set; } = null!;

    public int FollowingId { get; set; }
    public Review_UserDbModel Following { get; set; } = null!;
}
public class Review_UserLike_DbModel
{
    public int UserId { get; set; }
    public Review_UserDbModel User { get; set; } = null!;

    public int ReviewId { get; set; }
    public Review_ReviewDbModel Review { get; set; } = null!;
}
public class Review_UserThumbsUp_DbModel
{
    public int UserId { get; set; }
    public Review_UserDbModel User { get; set; } = null!;

    public int CommentId { get; set; }
    public Review_CommentDbModel Comment { get; set; } = null!;
}
public class Review_UserThumbsDown_DbModel
{
    public int UserId { get; set; }
    public Review_UserDbModel User { get; set; } = null!;

    public int CommentId { get; set; }
    public Review_CommentDbModel Comment { get; set; } = null!;
}

public class Review_DbContext : DbContext
{
    public Review_DbContext(DbContextOptions<Review_DbContext> options) : base(options) { }

    public DbSet<Review_UserDbModel> Users { get; set; }
    public DbSet<Review_ReviewDbModel> Reviews { get; set; }
    public DbSet<Review_CommentDbModel> Comments { get; set; }

    //*************** join tables **************
    public DbSet<Review_FollowerFollowing_DbModel> FollowerFollowings { get; set; }
    public DbSet<Review_UserLike_DbModel> UserLikes { get; set; }
    public DbSet<Review_UserThumbsUp_DbModel> UserThumbsUp { get; set; }
    public DbSet<Review_UserThumbsDown_DbModel> UserThumbsDown { get; set; }

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
        modelBuilder.Entity<Review_UserLike_DbModel>()
        .HasKey(ul => new { ul.UserId, ul.ReviewId });

        modelBuilder.Entity<Review_UserLike_DbModel>()
        .HasOne(ul => ul.User)
        .WithMany(u => u.GiveLikes)
        .HasForeignKey(ul => ul.UserId);

        modelBuilder.Entity<Review_UserLike_DbModel>()
        .HasOne(ul => ul.Review)
        .WithMany(r => r.LikedBy)
        .HasForeignKey(ul => ul.ReviewId);

        //************* Many-to-Many User-to-Comment_ThumbsUp *************
        modelBuilder.Entity<Review_UserThumbsUp_DbModel>()
        .HasKey(uup => new { uup.UserId, uup.CommentId });

        modelBuilder.Entity<Review_UserThumbsUp_DbModel>()
        .HasOne(uup => uup.User)
        .WithMany(u => u.GiveThumbsUps)
        .HasForeignKey(uup => uup.UserId);

        modelBuilder.Entity<Review_UserThumbsUp_DbModel>()
        .HasOne(uup => uup.Comment)
        .WithMany(c => c.ThumbsUpsBy)
        .HasForeignKey(uup => uup.CommentId);

        //************* Many-to-Many User-to-Comment_ThumbsDown *************
        modelBuilder.Entity<Review_UserThumbsDown_DbModel>()
        .HasKey(udn => new { udn.UserId, udn.CommentId });

        modelBuilder.Entity<Review_UserThumbsDown_DbModel>()
        .HasOne(udn => udn.User)
        .WithMany(u => u.GiveThumbsDowns)
        .HasForeignKey(udn => udn.UserId);

        modelBuilder.Entity<Review_UserThumbsDown_DbModel>()
        .HasOne(udn => udn.Comment)
        .WithMany(c => c.ThumbsDownsBy)
        .HasForeignKey(udn => udn.CommentId);

        //************* Many-to-Many Followers-to-Followings *************
        modelBuilder.Entity<Review_FollowerFollowing_DbModel>()
        .HasKey(ff => new { ff.FollowerId, ff.FollowingId });

        modelBuilder.Entity<Review_FollowerFollowing_DbModel>()
        .HasOne(ff => ff.Follower)
        .WithMany(u => u.Followings)
        .HasForeignKey(ff => ff.FollowerId);

        modelBuilder.Entity<Review_FollowerFollowing_DbModel>()
        .HasOne(ff => ff.Following)
        .WithMany(u => u.Followers)
        .HasForeignKey(ff => ff.FollowingId);

        //************* Review_ReviewDbModel *************
        //************* One-to-Many Review-to-Comments *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasMany(r => r.Comments)
        .WithOne(c => c.ParentReview)
        .IsRequired(true);

        //************* Review_CommentDbModel *************
        //************* One-to-Many Comment-to-Replies *************
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasMany(c => c.Replies)
        .WithOne(c => c.ReplyTo)
        .HasForeignKey(c => c.ReplyToId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Cascade);

        //************* Index Columns *************
        //************* Review_ReviewDbModel *************
        modelBuilder.Entity<Review_UserDbModel>()
        .HasIndex(u => u.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Review_UserDbModel>()
        .HasIndex(u => u.NormalizedUserName)
        .IsUnique(true);

        //************* Review_ReviewDbModel *************
        modelBuilder.Entity<Review_ReviewDbModel>()
        .HasIndex(r => r.SubjectGuid)
        .IsUnique(true);

        //************* Review_CommentDbModel *************
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasIndex(c => c.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Review_CommentDbModel>()
        .HasIndex(c => c.CreatedAt);
    }
}

//*********************** Form Models **************************
public class Review_NewComment_FormModel
{
    public Guid ParentSubjectGuid { get; set; }

    [StringLength(1000)]
    public string Text { get; set; } = null!;
}
public class Review_NewReply_FormModel
{
    public Guid ParentCommentGuid { get; set; }

    [StringLength(1000)]
    public string Text { get; set; } = null!;
}

//*********************** View Models **************************
public class Review_Comment_ViewModel
{
    public Guid Guid { get; set; }
    public Guid WriterGuid { get; set; }
    public bool IsReply { get; set; } = false;
    public Guid? ReplyToGuid { get; set; }
    public string? ReplyToBrief { get; set; }
    public string? ReplyToUsername { get; set; }
    public string Text { get; set; } = null!;
    //public bool AmIThumbsUp { get; set; } = false;
    //public bool AmIThumbsDown { get; set; } = false;
    public int NumberOfThumbsUps { get; set; } = 0;
    public int NumberOfThumbsDowns { get; set; } = 0;
    public int NumberOfReplies { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
}
public class Review_Review_ViewModel
{
    //public bool AmILiked { get; set; } = false;
    public int NumberOfLikes { get; set; } = 0;
    public int TotalNumberOfComments { get; set; } = 0;
    public Guid[] CommentsGuids { get; set; } = [];
}


//*********************** Process **************************
public class Review_Process
{
    /*
        //readonly DirectoryInfo Storage_Users;
        //readonly DirectoryInfo Storage_Reviews;
        //readonly DirectoryInfo Storage_Comments;
        //readonly string SeedFileName;

        public Review_Process(IWebHostEnvironment _env, IConfiguration config)
        {
            //SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
            //Storage_Users = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Users"));
            //Storage_Reviews = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Reviews"));
            //Storage_Comments = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Review", "Comments"));
        }
    */
    public async Task CreateNewUser(Review_DbContext reviewDb, Guid userGuid,
    string normalizedUserName)
    {
        bool userExist = await reviewDb.Users.AnyAsync(u => u.Guid == userGuid);
        if (userExist) return;

        //here user doesn't exist
        Review_UserDbModel userDbModel = new()
        {
            Guid = userGuid,
            NormalizedUserName = normalizedUserName
        };
        reviewDb.Users.Add(userDbModel);
        await reviewDb.SaveChangesAsync();

    }
    public async Task CreateNewReview(Review_DbContext reviewDb, Guid subjectGuid,
    Guid ownerGuid)
    {
        bool reviewExist = await reviewDb.Reviews.AnyAsync(r => r.SubjectGuid == subjectGuid);
        if (reviewExist) return;

        //here review does not exist
        Review_UserDbModel? ownerDbModel = await reviewDb.Users
        .Where(u => u.Guid == ownerGuid)
        .Select(u => new Review_UserDbModel()
        {
            Id = u.Id,
        })
        .FirstOrDefaultAsync();
        if (ownerDbModel is null)
        {
            //log
            Console.WriteLine($"\n     ***** Couldn't find Review_UserDbModel with guid {ownerGuid} *****");
            return;
        }

        //begin tracking
        if (reviewDb.Users.Entry(ownerDbModel).State == EntityState.Detached)
        {
            reviewDb.Users.Attach(ownerDbModel);
        }

        Review_ReviewDbModel reviewDbModel = new()
        {
            SubjectGuid = subjectGuid,
            Owner = ownerDbModel,
        };

        reviewDb.Reviews.Add(reviewDbModel);
        await reviewDb.SaveChangesAsync();
    }
    public async Task DeleteReview(Review_DbContext reviewDb, Guid subjectGuid,
    Notification_DbContext notifDb, Notification_Process notifProcess)
    {
        Guid[] commentsGuids = await reviewDb.Reviews
        .Where(r => r.SubjectGuid == subjectGuid)
        .SelectMany(r => r.Comments)
        .Select(c => c.Guid)
        .ToArrayAsync();

        //delete review
        await reviewDb.Reviews.Where(r => r.SubjectGuid == subjectGuid).ExecuteDeleteAsync();

        //delete comment notifs
        await notifProcess.DeleteAllNotificationForSubjectArray(notifDb, commentsGuids);
    }

    /*
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
    */

    //************************************ seed User data **********************************
    /*
        public async Task Update_UserSeed(string userGuid, Review_DbContext reviewDb)
        {
            Review_UserSeedModel? seedModel = await Review_UserSeedModel.Factory(userGuid, reviewDb);
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
    */
}
//*********************** Seed Models **************************
/*
public class Review_UserSeedModel
{
    public string Guid { get; set; } = null!;
    public string NormalizedUserName { get; set; } = null!;

    public static async Task<Review_UserSeedModel?> Factory(string userGuid, Review_DbContext reviewDb)
    {
        Review_UserSeedModel? seedModel = await reviewDb.Users
        .Where(u => u.Guid == userGuid)
        .Select(u => new Review_UserSeedModel()
        {
            Guid = u.Guid,
            NormalizedUserName = u.NormalizedUserName,
        })
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public Review_UserDbModel GetDbModel()
    {
        Review_UserDbModel reviewDbModel = new()
        {
            Guid = Guid,
            NormalizedUserName = NormalizedUserName,
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
*/
