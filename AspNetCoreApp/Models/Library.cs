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
    public int Id { get; set; }
    public string Guid { get; set; } = null!;
    public List<Library_LibraryDbModel> Libraries { get; set; } = [];
    public List<Library_ShelfDbModel> Shelves { get; set; } = [];
    public List<Library_DocumentDbModel> Documents { get; set; } = [];
    public List<Library_ElementDbModel> Elements { get; set; } = [];
    public string DefaultLibraryGuid { get; set; } = null!;
    public string DefaultShelfGuid { get; set; } = null!;
    public List<Library_OwnerDbModel> Followers { get; set; } = [];
    public List<Library_OwnerDbModel> Followings { get; set; } = [];
    public List<Library_LibraryDbModel> FavoriteLibraries { get; set; } = [];
    public List<Library_ShelfDbModel> FavoriteShelves { get; set; } = [];
    public List<Library_DocumentDbModel> FavoriteDocuments { get; set; } = [];
}
public class Library_LibraryDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Library_OwnerDbModel Owner { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public List<Library_ShelfDbModel> Shelves { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public bool HasImage { get; set; } = false;
    public List<Library_OwnerDbModel> InFavorOf { get; set; } = [];
}
public class Library_ShelfDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Library_OwnerDbModel Owner { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public List<Library_LibraryDbModel> ParentLibraries { get; set; } = [];
    public List<Library_DocumentDbModel> Documents { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public bool HasImage { get; set; } = false;
    public List<Library_OwnerDbModel> InFavorOf { get; set; } = [];
}
public class Library_DocumentDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Library_OwnerDbModel Owner { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Version { get; set; } = "Default";
    public Library_RelatedVersionsDbModel? RelatedVersions { get; set; }
    public List<Library_ShelfDbModel> ParentShelves { get; set; } = [];
    public List<Library_ElementDbModel> Elements { get; set; } = [];
    public List<Library_TagDbModel> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    public bool HasImage { get; set; } = false;
    public List<Library_OwnerDbModel> InFavorOf { get; set; } = [];
}
public class Library_RelatedVersionsDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public List<Library_DocumentDbModel> Documents { get; set; } = [];
}
public class Library_ElementDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public Library_OwnerDbModel Owner { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Value { get; set; } = null;
    public string? Title { get; set; } = null;
    public string? FileName { get; set; } = null;
    public int Order { get; set; }
    public Library_DocumentDbModel ParentDocument { get; set; } = null!;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
public class Library_TagDbModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Library_DocumentDbModel> Documents { get; set; } = [];
}


public class Library_DbContext : DbContext
{
    public Library_DbContext(DbContextOptions<Library_DbContext> options) : base(options) { }

    public DbSet<Library_OwnerDbModel> Owners { get; set; } = null!;
    public DbSet<Library_LibraryDbModel> Libraries { get; set; } = null!;
    public DbSet<Library_ShelfDbModel> Shelves { get; set; } = null!;
    public DbSet<Library_DocumentDbModel> Documents { get; set; } = null!;
    public DbSet<Library_RelatedVersionsDbModel> RelatedVersions { get; set; } = null!;
    public DbSet<Library_ElementDbModel> Elements { get; set; } = null!;
    public DbSet<Library_TagDbModel> Tags { get; set; } = null!;

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
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.Followers)
        .WithMany(o => o.Followings);

        //*********** Users-FavoriteLibraries Many-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.FavoriteLibraries)
        .WithMany(l => l.InFavorOf);

        //*********** Users-FavoriteShelves Many-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.FavoriteShelves)
        .WithMany(sh => sh.InFavorOf);

        //*********** Users-FavoriteDocuments Many-To-Many *********
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasMany(o => o.FavoriteDocuments)
        .WithMany(doc => doc.InFavorOf);


        //********************************** Library ***********************************
        //*********** Libraries-Shelves Many-To-Many *********
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasMany(l => l.Shelves)
        .WithMany(sh => sh.ParentLibraries);


