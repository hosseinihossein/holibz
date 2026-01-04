using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreApp.Validators;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

//********************************************************************************
//************************************ DbModels **********************************
public class Library_OwnerDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    [MaxLength(60)]
    public string NormalizedUserName { get; set; } = null!;
    public ICollection<Library_LibraryDbModel> Libraries { get; set; } = [];
    public ICollection<Library_ShelfDbModel> Shelves { get; set; } = [];
    public ICollection<Library_DocumentDbModel> Documents { get; set; } = [];
    public ICollection<Library_ElementDbModel> Elements { get; set; } = [];
    public Guid DefaultLibraryGuid { get; set; }
    public Guid DefaultShelfGuid { get; set; }
    public ICollection<Library_FollowerFollowing_DbModel> Followers { get; set; } = [];
    public ICollection<Library_FollowerFollowing_DbModel> Followings { get; set; } = [];
    public ICollection<Library_UserFavoriteLibrary_DbModel> FavoriteLibraries { get; set; } = [];
    public ICollection<Library_UserFavoriteShelf_DbModel> FavoriteShelves { get; set; } = [];
    public ICollection<Library_UserFavoriteDocument_DbModel> FavoriteDocuments { get; set; } = [];
}
public class Library_LibraryDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Library_OwnerDbModel Owner { get; set; } = null!;
    [MaxLength(60)]
    public string Title { get; set; } = null!;
    [MaxLength(500)]
    public string? Description { get; set; } = null;
    public ICollection<Library_LibraryShelf_DbModel> Shelves { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public bool HasImage { get; set; } = false;
    public ICollection<Library_UserFavoriteLibrary_DbModel> InFavorOf { get; set; } = [];
}
public class Library_ShelfDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Library_OwnerDbModel Owner { get; set; } = null!;
    [MaxLength(60)]
    public string Title { get; set; } = null!;
    [MaxLength(500)]
    public string? Description { get; set; } = null;
    public ICollection<Library_LibraryShelf_DbModel> ParentLibraries { get; set; } = [];
    public ICollection<Library_ShelfDocument_DbModel> Documents { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public bool HasImage { get; set; } = false;
    public ICollection<Library_UserFavoriteShelf_DbModel> InFavorOf { get; set; } = [];
}
public class Library_DocumentDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Library_OwnerDbModel Owner { get; set; } = null!;
    [MaxLength(60)]
    public string Title { get; set; } = null!;
    [MaxLength(500)]
    public string Description { get; set; } = null!;
    [MaxLength(30)]
    public string Version { get; set; } = "Default";
    public Library_RelatedVersionsDbModel? RelatedVersions { get; set; }
    public ICollection<Library_ShelfDocument_DbModel> ParentShelves { get; set; } = [];
    public ICollection<Library_ElementDbModel> Elements { get; set; } = [];
    public ICollection<Library_DocumentTag_DbModel> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public bool HasImage { get; set; } = false;
    public ICollection<Library_UserFavoriteDocument_DbModel> InFavorOf { get; set; } = [];
}
public class Library_RelatedVersionsDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public ICollection<Library_DocumentDbModel> Documents { get; set; } = [];
}
public class Library_ElementDbModel
{
    [Key]
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Library_OwnerDbModel Owner { get; set; } = null!;
    [MaxLength(20)]
    public string Type { get; set; } = null!;
    [MaxLength(4000)]
    public string? Value { get; set; } = null;
    [MaxLength(60)]
    public string? Title { get; set; } = null;
    [MaxLength(60)]
    public string? FileName { get; set; } = null;
    public byte _order { get; set; } = 0;
    [NotMapped]
    public int Order
    {
        get => _order;
        set => _order = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public Library_DocumentDbModel ParentDocument { get; set; } = null!;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
public class Library_TagDbModel
{
    [Key]
    public int Id { get; set; }
    [MaxLength(30)]
    public string Name { get; set; } = null!;
    public ICollection<Library_DocumentTag_DbModel> Documents { get; set; } = [];
}

//************** join tables *************
public class Library_FollowerFollowing_DbModel
{
    public int FollowerId { get; set; }
    public Library_OwnerDbModel Follower { get; set; } = null!;

    public int FollowingId { get; set; }
    public Library_OwnerDbModel Following { get; set; } = null!;
}
public class Library_UserFavoriteLibrary_DbModel
{
    public int UserId { get; set; }
    public Library_OwnerDbModel User { get; set; } = null!;

    public int LibraryId { get; set; }
    public Library_LibraryDbModel Library { get; set; } = null!;
}
public class Library_UserFavoriteShelf_DbModel
{
    public int UserId { get; set; }
    public Library_OwnerDbModel User { get; set; } = null!;

    public int ShelfId { get; set; }
    public Library_ShelfDbModel Shelf { get; set; } = null!;
}
public class Library_UserFavoriteDocument_DbModel
{
    public int UserId { get; set; }
    public Library_OwnerDbModel User { get; set; } = null!;

    public int DocumentId { get; set; }
    public Library_DocumentDbModel Document { get; set; } = null!;
}

public class Library_LibraryShelf_DbModel
{
    public int LibraryId { get; set; }
    public Library_LibraryDbModel Library { get; set; } = null!;

    public int ShelfId { get; set; }
    public Library_ShelfDbModel Shelf { get; set; } = null!;
}
public class Library_ShelfDocument_DbModel
{
    public int ShelfId { get; set; }
    public Library_ShelfDbModel Shelf { get; set; } = null!;

    public int DocumentId { get; set; }
    public Library_DocumentDbModel Document { get; set; } = null!;
}
public class Library_DocumentTag_DbModel
{
    public int DocumentId { get; set; }
    public Library_DocumentDbModel Document { get; set; } = null!;

    public int TagId { get; set; }
    public Library_TagDbModel Tag { get; set; } = null!;
}


public class Library_DbContext : DbContext
{
    public Library_DbContext(DbContextOptions<Library_DbContext> options) : base(options) { }

    public DbSet<Library_OwnerDbModel> Owners { get; set; } //= null!;
    public DbSet<Library_LibraryDbModel> Libraries { get; set; } //= null!;
    public DbSet<Library_ShelfDbModel> Shelves { get; set; } //= null!;
    public DbSet<Library_DocumentDbModel> Documents { get; set; } //= null!;
    public DbSet<Library_RelatedVersionsDbModel> RelatedVersions { get; set; } //= null!;
    public DbSet<Library_ElementDbModel> Elements { get; set; } //= null!;
    public DbSet<Library_TagDbModel> Tags { get; set; } //= null!;

    //*********** join tables ***********
    public DbSet<Library_FollowerFollowing_DbModel> FollowerFollowings { get; set; }
    public DbSet<Library_UserFavoriteLibrary_DbModel> UserFavoriteLibraries { get; set; }
    public DbSet<Library_UserFavoriteShelf_DbModel> UserFavoriteShelves { get; set; }
    public DbSet<Library_UserFavoriteDocument_DbModel> UserFavoriteDocuments { get; set; }
    public DbSet<Library_LibraryShelf_DbModel> LibraryShelves { get; set; }
    public DbSet<Library_ShelfDocument_DbModel> ShelfDocuments { get; set; }
    public DbSet<Library_DocumentTag_DbModel> DocumentTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //*************************** Relationships *********************************
        //********************************** Owner ***********************************
        //*********** Owner-Libraries One-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.Libraries)
        .WithOne(l => l.Owner)
        .IsRequired(true);

        //*********** Owner-Shelves One-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.Shelves)
        .WithOne(sh => sh.Owner)
        .IsRequired(true);

        //*********** Owner-Documents One-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.Documents)
        .WithOne(doc => doc.Owner)
        .IsRequired(true);

        //*********** Owner-Elements One-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.Elements)
        .WithOne(el => el.Owner)
        .IsRequired(true);

        //*********** Followers-Followings Many-To-Many *********
        modelBuilder.Entity<Library_FollowerFollowing_DbModel>()
        .HasKey(ff => new { ff.FollowerId, ff.FollowingId });

        modelBuilder.Entity<Library_FollowerFollowing_DbModel>()
        .HasOne(ff => ff.Follower)
        .WithMany(u => u.Followers)
        .HasForeignKey(ff => ff.FollowerId);

        modelBuilder.Entity<Library_FollowerFollowing_DbModel>()
        .HasOne(ff => ff.Following)
        .WithMany(u => u.Followings)
        .HasForeignKey(ff => ff.FollowingId);

        //*********** Users-FavoriteLibraries Many-To-Many *********
        modelBuilder.Entity<Library_UserFavoriteLibrary_DbModel>()
        .HasKey(ul => new { ul.UserId, ul.LibraryId });

        modelBuilder.Entity<Library_UserFavoriteLibrary_DbModel>()
        .HasOne(ul => ul.User)
        .WithMany(o => o.FavoriteLibraries)
        .HasForeignKey(ul => ul.UserId);

        modelBuilder.Entity<Library_UserFavoriteLibrary_DbModel>()
        .HasOne(ul => ul.Library)
        .WithMany(lib => lib.InFavorOf)
        .HasForeignKey(ul => ul.LibraryId);

        //*********** Users-FavoriteShelves Many-To-Many *********
        modelBuilder.Entity<Library_UserFavoriteShelf_DbModel>()
        .HasKey(ul => new { ul.UserId, ul.ShelfId });

        modelBuilder.Entity<Library_UserFavoriteShelf_DbModel>()
        .HasOne(ul => ul.User)
        .WithMany(o => o.FavoriteShelves)
        .HasForeignKey(ul => ul.UserId);

        modelBuilder.Entity<Library_UserFavoriteShelf_DbModel>()
        .HasOne(ul => ul.Shelf)
        .WithMany(lib => lib.InFavorOf)
        .HasForeignKey(ul => ul.ShelfId);

        //*********** Users-FavoriteDocuments Many-To-Many *********
        modelBuilder.Entity<Library_UserFavoriteDocument_DbModel>()
        .HasKey(ul => new { ul.UserId, ul.DocumentId });

        modelBuilder.Entity<Library_UserFavoriteDocument_DbModel>()
        .HasOne(ul => ul.User)
        .WithMany(o => o.FavoriteDocuments)
        .HasForeignKey(ul => ul.UserId);

        modelBuilder.Entity<Library_UserFavoriteDocument_DbModel>()
        .HasOne(ul => ul.Document)
        .WithMany(lib => lib.InFavorOf)
        .HasForeignKey(ul => ul.DocumentId);


        //********************************** Library ***********************************
        //*********** Libraries-Shelves Many-To-Many *********
        modelBuilder.Entity<Library_LibraryShelf_DbModel>()
        .HasKey(ls => new { ls.LibraryId, ls.ShelfId });

        modelBuilder.Entity<Library_LibraryShelf_DbModel>()
        .HasOne(ls => ls.Library)
        .WithMany(lib => lib.Shelves)
        .HasForeignKey(ls => ls.LibraryId);

        modelBuilder.Entity<Library_LibraryShelf_DbModel>()
        .HasOne(ls => ls.Shelf)
        .WithMany(shelf => shelf.ParentLibraries)
        .HasForeignKey(ls => ls.ShelfId);


        //********************************** Shelf ***********************************
        //*********** Shelves-Documents Many-To-Many *********
        modelBuilder.Entity<Library_ShelfDocument_DbModel>()
        .HasKey(ls => new { ls.ShelfId, ls.DocumentId });

        modelBuilder.Entity<Library_ShelfDocument_DbModel>()
        .HasOne(ls => ls.Shelf)
        .WithMany(shelf => shelf.Documents)
        .HasForeignKey(ls => ls.ShelfId);

        modelBuilder.Entity<Library_ShelfDocument_DbModel>()
        .HasOne(ls => ls.Document)
        .WithMany(doc => doc.ParentShelves)
        .HasForeignKey(ls => ls.DocumentId);


        //********************************** RelatedVersions ***********************************
        //*********** RelatedVerions-Documents One-To-Many *********
        modelBuilder.Entity<Library_RelatedVersionsDbModel>()
        .HasMany(d => d.Documents)
        .WithOne(rv => rv.RelatedVersions)
        .IsRequired(false);


        //********************************** Document ***********************************
        //*********** Document-Elements One-To-Many *********
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasMany(d => d.Elements)
        .WithOne(e => e.ParentDocument)
        .IsRequired(true);
        //.OnDelete(DeleteBehavior.Cascade);//default for required entities


        //********************************** Tag ***********************************
        //*********** Tags-Documents Many-To-Many *********
        modelBuilder.Entity<Library_DocumentTag_DbModel>()
        .HasKey(ls => new { ls.DocumentId, ls.TagId });

        modelBuilder.Entity<Library_DocumentTag_DbModel>()
        .HasOne(dt => dt.Document)
        .WithMany(doc => doc.Tags)
        .HasForeignKey(dt => dt.DocumentId);

        modelBuilder.Entity<Library_DocumentTag_DbModel>()
        .HasOne(dt => dt.Tag)
        .WithMany(tag => tag.Documents)
        .HasForeignKey(dt => dt.TagId);


        //***************************************************************************
        //*************************** Index Columns *********************************
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasIndex(o => o.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasIndex(o => o.NormalizedUserName)
        .IsUnique(true);

        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasIndex(lib => lib.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasIndex(lib => lib.CreatedAt);

        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasIndex(shelf => shelf.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasIndex(shelf => shelf.CreatedAt);

        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasIndex(doc => doc.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasIndex(doc => doc.CreatedAt);

        modelBuilder.Entity<Library_RelatedVersionsDbModel>()
        .HasIndex(rv => rv.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Library_ElementDbModel>()
        .HasIndex(el => el.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_ElementDbModel>()
        .HasIndex(el => el.UpdatedAt);

        modelBuilder.Entity<Library_TagDbModel>()
        .HasIndex(tag => tag.Name)
        .IsUnique(true);

    }
}

//********************************************************************************
//*********************************** Processes **********************************
public class Library_Process //singleton service
{
    readonly DirectoryInfo Storage_Owners;
    public readonly DirectoryInfo Storage_Libraries;
    public readonly DirectoryInfo Storage_Shelves;
    public readonly DirectoryInfo Storage_Documents;
    public readonly DirectoryInfo Storage_Elements;
    //readonly DirectoryInfo Storage_Tags;
    //readonly DirectoryInfo Storage_RelatedVersions;
    readonly FileNameValidator fileNameValidator;
    //readonly string SeedFileName;

    public Library_Process(IWebHostEnvironment _env, FileNameValidator _fileNameValidator/*,
    IConfiguration config*/)
    {
        //SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
        Storage_Owners = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Owners"));
        Storage_Libraries = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Libraries"));
        Storage_Shelves = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Shelves"));
        Storage_Documents = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Documents"));
        Storage_Elements = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Elements"));
        //Storage_Tags = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Tags"));
        //Storage_RelatedVersions = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "RelatedVersions"));
        fileNameValidator = _fileNameValidator;
    }
    /*public string? BuildTagName(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            value = value.Trim().Replace(" ", "_").ToUpper();
            string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";//"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            for (int i = 0; i < value.Length; i++)
            {
                if (!allowedChars.Contains(value[i]))
                {
                    value = value.Replace(value[i].ToString(), "");
                    i--;
                    continue;
                }
            }
            if (value.Length > 3)
            {
                return value;
            }
        }
        return null;
    }*/

    public async Task CreateNewOwner(Library_DbContext libraryDb, Guid ownerGuid,
    string normalizedUserName)
    {
        bool ownerExist = await libraryDb.Owners
        .AnyAsync(o => o.Guid == ownerGuid);
        if (!ownerExist)
        {
            Library_LibraryDbModel defaultLibrary = new()
            {
                Description = "Containing all shelves that doesn't belong to anyother libraries.",
                Title = "Default Library",
            };

            Library_ShelfDbModel defaultShelf = new()
            {
                Description = "Containing all documents that doesn't belong to anyother shelves.",
                Title = "Default Shelf",
            };
            Library_LibraryShelf_DbModel libraryShelf = new()
            {
                Library = defaultLibrary,
                Shelf = defaultShelf,
            };
            libraryDb.LibraryShelves.Add(libraryShelf);

            Library_OwnerDbModel ownerDbModel = new()
            {
                Guid = ownerGuid,
                NormalizedUserName = normalizedUserName,
                DefaultLibraryGuid = defaultLibrary.Guid,
                DefaultShelfGuid = defaultShelf.Guid,
                Libraries = [defaultLibrary],
                Shelves = [defaultShelf],
            };

            libraryDb.Owners.Add(ownerDbModel);
            await libraryDb.SaveChangesAsync();
        }
    }
    public async Task<Library_ProcessResult> CreateNewLibrary(Library_DbContext libraryDb, Guid ownerGuid,
    Library_NewLibrary_FormModel formModel)
    {
        //fetch and create
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .Where(o => o.Guid == ownerGuid)
        .Select(o => new Library_OwnerDbModel() { Id = o.Id })
        .FirstOrDefaultAsync();
        if (ownerDbModel is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid.ToString("N")}'!",
                Success = false,
            };
            return processResult;
        }

        //attach
        libraryDb.Owners.Attach(ownerDbModel);

        Library_LibraryDbModel libraryDbModel = new()
        {
            Title = formModel.Title,
            Owner = ownerDbModel,
            Description = formModel.Description,
        };

        //first save image and set HasImage then save db
        if (formModel.Image is not null)
        {
            DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(
                Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid.ToString("N"))
            );
            string libraryImagePath = Path.Combine(libraryDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(libraryImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
            libraryDbModel.HasImage = true;
        }

        libraryDb.Libraries.Add(libraryDbModel);
        await libraryDb.SaveChangesAsync();

        return new Library_ProcessResult() { Success = true, ResultObject = libraryDbModel };
    }
    public async Task<Library_ProcessResult> CreateNewShelf(Library_DbContext libraryDb, Guid ownerGuid,
    Library_NewShelf_FormModel formModel)
    {
        //fetch and create
        var ownerDbInfo = await libraryDb.Owners
        .Where(o => o.Guid == ownerGuid)
        .Select(o => new
        {
            owner = new Library_OwnerDbModel() { Id = o.Id },
            specifiedLibraries = o.Libraries
                .Where(lib => formModel.LibraryGuids != null &&
                formModel.LibraryGuids.Length > 0 &&
                formModel.LibraryGuids.Contains(lib.Guid))
                .Select(lib => new Library_LibraryDbModel()
                {
                    Id = lib.Id,
                }),
            defaultLibrary = o.Libraries.Where(lib => lib.Guid == o.DefaultLibraryGuid)
                .Select(lib => new Library_LibraryDbModel()
                {
                    Id = lib.Id,
                })
                .Single(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (ownerDbInfo is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid.ToString("N")}'!",
                Success = false,
            };
            return processResult;
        }

        //attach
        libraryDb.Owners.Attach(ownerDbInfo.owner);

        Library_ShelfDbModel shelfDbModel = new()
        {
            Title = formModel.Title,
            Owner = ownerDbInfo.owner,
            Description = formModel.Description,
        };

        //save image and set HasImage property
        if (formModel.Image is not null)
        {
            DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(
                Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid.ToString("N"))
            );
            string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(shelfImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
            shelfDbModel.HasImage = true;
        }

        //add and begin tracking
        libraryDb.Shelves.Add(shelfDbModel);

        //set shelf parentLibraries
        IEnumerable<Library_LibraryDbModel> parentLibraries = ownerDbInfo.specifiedLibraries;
        if (!parentLibraries.Any())
        {
            parentLibraries = [ownerDbInfo.defaultLibrary];
        }

        //attach
        libraryDb.Libraries.AttachRange(parentLibraries);

        //create join tables
        foreach (Library_LibraryDbModel parentLib in parentLibraries)
        {
            Library_LibraryShelf_DbModel libShelf = new()
            {
                Library = parentLib,
                Shelf = shelfDbModel,
            };
            libraryDb.LibraryShelves.Add(libShelf);
        }

        //save
        await libraryDb.SaveChangesAsync();

        return new Library_ProcessResult() { Success = true, ResultObject = shelfDbModel };
    }
    public async Task<Library_ProcessResult> CreateNewDocument(Library_DbContext libraryDb, Guid ownerGuid,
    Library_NewDocument_FormModel formModel)
    {
        var ownerInfo = await libraryDb.Owners
        .Where(o => o.Guid == ownerGuid)
        .Select(o => new
        {
            owner = new Library_OwnerDbModel() { Id = o.Id },
            specifiedShelves = o.Shelves
                .Where(shelf => formModel.ShelfGuids != null &&
                formModel.ShelfGuids.Length > 0 &&
                formModel.ShelfGuids.Contains(shelf.Guid))
                .Select(shelf => new Library_ShelfDbModel()
                {
                    Id = shelf.Id,
                }),
            defaultShelf = o.Shelves.Where(shelf => shelf.Guid == o.DefaultShelfGuid)
                .Select(lib => new Library_ShelfDbModel()
                {
                    Id = lib.Id,
                })
                .Single(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (ownerInfo is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid.ToString("N")}'!",
                Success = false,
            };
            return processResult;
        }

        //attach
        libraryDb.Owners.Attach(ownerInfo.owner);

        Library_DocumentDbModel documentDbModel = new()
        {
            Description = formModel.Description,
            Owner = ownerInfo.owner,
            Title = formModel.Title,
        };

        if (formModel.Image is not null)
        {
            DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, documentDbModel.Guid.ToString("N")));
            string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(documentImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
            documentDbModel.HasImage = true;
        }

        //add and begin tracking
        libraryDb.Documents.Add(documentDbModel);

        IEnumerable<Library_ShelfDbModel> parentShelves = ownerInfo.specifiedShelves;
        if (!parentShelves.Any())
        {
            parentShelves = [ownerInfo.defaultShelf];
        }

        //attach
        libraryDb.Shelves.AttachRange(parentShelves);

        foreach (Library_ShelfDbModel parentShelf in parentShelves)
        {
            Library_ShelfDocument_DbModel shelfDocument = new()
            {
                Shelf = parentShelf,
                Document = documentDbModel,
            };
            libraryDb.ShelfDocuments.Add(shelfDocument);
        }

        await libraryDb.SaveChangesAsync();

        return new Library_ProcessResult()
        {
            Success = true,
            ResultObject = documentDbModel,
        };
    }
    public async Task<Library_ProcessResult> CreateNewElement(Library_DbContext libraryDb, Guid ownerGuid,
    Library_NewElement_FormModel formModel)
    {
        //fetch and create
        var documentInfo = await libraryDb.Documents
        .Where(doc => doc.Guid == formModel.DocumentGuid && doc.Owner.Guid == ownerGuid)
        .Select(doc => new
        {
            owner = new Library_OwnerDbModel() { Id = doc.Owner.Id },
            document = new Library_DocumentDbModel() { Id = doc.Id },
            elementsWithGreaterEqualOrder = doc.Elements
            .Where(el => el._order >= (byte)formModel.Order)
            .Select(el => new Library_ElementDbModel()
            {
                Id = el.Id,
            }),
        })
        .FirstOrDefaultAsync();

        if (documentInfo is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "documentGuid",
                ErrorDescription = $"Couldn't find any parent document with guid '{formModel.DocumentGuid.ToString("N")}' and the ownerGuid '{ownerGuid}'!",
                Success = false,
            };
            return processResult;
        }

        //attach
        libraryDb.Owners.Attach(documentInfo.owner);
        libraryDb.Documents.Attach(documentInfo.document);

        Library_ElementDbModel newElementDbmodel;

        if (formModel.Type == "h1" || formModel.Type == "h2" || formModel.Type == "p" ||
        formModel.Type == "code" || formModel.Type == "link")
        {
            newElementDbmodel = new()
            {
                ParentDocument = documentInfo.document,
                Order = formModel.Order,
                Owner = documentInfo.owner,
                Title = formModel.Title,
                Type = formModel.Type,
                Value = formModel.Value,
            };
        }
        else if ((formModel.Type == "img" || formModel.Type == "file") && formModel.File is not null)
        {
            //get a valid file name
            string validFileName =
            fileNameValidator.GetValidFileName(WebUtility.HtmlEncode(formModel.File.FileName));

            newElementDbmodel = new()
            {
                ParentDocument = documentInfo.document,
                Order = formModel.Order,
                Owner = documentInfo.owner,
                Title = formModel.Title,
                Type = formModel.Type,
                FileName = validFileName,
            };

            DirectoryInfo elementDirectoryInfo = Directory.CreateDirectory(
                Path.Combine(Storage_Elements.FullName, newElementDbmodel.Guid.ToString("N"))
            );
            string elementFilePath = Path.Combine(elementDirectoryInfo.FullName, validFileName);
            using (FileStream fs = System.IO.File.Create(elementFilePath))
            {
                await formModel.File.CopyToAsync(fs);
            }
        }
        else
        {
            return new Library_ProcessResult()
            {
                Success = false,
                ErrorTitle = "Element Type",
                ErrorDescription = $"The element Type is unknown! Element type: '{formModel.Type}'",
            };
        }

        //add
        libraryDb.Elements.Add(newElementDbmodel);

        //attach
        //libraryDb.Elements.AttachRange(documentInfo.elementsWithGreaterEqualOrder);
        //reorder
        foreach (var reOrderElement in documentInfo.elementsWithGreaterEqualOrder)
        {
            //edit
            reOrderElement.Order += 1;
            //modified
            if (libraryDb.Elements.Entry(reOrderElement).State == EntityState.Detached)
            {
                libraryDb.Elements.Entry(reOrderElement).Property(el => el._order).IsModified = true;
            }
        }
        //save
        await libraryDb.SaveChangesAsync();

        return new Library_ProcessResult()
        {
            Success = true,
            ResultObject = newElementDbmodel,
        };

    }


    //************************************ storage **********************************
    public void Delete_OwnerDirectory(string dbModelGuid)
    {
        string directoryPath = Path.Combine(Storage_Owners.FullName, dbModelGuid);
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
    public void Delete_LibraryDirectory(string dbModelGuid)
    {
        string directoryPath = Path.Combine(Storage_Libraries.FullName, dbModelGuid);
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
    public void Delete_ShelfDirectory(string dbModelGuid)
    {
        string directoryPath = Path.Combine(Storage_Shelves.FullName, dbModelGuid);
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
    public void Delete_DocumentDirectory(string dbModelGuid)
    {
        string directoryPath = Path.Combine(Storage_Documents.FullName, dbModelGuid);
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
    public void Delete_ElementDirectory(string dbModelGuid)
    {
        string directoryPath = Path.Combine(Storage_Elements.FullName, dbModelGuid);
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


    /*
        //************************************ seed Owner data **********************************
        public async Task Update_OwnerSeed(string dbModelGuid, Library_DbContext libraryDb)
        {
            Library_OwnerSeedModel? seedModel = await Library_OwnerSeedModel.Factory(dbModelGuid, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Owners.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_OwnerDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Owners.FullName, dbModelGuid);
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
        public async Task Seed_OwnersToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Owners.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Owners
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Owners.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_OwnerSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_OwnerSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Owner seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_OwnerDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.Owners.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed Library data **********************************
        public async Task Update_LibrarySeed(string dbModelGuid, Library_DbContext libraryDb)
        {
            Library_LibrarySeedModel? seedModel = await Library_LibrarySeedModel.Factory(dbModelGuid, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_LibraryDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Libraries.FullName, dbModelGuid);
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
        public async Task Seed_LibrariesToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Libraries.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Libraries
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Libraries.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_LibrarySeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_LibrarySeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Library seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_LibraryDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.Libraries.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed Shelf data **********************************
        public async Task Update_ShelvesSeeds(string[] dbModelsGuids, Library_DbContext libraryDb)
        {
            foreach (string guid in dbModelsGuids)
            {
                await Update_ShelfSeed(guid, libraryDb);
            }
        }
        public async Task Update_ShelfSeed(string dbModelGuid, Library_DbContext libraryDb)
        {
            Library_ShelfSeedModel? seedModel = await Library_ShelfSeedModel.Factory(dbModelGuid, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_ShelfDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Shelves.FullName, dbModelGuid);
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
        public async Task Seed_ShelvesToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Shelves.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Shelves
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Shelves.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_ShelfSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_ShelfSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Shelf seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_ShelfDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.Shelves.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed Document data **********************************
        public async Task Update_DocumentsSeeds(string[] dbModelsGuids, Library_DbContext libraryDb)
        {
            foreach (string guid in dbModelsGuids)
            {
                await Update_DocumentSeed(guid, libraryDb);
            }
        }
        public async Task Update_DocumentSeed(string dbModelGuid, Library_DbContext libraryDb)
        {
            Library_DocumentSeedModel? seedModel = await Library_DocumentSeedModel.Factory(dbModelGuid, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_DocumentDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Documents.FullName, dbModelGuid);
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
        public async Task Seed_DocumentsToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Documents.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Documents
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Documents.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_DocumentSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_DocumentSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Document seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_DocumentDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.Documents.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed Element data **********************************
        public async Task Update_ElementSeed(string dbModelGuid, Library_DbContext libraryDb)
        {
            Library_ElementSeedModel? seedModel = await Library_ElementSeedModel.Factory(dbModelGuid, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Elements.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_ElementDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_Elements.FullName, dbModelGuid);
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
        public async Task Seed_ElementsToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Elements.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Elements
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Elements.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_ElementSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_ElementSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Element seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_ElementDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.Elements.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed RelatedVersions data **********************************
        public async Task Update_RelatedVersionsSeed(string dbModelGuid, Library_DbContext libraryDb)
        {
            Library_RelatedVersionsSeedModel? seedModel = await Library_RelatedVersionsSeedModel.Factory(dbModelGuid, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_RelatedVersions.FullName, dbModelGuid));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_RelatedVersionsDirectory(string dbModelGuid)
        {
            string directoryPath = Path.Combine(Storage_RelatedVersions.FullName, dbModelGuid);
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
        public async Task Seed_RelatedVersionsToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_RelatedVersions.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.RelatedVersions
                .AnyAsync(o => o.Guid == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_RelatedVersions.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_RelatedVersionsSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_RelatedVersionsSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing RelatedVersions seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_RelatedVersionsDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.RelatedVersions.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed Tag data **********************************
        public async Task Update_TagSeed(string tagName, Library_DbContext libraryDb)
        {
            Library_TagSeedModel? seedModel = await Library_TagSeedModel.Factory(tagName, libraryDb);
            if (seedModel is null) return;

            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Tags.FullName, tagName));
            string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_TagDirectory(string tagName)
        {
            string directoryPath = Path.Combine(Storage_Tags.FullName, tagName);
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
        public async Task Seed_TagsToDb(Library_DbContext libraryDb)
        {
            foreach (var seedDirectory in Storage_Tags.EnumerateDirectories())
            {
                var dbModelExist = await libraryDb.Tags
                .AnyAsync(o => o.Name == seedDirectory.Name);
                if (dbModelExist)
                {
                    continue;
                }

                string seedPath = Path.Combine(Storage_Tags.FullName, seedDirectory.Name, SeedFileName);
                if (!File.Exists(seedPath))
                {
                    continue;
                }

                string json = await File.ReadAllTextAsync(seedPath);
                Library_TagSeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_TagSeedModel>(json);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** an exception occured during deserializing Tag seed data! guid: '{seedDirectory.Name}'");
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                    continue;
                }
                if (seedModel is not null)
                {
                    Library_TagDbModel? dbModel = await seedModel.GetDbModel(libraryDb);
                    if (dbModel is not null)
                    {
                        await libraryDb.Tags.AddAsync(dbModel);
                        await libraryDb.SaveChangesAsync();
                    }
                }
            }
        }

        //************************************ seed DB **********************************
    
        public async Task SeedLibraryDb(Library_DbContext libraryDb)
        {
            await Seed_OwnersToDb(libraryDb);
            await Seed_LibrariesToDb(libraryDb);
            await Seed_ShelvesToDb(libraryDb);
            await Seed_DocumentsToDb(libraryDb);
            await Seed_ElementsToDb(libraryDb);
            await Seed_RelatedVersionsToDb(libraryDb);
            await Seed_TagsToDb(libraryDb);
        }
    */
}
public class Library_ProcessResult
{
    public bool Success { get; set; } = false;
    public string? ErrorTitle { get; set; } = null;
    public string? ErrorDescription { get; set; } = null;
    public object? ResultObject { get; set; } = null;
}

//************************************ Seed Models ********************************
/*
public class Library_OwnerSeedModel
{
    public string Guid { get; set; } = null!;
    public string NormalizedUserName { get; set; } = null!;
    public string DefaultLibraryGuid { get; set; } = null!;
    public string DefaultShelfGuid { get; set; } = null!;
    public string[] FollowersGuids { get; set; } = [];
    public string[] FollowingsGuids { get; set; } = [];

    public static async Task<Library_OwnerSeedModel?> Factory(string dbModel_Guid,
    Library_DbContext libraryDb)
    {
        Library_OwnerSeedModel? seedModel = await libraryDb.Owners
        .Where(o => o.Guid == dbModel_Guid)
        .Include(o => o.Followers)
        .Include(o => o.Followings)
        .Select(o => new Library_OwnerSeedModel()
        {
            Guid = o.Guid,
            DefaultLibraryGuid = o.DefaultLibraryGuid,
            DefaultShelfGuid = o.DefaultShelfGuid,
            FollowersGuids = o.Followers.Select(f => f.Guid).ToArray(),
            FollowingsGuids = o.Followings.Select(f => f.Guid).ToArray(),
            NormalizedUserName = o.NormalizedUserName,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_OwnerDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        List<Library_OwnerDbModel> followers = [];
        if (FollowersGuids.Length > 0)
        {
            followers = await libraryDb.Owners
            .Where(o => FollowersGuids.Contains(o.Guid))
            .ToListAsync();
        }

        List<Library_OwnerDbModel> followings = [];
        if (FollowingsGuids.Length > 0)
        {
            followings = await libraryDb.Owners
            .Where(o => FollowingsGuids.Contains(o.Guid))
            .ToListAsync();
        }

        Library_OwnerDbModel ownerDbModel = new()
        {
            DefaultLibraryGuid = DefaultLibraryGuid,
            DefaultShelfGuid = DefaultShelfGuid,
            Guid = Guid,
            Followers = followers,
            Followings = followings,
            NormalizedUserName = NormalizedUserName,
        };

        return ownerDbModel;
    }
}

public class Library_LibrarySeedModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public bool HasImage { get; set; }
    public string[] InFavorOfGuids { get; set; } = [];

    public static async Task<Library_LibrarySeedModel?> Factory(string dbModel_Guid,
    Library_DbContext libraryDb)
    {
        Library_LibrarySeedModel? seedModel = await libraryDb.Libraries
        .Where(l => l.Guid == dbModel_Guid)
        .Include(l => l.Owner)
        .Include(l => l.InFavorOf)
        .Select(l => new Library_LibrarySeedModel()
        {
            Guid = l.Guid,
            CreatedAt = l.CreatedAt,
            Description = l.Description,
            HasImage = l.HasImage,
            InFavorOfGuids = l.InFavorOf.Select(o => o.Guid).ToArray(),
            OwnerGuid = l.Owner.Guid,
            Title = l.Title,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_LibraryDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == OwnerGuid);
        if (owner is null)
        {
            //log
            Console.WriteLine($"\n     ***** owner Not found with guid '{OwnerGuid}'!");
            return null;
        }

        List<Library_OwnerDbModel> inFavorOf = [];
        if (InFavorOfGuids.Length > 0)
        {
            inFavorOf = await libraryDb.Owners
            .Where(o => InFavorOfGuids.Contains(o.Guid))
            .ToListAsync();
        }

        Library_LibraryDbModel libraryDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Guid = Guid,
            HasImage = HasImage,
            InFavorOf = inFavorOf,
            Owner = owner,
            Title = Title,
        };

        return libraryDbModel;
    }
}

public class Library_ShelfSeedModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public string[] ParentLibrariesGuids { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public bool HasImage { get; set; }
    public string[] InFavorOfGuids { get; set; } = [];

    public static async Task<Library_ShelfSeedModel?> Factory(string dbModel_Guid,
    Library_DbContext libraryDb)
    {
        Library_ShelfSeedModel? seedModel = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == dbModel_Guid)
        .Include(shelf => shelf.Owner)
        .Include(shelf => shelf.InFavorOf)
        .Include(shelf => shelf.ParentLibraries)
        .Select(shelf => new Library_ShelfSeedModel()
        {
            Guid = shelf.Guid,
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            HasImage = shelf.HasImage,
            InFavorOfGuids = shelf.InFavorOf.Select(o => o.Guid).ToArray(),
            OwnerGuid = shelf.Owner.Guid,
            Title = shelf.Title,
            ParentLibrariesGuids = shelf.ParentLibraries.Select(l => l.Guid).ToArray(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_ShelfDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == OwnerGuid);
        if (owner is null)
        {
            //log
            Console.WriteLine($"\n     ***** owner Not found with guid '{OwnerGuid}'!");
            return null;
        }

        List<Library_OwnerDbModel> inFavorOf = [];
        if (InFavorOfGuids.Length > 0)
        {
            inFavorOf = await libraryDb.Owners
            .Where(o => InFavorOfGuids.Contains(o.Guid))
            .ToListAsync();
        }

        List<Library_LibraryDbModel> parentLibraries = [];
        if (ParentLibrariesGuids.Length > 0)
        {
            parentLibraries = await libraryDb.Libraries
            .Where(l => ParentLibrariesGuids.Contains(l.Guid))
            .ToListAsync();
        }

        Library_ShelfDbModel shelfDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Guid = Guid,
            HasImage = HasImage,
            InFavorOf = inFavorOf,
            Owner = owner,
            Title = Title,
            ParentLibraries = parentLibraries,
        };

        return shelfDbModel;
    }
}

public class Library_DocumentSeedModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string[] ParentShelvesGuids { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public bool HasImage { get; set; }
    public string[] InFavorOfGuids { get; set; } = [];

    public static async Task<Library_DocumentSeedModel?> Factory(string dbModel_Guid,
    Library_DbContext libraryDb)
    {
        Library_DocumentSeedModel? seedModel = await libraryDb.Documents
        .Where(doc => doc.Guid == dbModel_Guid)
        .Include(doc => doc.Owner)
        .Include(doc => doc.InFavorOf)
        .Include(doc => doc.ParentShelves)
        .Select(doc => new Library_DocumentSeedModel()
        {
            Guid = doc.Guid,
            CreatedAt = doc.CreatedAt,
            Description = doc.Description,
            HasImage = doc.HasImage,
            InFavorOfGuids = doc.InFavorOf.Select(o => o.Guid).ToArray(),
            OwnerGuid = doc.Owner.Guid,
            Title = doc.Title,
            ParentShelvesGuids = doc.ParentShelves.Select(shelf => shelf.Guid).ToArray(),
            Version = doc.Version,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_DocumentDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == OwnerGuid);
        if (owner is null)
        {
            //log
            Console.WriteLine($"\n     ***** owner Not found with guid '{OwnerGuid}'!");
            return null;
        }

        List<Library_OwnerDbModel> inFavorOf = [];
        if (InFavorOfGuids.Length > 0)
        {
            inFavorOf = await libraryDb.Owners
            .Where(o => InFavorOfGuids.Contains(o.Guid))
            .ToListAsync();
        }

        List<Library_ShelfDbModel> parentShelves = [];
        if (ParentShelvesGuids.Length > 0)
        {
            parentShelves = await libraryDb.Shelves
            .Where(shelf => ParentShelvesGuids.Contains(shelf.Guid))
            .ToListAsync();
        }

        Library_DocumentDbModel documentDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Guid = Guid,
            HasImage = HasImage,
            InFavorOf = inFavorOf,
            Owner = owner,
            Title = Title,
            ParentShelves = parentShelves,
            Version = Version,
        };

        return documentDbModel;
    }
}

public class Library_ElementSeedModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Value { get; set; }
    public string? Title { get; set; }
    public string? FileName { get; set; }
    public int Order { get; set; }
    public string ParentDocumentGuid { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }

    public static async Task<Library_ElementSeedModel?> Factory(string dbModel_Guid,
    Library_DbContext libraryDb)
    {
        Library_ElementSeedModel? seedModel = await libraryDb.Elements
        .Where(el => el.Guid == dbModel_Guid)
        .Include(el => el.Owner)
        .Include(el => el.ParentDocument)
        .Select(el => new Library_ElementSeedModel()
        {
            Guid = el.Guid,
            OwnerGuid = el.Owner.Guid,
            Title = el.Title,
            FileName = el.FileName,
            Order = el.Order,
            ParentDocumentGuid = el.ParentDocument.Guid,
            Type = el.Type,
            UpdatedAt = el.UpdatedAt,
            Value = el.Value,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_ElementDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == OwnerGuid);
        if (owner is null)
        {
            //log
            Console.WriteLine($"\n     ***** owner Not found with guid '{OwnerGuid}'!");
            return null;
        }

        Library_DocumentDbModel? parentDocument = await libraryDb.Documents
        .FirstOrDefaultAsync(doc => doc.Guid == ParentDocumentGuid);
        if (parentDocument is null)
        {
            //log
            Console.WriteLine($"\n     ***** parent document Not found with guid '{ParentDocumentGuid}'!");
            return null;
        }

        Library_ElementDbModel elementDbModel = new()
        {
            Guid = Guid,
            Owner = owner,
            Title = Title,
            ParentDocument = parentDocument,
            FileName = FileName,
            Order = Order,
            Type = Type,
            UpdatedAt = UpdatedAt,
            Value = Value,
        };

        return elementDbModel;
    }
}

public class Library_RelatedVersionsSeedModel
{
    public string Guid { get; set; } = null!;
    public string[] DocumentsGuids { get; set; } = [];

    public static async Task<Library_RelatedVersionsSeedModel?> Factory(string dbModel_Guid,
    Library_DbContext libraryDb)
    {
        Library_RelatedVersionsSeedModel? seedModel = await libraryDb.RelatedVersions
        .Where(rv => rv.Guid == dbModel_Guid)
        .Include(rv => rv.Documents)
        .Select(rv => new Library_RelatedVersionsSeedModel()
        {
            Guid = rv.Guid,
            DocumentsGuids = rv.Documents.Select(doc => doc.Guid).ToArray(),
        })
        //.AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_RelatedVersionsDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        List<Library_DocumentDbModel> documents = [];
        if (DocumentsGuids.Length > 0)
        {
            documents = await libraryDb.Documents
            .Where(doc => DocumentsGuids.Contains(doc.Guid))
            .ToListAsync();
        }

        Library_RelatedVersionsDbModel relatedVersionsDbModel = new()
        {
            Guid = Guid,
            Documents = documents,
        };

        return relatedVersionsDbModel;
    }
}

public class Library_TagSeedModel
{
    public string Name { get; set; } = null!;
    public string[] DocumentsGuids { get; set; } = [];

    public static async Task<Library_TagSeedModel?> Factory(string dbModel_Name,
    Library_DbContext libraryDb)
    {
        Library_TagSeedModel? seedModel = await libraryDb.Tags
        .Where(tag => tag.Name == dbModel_Name)
        .Include(tag => tag.Documents)
        .Select(tag => new Library_TagSeedModel()
        {
            Name = tag.Name,
            DocumentsGuids = tag.Documents.Select(doc => doc.Guid).ToArray(),
        })
        //.AsSplitQuery()
        .FirstOrDefaultAsync();

        return seedModel;
    }

    public async Task<Library_TagDbModel?> GetDbModel(Library_DbContext libraryDb)
    {
        List<Library_DocumentDbModel> documents = [];
        if (DocumentsGuids.Length > 0)
        {
            documents = await libraryDb.Documents
            .Where(doc => DocumentsGuids.Contains(doc.Guid))
            .ToListAsync();
        }

        Library_TagDbModel tagDbModel = new()
        {
            Name = Name,
            Documents = documents,
        };

        return tagDbModel;
    }
}

*/


//********************************************************************************
//************************************ View Models ********************************
public class Library_Owner_ViewModel
{
    public Guid Guid { get; set; }
    public string Username { get; set; } = null!;
    public int IntegrityVersion { get; set; } = 0;
    public bool HasImage { get; set; } = false;
}
public class Library_OwnerProfileStatics_ViewModel
{
    public int NumberOfLibraries { get; set; }
    public int NumberOfShelves { get; set; }
    public int NumberOfDocuments { get; set; }
    public int NumberOfFollowers { get; set; }
    public int NumberOfFollowings { get; set; }
    public int NumberOfFavoriteLibraries { get; set; }
    public int NumberOfFavoriteShelves { get; set; }
    public int NumberOfFavoriteDocuments { get; set; }
}
public class Library_LibraryCard_ViewModel
{
    public Guid Guid { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[] ShelvesTitles { get; set; } = [];
    public bool HasImage { get; set; } = false;
    public int IntegrityVersion { get; set; }
    //public string OwnerUsername { get; set; } = null!;
    public Guid OwnerGuid { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDefault { get; set; }

}
public class Library_ShelfCard_ViewModel
{
    public Guid Guid { get; set; }
    public Guid OwnerGuid { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public Library_LibraryBrief_ViewModel[] Libraries { get; set; } = [];
    public Library_DocumentCard_ViewModel[] DocumentCardModels { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public int TotalNumberOfShelfDocuments { get; set; }
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public bool IsDefault { get; set; }
}
public class Library_DocumentCard_ViewModel
{
    public Guid Guid { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string[] Headers { get; set; } = [];
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public Guid OwnerGuid { get; set; }
    public string? VersionName { get; set; } = null;
}
public class Library_DocumentPage_ViewModel
{
    public Guid Guid { get; set; }
    public Library_OwnerBrief_ViewModel Owner { get; set; } = null!;
    //public Library_LibraryBrief Library { get; set; } = null!;//could be 
    public string Title { get; set; } = null!;
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;
    public Library_VersionBrief_ViewModel[] RelatedVersions { get; set; } = [];
    public Library_ShelfBrief_ViewModel[] Shelves { get; set; } = [];
    public Library_Element_ViewModel[] Elements { get; set; } = [];
    public string[] Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
public class Library_Element_ViewModel
{
    public Guid Guid { get; set; }
    public Guid OwnerGuid { get; set; }
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Title { get; set; }
    public int Order { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class Library_ShelfBrief_ViewModel
{
    public Guid Guid { get; set; }
    public string Title { get; set; } = null!;
    public Library_LibraryBrief_ViewModel[] Libraries { get; set; } = [];
    //public Library_OwnerBrief Owner { get; set; } = null!;
    public Library_DocumentBrief_ViewModel[] Documents { get; set; } = [];
}
public class Library_DocumentBrief_ViewModel
{
    public Guid Guid { get; set; }
    public string Title { get; set; } = null!;
}
public class Library_VersionBrief_ViewModel
{
    public Guid DocumentGuid { get; set; }
    public string VersionName { get; set; } = null!;
}
public class Library_OwnerBrief_ViewModel
{
    public Guid UserGuid { get; set; }
    public string UserName { get; set; } = "_";
}
public class Library_LibraryBrief_ViewModel
{
    public Guid Guid { get; set; }
    public string Title { get; set; } = null!;
}


//************************************ Form Models ********************************
public class Library_NewLibrary_FormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; } = null;

    public IFormFile? Image { get; set; }
}
public class Library_NewShelf_FormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; } = null;

    //[MaxStringArrayLength(100, 32)]
    [MaxLength(100)]
    public Guid[]? LibraryGuids { get; set; } = [];

    public IFormFile? Image { get; set; }
}
public class Library_NewDocument_FormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;

    //[MaxStringArrayLength(100, 32)]
    [MaxLength(100)]
    public Guid[]? ShelfGuids { get; set; } = null;

    public IFormFile? Image { get; set; }
}
public class Library_NewElement_FormModel
{
    [StringLength(10)]
    public string Type { get; set; } = null!;

    [StringLength(1000)]
    public string? Value { get; set; } = null!;

    [StringLength(60, MinimumLength = 3)]
    public string? Title { get; set; }

    public int Order { get; set; }

    //[StringLength(32)]
    public Guid DocumentGuid { get; set; }

    public IFormFile? File { get; set; }
}

public class Library_EditElement_FormModel
{
    //[StringLength(32)]
    public Guid Guid { get; set; }

    [StringLength(1000)]
    public string? Value { get; set; } = null!;

    [StringLength(60, MinimumLength = 3)]
    public string? Title { get; set; }

    public int? Order { get; set; }

    public bool? Delete { get; set; } = false;
}

public class Library_EditIntroduction_FormModel
{
    //[StringLength(32)]
    public Guid Guid { get; set; }

    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;

    public IFormFile? Image { get; set; }
}

public class Library_DocumentParentShelves_FormModel
{
    //[StringLength(32)]
    public Guid DocumentGuid { get; set; }

    //[MaxStringArrayLength(100, 32)]
    [MaxLength(100)]
    public Guid[] ShelfGuids { get; set; } = [];
}
public class Library_ShelfParentLibraries_FormModel
{
    //[StringLength(32)]
    public Guid ShelfGuid { get; set; }

    //[MaxStringArrayLength(100, 32)]
    [MaxLength(100)]
    public Guid[] LibraryGuids { get; set; } = [];
}

public class Library_EditTags_FormModel
{
    //[StringLength(32)]
    public Guid DocumentGuid { get; set; }

    [MaxStringArrayLength(32, 32)]
    [TagCharactersValidator]
    public string[] Tags { get; set; } = [];
}