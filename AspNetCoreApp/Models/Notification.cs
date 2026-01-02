using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public class Notification_UserDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public ICollection<Notification_NotificationDbModel> Notifications { get; set; } = [];
    //public bool EnableForComments = true;
}
public class Notification_NotificationDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid? SubjectGuid { get; set; } = null;
    public Notification_UserDbModel Owner { get; set; } = null!;
    [MaxLength(60)]
    public string Title { get; set; } = null!;
    [MaxLength(500)]
    public string[] Description { get; set; } = [];
    [MaxLength(500)]
    public string? Link { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification_DbContext : DbContext
{
    public Notification_DbContext(DbContextOptions<Notification_DbContext> options) : base(options) { }

    public DbSet<Notification_UserDbModel> Users { get; set; } //= null!;
    public DbSet<Notification_NotificationDbModel> Notifications { get; set; } //= null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //************* One-to-Many User-to-Notifications *************
        modelBuilder.Entity<Notification_UserDbModel>()
        .HasMany(user => user.Notifications)
        .WithOne(notif => notif.Owner)
        .IsRequired(true);

        //************* Index Columns *************
        modelBuilder.Entity<Notification_UserDbModel>()
        .HasIndex(user => user.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Notification_NotificationDbModel>()
        .HasIndex(notif => notif.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Notification_NotificationDbModel>()
        .HasIndex(notif => notif.SubjectGuid);
        modelBuilder.Entity<Notification_NotificationDbModel>()
        .HasIndex(notif => notif.CreatedAt);
    }
}

public class Notification_Process
{
    /*
        readonly string SeedFileName;
        readonly DirectoryInfo Storage_Users;
        readonly DirectoryInfo Storage_Notifications;

        public Notification_Process(IWebHostEnvironment _env, IConfiguration config)
        {
            SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
            Storage_Users = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Notification", "Users"));
            Storage_Notifications = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Notification", "Notifications"));
        }
    */
    public async Task<Notification_UserDbModel?> CreateNewUser(Notification_DbContext notifDb, Guid userGuid)
    {
        bool userExist = await notifDb.Users.AnyAsync(u => u.Guid == userGuid);
        if (userExist) return null;

        Notification_UserDbModel userDbModel = new() { Guid = userGuid };
        notifDb.Users.Add(userDbModel);
        await notifDb.SaveChangesAsync();

        //seed
        //await Update_UserSeed(userDbModel.Guid);

        Notification_NotifCreation_FormModel welcomeNotifModel = new()
        {
            Title = "Welcome to HoLibz",
            Description = [
                "Your account created and confirmed successfully.",
                "It's great to have you here",
            ],
            OwnerGuid = userDbModel.Guid,
        };

        await CreateNewNotification(notifDb, welcomeNotifModel);

        return userDbModel;
    }
    public async Task CreateNewNotification(Notification_DbContext notifDb, Notification_NotifCreation_FormModel newNotifModel)
    {
        Notification_UserDbModel? ownerDbModel = await notifDb.Users
        .Where(u => u.Guid == newNotifModel.OwnerGuid)
        .Select(u => new Notification_UserDbModel()
        {
            Id = u.Id,
        })
        .FirstOrDefaultAsync();
        if (ownerDbModel is null)
        {
            var result = await CreateNewUser(notifDb, newNotifModel.OwnerGuid);

            if (result is null)
            {
                ownerDbModel = await notifDb.Users
                .Where(u => u.Guid == newNotifModel.OwnerGuid)
                .Select(u => new Notification_UserDbModel()
                {
                    Id = u.Id,
                })
                .FirstAsync();
            }
            else
            {
                ownerDbModel = result;
            }
        }

        if (notifDb.Users.Entry(ownerDbModel).State == EntityState.Detached)
        {
            //begin tracking
            notifDb.Users.Attach(ownerDbModel);
        }

        Notification_NotificationDbModel notifDbModel = new()
        {
            Description = newNotifModel.Description,
            Link = newNotifModel.Link,
            Owner = ownerDbModel,
            SubjectGuid = newNotifModel.SubjectGuid,
            Title = newNotifModel.Title,
        };

        notifDb.Notifications.Add(notifDbModel);
        await notifDb.SaveChangesAsync();

        //seed
        //await Update_NotificationSeed(notifDbModel.Guid, notifDb);
    }
    public async Task DeleteNotification(Notification_DbContext notifDb, Guid subjectGuid)
    {
        await notifDb.Notifications
        .Where(n => n.SubjectGuid == subjectGuid)
        .ExecuteDeleteAsync();
    }


    //************************************ seed User data **********************************
    /*
        public async Task Update_UserSeed(string dbModelGuid)
        {
            Notification_UserSeedModel? seedModel = Notification_UserSeedModel.Factory(dbModelGuid);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Users.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_UserDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Users.FullName, dbModelGuid);
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
        public async Task Seed_UsersToDb(Notification_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Users.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Users
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
                Notification_UserSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Notification_UserSeedModel>(json);
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
                    Notification_UserDbModel? dbModel = seedModel.GetDbModel();
                    if (dbModel is not null)
                    {
                        await libraryDb.Users.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed Notification data **********************************
        public async Task Update_NotificationSeed(string dbModelGuid, Notification_DbContext notifDb)
        {
            Notification_NotificationSeedModel? seedModel = await Notification_NotificationSeedModel.Factory(dbModelGuid, notifDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Notifications.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_NotificationDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Notifications.FullName, dbModelGuid);
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
        public async Task Seed_NotificationsToDb(Notification_DbContext notifDb)
        {
            foreach (var seedDirectory in Storage_Notifications.EnumerateDirectories())
            {
                var dbModelExist = await notifDb.Notifications
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Notifications.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Notification_NotificationSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Notification_NotificationSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Notification seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Notification_NotificationDbModel? dbModel = await seedModel.GetDbModel(notifDb);
                    if (dbModel is not null)
                    {
                        await notifDb.Notifications.AddAsync(dbModel);
                        await notifDb.SaveChangesAsync();
                    }
                }
            }
        }
    */
}
//************************************ Seed Models ********************************
/*
public class Notification_UserSeedModel
{
    public string Guid { get; set; } = null!;

    public static Notification_UserSeedModel Factory(string dbModel_Guid)
    {
        Notification_UserSeedModel seedModel = new()
        {
            Guid = dbModel_Guid,
        };

        return seedModel;
    }

    public Notification_UserDbModel GetDbModel()
    {
        Notification_UserDbModel userDbModel = new()
        {
            Guid = Guid,
        };

        return userDbModel;
    }
}

public class Notification_NotificationSeedModel
{
    public string Guid { get; set; } = null!;
    public string? SubjectGuid { get; set; } = null;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string[] Description { get; set; } = [];
    public string? Link { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static async Task<Notification_NotificationSeedModel?> Factory(string dbModel_Guid,
    Notification_DbContext notifDb)
    {
        Notification_NotificationSeedModel? seedModel = await notifDb.Notifications
        .Where(n => n.Guid == dbModel_Guid)
        .Include(n => n.Owner)
        .Select(n => new Notification_NotificationSeedModel()
        {
            Guid = n.Guid,
            CreatedAt = n.CreatedAt,
            Description = n.Description,
            Link = n.Link,
            OwnerGuid = n.Owner.Guid,
            SubjectGuid = n.SubjectGuid,
            Title = n.Title,
        })
        //.AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Notification_NotificationDbModel?> GetDbModel(Notification_DbContext notifDb)
    {
        Notification_UserDbModel? owner = await notifDb.Users
        .FirstOrDefaultAsync(u => u.Guid == OwnerGuid);
        if (owner is null)
        {
            //log
            Console.WriteLine($"\n     ***** owner Not found with guid '{OwnerGuid}'!");
            return null;
        }

        Notification_NotificationDbModel notificationDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Guid = Guid,
            Link = Link,
            Owner = owner,
            SubjectGuid = SubjectGuid,
            Title = Title,
        };

        return notificationDbModel;
    }
}
*/
//********************* Form models *************
public class Notification_NotifCreation_FormModel
{
    public Guid? SubjectGuid { get; set; } = null;
    public Guid OwnerGuid { get; set; }
    public string Title { get; set; } = null!;
    public string[] Description { get; set; } = [];
    public string? Link { get; set; } = null;
}

//********************* View models *************
public class Notification_NotifClient_ViewModel
{
    public Guid Guid { get; set; }
    public string Title { get; set; } = null!;
    public string[] Description { get; set; } = [];
    public string? Link { get; set; } = null;
    public DateTime CreatedAt { get; set; }
}