        //********************************** Shelf ***********************************
        //*********** Shelves-Documents Many-To-Many *********
        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasMany(sh => sh.Documents)
        .WithMany(d => d.ParentShelves);


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
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasMany(d => d.Tags)
        .WithMany(t => t.Documents);


        //***************************************************************************
        //*************************** Index Columns *********************************
        modelBuilder.Entity<Library_OwnerDbModel>()
        .HasIndex(o => o.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasIndex(lib => lib.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasIndex(shelf => shelf.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasIndex(doc => doc.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Library_RelatedVersionsDbModel>()
        .HasIndex(rv => rv.Guid)
        .IsUnique(true);

        modelBuilder.Entity<Library_ElementDbModel>()
        .HasIndex(el => el.Guid)
        .IsUnique(true);

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
    readonly DirectoryInfo Storage_Tags;
    readonly DirectoryInfo Storage_RelatedVersions;
    readonly FileNameValidator fileNameValidator;
    readonly string SeedFileName;

    public Library_Process(IWebHostEnvironment _env, FileNameValidator _fileNameValidator,
    IConfiguration config)
    {
        SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
        Storage_Owners = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Owners"));
        Storage_Libraries = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Libraries"));
        Storage_Shelves = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Shelves"));
        Storage_Documents = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Documents"));
        Storage_Elements = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Elements"));
        Storage_Tags = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "Tags"));
        Storage_RelatedVersions = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library", "RelatedVersions"));
        fileNameValidator = _fileNameValidator;
    }
    public string? BuildTagName(string value)
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
    }

    public async Task<Library_ProcessResult> CreateNewOwner(Library_DbContext libraryDb, string ownerGuid)
    {
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == ownerGuid);
        if (ownerDbModel is null)
        {
            Library_ShelfDbModel defaultShelf = new()
            {
                Description = "Containing all documents that doesn't belong to anyother shelves.",
                Title = "Default Shelf",
            };
            Library_LibraryDbModel defaultLibrary = new()
            {
                Description = "Containing all shelves that doesn't belong to anyother libraries.",
                Title = "Default Library",
                Shelves = [defaultShelf],
            };
            ownerDbModel = new()
            {
                Guid = ownerGuid,
                DefaultLibraryGuid = defaultLibrary.Guid,
                DefaultShelfGuid = defaultShelf.Guid,
                Libraries = [defaultLibrary],
                Shelves = [defaultShelf],
            };

            await libraryDb.Owners.AddAsync(ownerDbModel);
            await libraryDb.SaveChangesAsync();

            //seed
            await Update_OwnerSeed(ownerDbModel.Guid, libraryDb);
        }

        return new Library_ProcessResult()
        {
            Success = true,
            ResultObject = ownerDbModel,
        };
    }
    public async Task<Library_ProcessResult> CreateNewLibrary(Library_DbContext libraryDb, string ownerGuid,
    Library_NewLibraryFormModel formModel)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == ownerGuid);
        if (owner is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid}'!",
                Success = false,
            };
            return processResult;
        }

        Library_LibraryDbModel libraryDbModel = new()
        {
            Title = formModel.Title,
            Owner = owner,
            Description = formModel.Description,
        };

        if (formModel.Image is not null)
        {
            DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
            string libraryImagePath = Path.Combine(libraryDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(libraryImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
            libraryDbModel.HasImage = true;
        }

        await libraryDb.Libraries.AddAsync(libraryDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        await Update_LibrarySeed(libraryDbModel.Guid, libraryDb);

        return new Library_ProcessResult() { Success = true, ResultObject = libraryDbModel };
    }
    public async Task<Library_ProcessResult> CreateNewShelf(Library_DbContext libraryDb, string ownerGuid,
    Library_NewShelfFormModel formModel)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .Include(o => o.Libraries)
        .FirstOrDefaultAsync(o => o.Guid == ownerGuid);
        if (owner is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid}'!",
                Success = false,
            };
            return processResult;
        }

        List<Library_LibraryDbModel> parentLibraries = [];
        if (formModel.LibraryGuids is not null && formModel.LibraryGuids.Length > 0)
        {
            parentLibraries = owner.Libraries
            .Where(lib => formModel.LibraryGuids.Contains(lib.Guid))
            .ToList();
        }

        if (parentLibraries.Count == 0)
        {
            var defaultLibrary = owner.Libraries.FirstOrDefault(lib => lib.Guid == owner.DefaultLibraryGuid)!;
            parentLibraries.Add(defaultLibrary);
        }

        Library_ShelfDbModel shelfDbModel = new()
        {
            Title = formModel.Title,
            Owner = owner,
            Description = formModel.Description,
            ParentLibraries = parentLibraries,
        };

        if (formModel.Image is not null)
        {
            DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
            string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(shelfImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
            shelfDbModel.HasImage = true;
        }

        await libraryDb.Shelves.AddAsync(shelfDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        await Update_ShelfSeed(shelfDbModel.Guid, libraryDb);

        return new Library_ProcessResult() { Success = true, ResultObject = shelfDbModel };
    }
    public async Task<Library_ProcessResult> CreateNewDocument(Library_DbContext libraryDb, string ownerGuid,
    Library_NewDocumentFormModel formModel)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .Include(o => o.Shelves)
        .FirstOrDefaultAsync(o => o.Guid == ownerGuid);
        if (owner is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid}'!",
                Success = false,
            };
            return processResult;
        }

        List<Library_ShelfDbModel> parentShelfDbModels = [];
        if (formModel.ShelfGuids is not null && formModel.ShelfGuids.Length > 0)
        {
            parentShelfDbModels = owner.Shelves
            .Where(shelf => formModel.ShelfGuids.Contains(shelf.Guid))
            .ToList();
        }

        if (parentShelfDbModels.Count == 0)
        {
            var defaultShelf = owner.Shelves.FirstOrDefault(shelf => shelf.Guid == owner.DefaultShelfGuid)!;
            parentShelfDbModels.Add(defaultShelf);
        }

        Library_DocumentDbModel documentDbModel = new()
        {
            Description = formModel.Description,
            Owner = owner,
            ParentShelves = parentShelfDbModels,
            Title = formModel.Title,
        };

        if (formModel.Image is not null)
        {
            DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, documentDbModel.Guid));
            string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(documentImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
            documentDbModel.HasImage = true;
        }

        await libraryDb.Documents.AddAsync(documentDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        await Update_DocumentSeed(documentDbModel.Guid, libraryDb);

        return new Library_ProcessResult()
        {
            Success = true,
            ResultObject = documentDbModel,
        };
    }
    public async Task<Library_ProcessResult> CreateNewElement(Library_DbContext libraryDb, string ownerGuid,
    Library_NewElementFormModel formModel)
    {
        Library_OwnerDbModel? owner = await libraryDb.Owners
        .FirstOrDefaultAsync(o => o.Guid == ownerGuid);
        if (owner is null)
        {
            Library_ProcessResult processResult = new()
            {
                ErrorTitle = "OwnerGuid",
                ErrorDescription = $"Couldn't find any owner with guid '{ownerGuid}'!",
                Success = false,
            };
            return processResult;
        }

        Library_DocumentDbModel? documentDbmodel = await libraryDb.Documents
        .FirstOrDefaultAsync(doc => doc.Guid == formModel.DocumentGuid);
        if (documentDbmodel is null)
        {
            return new Library_ProcessResult()
            {
                Success = false,
                ErrorTitle = "Parent Document",
                ErrorDescription = $"Theres no document with guid '{formModel.DocumentGuid}'!",
            };
        }

        if (formModel.Type == "h1" || formModel.Type == "h2" || formModel.Type == "p" ||
        formModel.Type == "code" || formModel.Type == "link")
        {
            Library_ElementDbModel elementDbmodel = new()
            {
                ParentDocument = documentDbmodel,
                Order = formModel.Order,
                Owner = owner,
                Title = formModel.Title,
                Type = formModel.Type,
                Value = formModel.Value,
            };

            await libraryDb.Elements.AddAsync(elementDbmodel);
            await libraryDb.SaveChangesAsync();

            //reorder elements
            await ReorderElements(libraryDb, documentDbmodel.Guid);

            //seed
            await Update_ElementSeed(elementDbmodel.Guid, libraryDb);

            return new Library_ProcessResult()
            {
                Success = true,
                ResultObject = elementDbmodel,
            };
        }

        if ((formModel.Type == "img" || formModel.Type == "file") && formModel.File is not null)
        {
            //get a valid file name
            string validFileName = fileNameValidator.GetValidFileName(WebUtility.HtmlEncode(formModel.File.FileName));

            Library_ElementDbModel elementDbmodel = new()
            {
                ParentDocument = documentDbmodel,
                Order = formModel.Order,
                Owner = owner,
                Title = formModel.Title,
                Type = formModel.Type,
                FileName = validFileName,
            };

            await libraryDb.Elements.AddAsync(elementDbmodel);
            await libraryDb.SaveChangesAsync();

            DirectoryInfo elementDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Elements.FullName, elementDbmodel.Guid));
            string elementFilePath = Path.Combine(elementDirectoryInfo.FullName, validFileName);
            using (FileStream fs = System.IO.File.Create(elementFilePath))
            {
                await formModel.File.CopyToAsync(fs);
            }

            //reorder elements
            await ReorderElements(libraryDb, documentDbmodel.Guid);

            //seed
            await Update_ElementSeed(elementDbmodel.Guid, libraryDb);

            return new Library_ProcessResult()
            {
                Success = true,
                ResultObject = elementDbmodel,
            };
        }

        return new Library_ProcessResult()
        {
            Success = false,
            ErrorTitle = "Element Type",
            ErrorDescription = $"The element Type is unknown! Element type: '{formModel.Type}'",
        };
    }
    public async Task<Library_ProcessResult> ReorderElements(Library_DbContext libraryDb, string parentDocumentGuid)
    {
        List<Library_ElementDbModel> elementsToReorder = (await libraryDb.Documents
        .Include(doc => doc.Elements)
        .Where(doc => doc.Guid == parentDocumentGuid)
        .Select(doc => doc.Elements)
        .FirstOrDefaultAsync())!
        .OrderBy(el => el.Order)
        .ToList();

        for (int i = 0; i < elementsToReorder.Count; i++)
        {
            elementsToReorder[i].Order = i;
        }

        await libraryDb.SaveChangesAsync();

        return new Library_ProcessResult()
        {
            Success = true,
            ResultObject = elementsToReorder,
        };
    }



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

}
public class Library_ProcessResult
{
    public bool Success { get; set; } = false;
    public string? ErrorTitle { get; set; } = null;
    public string? ErrorDescription { get; set; } = null;
    public object? ResultObject { get; set; } = null;
}

//************************************ Seed Models ********************************

public class Library_OwnerSeedModel
{
    public string Guid { get; set; } = null!;
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




//********************************************************************************
//************************************ Data Models ********************************
public class Library_LibraryCardModel
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[] ShelvesTitles { get; set; } = [];
    public bool HasImage { get; set; } = false;
    public int IntegrityVersion { get; set; }
    //public string OwnerUsername { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsDefault { get; set; }

}
public class Library_ShelfCardModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public Library_LibraryBrief[] Libraries { get; set; } = [];
    public Library_DocumentCardModel[] DocumentCardModels { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public int TotalNumberOfShelfDocuments { get; set; }
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public bool IsDefault { get; set; }
}
public class Library_DocumentCardModel
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string[] Headers { get; set; } = [];
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public string OwnerGuid { get; set; } = null!;
    public string? VersionName { get; set; } = null;
}
public class Library_DocumentPageModel
{
    public string Guid { get; set; } = null!;
    public Library_OwnerBrief Owner { get; set; } = null!;
    //public Library_LibraryBrief Library { get; set; } = null!;//could be 
    public string Title { get; set; } = null!;
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;
    public Library_VersionBrief[] RelatedVersions { get; set; } = [];
    public Library_ShelfBrief[] Shelves { get; set; } = [];
    public Library_ElementModel[] Elements { get; set; } = [];
    public string[] Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
public class Library_ElementModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Title { get; set; }
    public int Order { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class Library_ShelfBrief
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public Library_LibraryBrief[] Libraries { get; set; } = [];
    //public Library_OwnerBrief Owner { get; set; } = null!;
    public Library_DocumentBrief[] Documents { get; set; } = [];
}
public class Library_DocumentBrief
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
}
public class Library_VersionBrief
{
    public string DocumentGuid { get; set; } = null!;
    public string VersionName { get; set; } = null!;
}
public class Library_OwnerBrief
{
    public string UserGuid { get; set; } = null!;
    public string UserName { get; set; } = "_";
}
public class Library_LibraryBrief
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
}

public class Library_NewLibraryFormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; } = null;

    public IFormFile? Image { get; set; }
}
public class Library_NewShelfFormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; } = null;

    [MaxStringArrayLength(100, 32)]
    public string[]? LibraryGuids { get; set; } = [];

    public IFormFile? Image { get; set; }
}
public class Library_NewDocumentFormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;

    [MaxStringArrayLength(100, 32)]
    public string[]? ShelfGuids { get; set; } = null;

    public IFormFile? Image { get; set; }
}
public class Library_NewElementFormModel
{
    [StringLength(10)]
    public string Type { get; set; } = null!;

    [StringLength(1000)]
    public string? Value { get; set; } = null!;

    [StringLength(60, MinimumLength = 3)]
    public string? Title { get; set; }

    public int Order { get; set; }

    [StringLength(32)]
    public string DocumentGuid { get; set; } = null!;

    public IFormFile? File { get; set; }
}

public class Library_EditElementFormModel
{
    [StringLength(32)]
    public string Guid { get; set; } = null!;

    [StringLength(1000)]
    public string? Value { get; set; } = null!;

    [StringLength(60, MinimumLength = 3)]
    public string? Title { get; set; }

    public int? Order { get; set; }

    public bool? Delete { get; set; } = false;
}

public class Library_EditIntroductionFormModel
{
    [StringLength(32)]
    public string Guid { get; set; } = null!;

    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;

    public IFormFile? Image { get; set; }
}

public class Library_DocumentParentShelvesFormModel
{
    [StringLength(32)]
    public string DocumentGuid { get; set; } = null!;

    [MaxStringArrayLength(100, 32)]
    public string[] ShelfGuids { get; set; } = [];
}
public class Library_ShelfParentLibrariesFormModel
{
    [StringLength(32)]
    public string ShelfGuid { get; set; } = null!;

    [MaxStringArrayLength(100, 32)]
    public string[] LibraryGuids { get; set; } = [];
}

public class Library_OwnerModel
{
    public string Guid { get; set; } = null!;
    public string Username { get; set; } = null!;
    public int IntegrityVersion { get; set; } = 0;
    public bool HasImage { get; set; } = false;
}

public class Library_FavoriteModel
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool HasImage { get; set; } = false;
    public int IntegrityVersion { get; set; }
    public Library_OwnerModel Owner { get; set; } = null!;
}

public class Library_EditTagsFormModel
{
    [StringLength(32)]
    public string DocumentGuid { get; set; } = null!;

    [MaxStringArrayLength(32, 32)]
    [TagCharactersValidator]
    public string[] Tags { get; set; } = [];
}