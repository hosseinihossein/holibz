using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreApp.Validators;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

//********************************************************************************
//************************************ DbModels **********************************
public class Library_LibraryDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public List<Library_ShelfDbModel> Shelves { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Library_ShelfDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public Library_LibraryDbModel Library { get; set; } = null!;
    public List<Library_DocumentDbModel> Documents { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Library_DocumentDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Version { get; set; } = "Default";
    public Library_RelatedVersionsDbModel? RelatedVersions { get; set; }
    public List<Library_ShelfDbModel> Shelves { get; set; } = [];
    public List<Library_ElementDbModel> Elements { get; set; } = [];
    public List<Library_TagDbModel> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
    public string OwnerGuid { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Title { get; set; } = null;
    public int Order { get; set; }
    public Library_DocumentDbModel Document { get; set; } = null!;
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

    public DbSet<Library_LibraryDbModel> Libraries { get; set; } = null!;
    public DbSet<Library_ShelfDbModel> Shelves { get; set; } = null!;
    public DbSet<Library_DocumentDbModel> Documents { get; set; } = null!;
    public DbSet<Library_ElementDbModel> Elements { get; set; } = null!;
    public DbSet<Library_TagDbModel> Tags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //*********** Library-Shelves One-To-Many *********
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasMany<Library_ShelfDbModel>(l => l.Shelves)
        .WithOne(sh => sh.Library)
        .IsRequired(true);

        //*********** Shelves-Documents Many-To-Many *********
        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasMany<Library_DocumentDbModel>(sh => sh.Documents)
        .WithMany(d => d.Shelves);

        //*********** RelatedVerions-Documents One-To-Many *********
        modelBuilder.Entity<Library_RelatedVersionsDbModel>()
        .HasMany<Library_DocumentDbModel>(d => d.Documents)
        .WithOne(rv => rv.RelatedVersions)
        .IsRequired(false);

        //*********** Document-Elements One-To-Many *********
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasMany<Library_ElementDbModel>(d => d.Elements)
        .WithOne(e => e.Document)
        .IsRequired(true);

        //*********** Tags-Documents Many-To-Many *********
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasMany<Library_TagDbModel>(d => d.Tags)
        .WithMany(t => t.Documents);

    }
}

//********************************************************************************
//*********************************** Processes **********************************
public class Library_Process //singleton service
{
    public readonly DirectoryInfo Storage_Library;
    public readonly DirectoryInfo Storage_Shelf;
    public readonly DirectoryInfo Storage_Document;

    public Library_Process(IWebHostEnvironment _env)
    {
        Storage_Library = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library"));
        Storage_Shelf = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Shelf"));
        Storage_Document = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Document"));
    }
    public string? BuildTagName(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            string v1 = value.Trim().Replace(" ", "_").ToUpper();
            string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            string v2 = "";
            foreach (char c in v1)
            {
                if (!allowedChars.Contains(c))
                {
                    v2 = v1.Replace(c.ToString(), "");
                }
            }
            if (v2.Length > 3)
            {
                return v2;
            }
        }
        return null;
    }

    public async Task<ProcessResult> CreateNewLibrary(Library_DbContext libraryDb, string ownerGuid,
    Library_NewLibraryFormModel formModel)
    {
        if (!await libraryDb.Libraries.AnyAsync(lib =>
            lib.OwnerGuid == ownerGuid && lib.Title == formModel.Title))
        {
            Library_LibraryDbModel libraryDbModel;
            if (formModel.Title == "Default Library")
            {
                libraryDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Decription,
                    Guid = "DefaultLibrary",
                };
            }
            else
            {
                libraryDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Decription,
                };
            }

            await libraryDb.Libraries.AddAsync(libraryDbModel);
            await libraryDb.SaveChangesAsync();

            return new ProcessResult() { Success = true, ResultObject = libraryDbModel };
        }
        return new ProcessResult()
        {
            ErrorTitle = "Title Conflict",
            ErrorDescription = $"There's already been a library with title '{formModel.Title}'!"
        };
    }
    public async Task<ProcessResult> CreateNewShelf(Library_DbContext libraryDb, string ownerGuid,
    Library_NewShelfFormModel formModel)
    {
        if (!await libraryDb.Shelves.AnyAsync(shelf =>
            shelf.OwnerGuid == ownerGuid && shelf.Title == formModel.Title))
        {
            Library_LibraryDbModel? libraryContainer =
            await libraryDb.Libraries.FirstOrDefaultAsync(lib => lib.Guid == formModel.LibraryGuid);

            if (libraryContainer is null)
            {
                return new ProcessResult()
                {
                    ErrorTitle = "Library",
                    ErrorDescription = $"There's no library with guid '{formModel.LibraryGuid}'!"
                };
            }

            Library_ShelfDbModel shelfDbModel;
            if (formModel.Title == "Default Shelf")
            {
                shelfDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Decription,
                    Library = libraryContainer,
                    Guid = "DefaultShelf",
                };
            }
            else
            {
                shelfDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Decription,
                    Library = libraryContainer,
                };
            }

            await libraryDb.Shelves.AddAsync(shelfDbModel);
            await libraryDb.SaveChangesAsync();

            return new ProcessResult() { Success = true, ResultObject = shelfDbModel };
        }
        return new ProcessResult()
        {
            ErrorTitle = "Title Conflict",
            ErrorDescription = $"There's already been a shelf with title '{formModel.Title}'!"
        };
    }
    public async Task<ProcessResult> CreateNewDocument(Library_DbContext libraryDb, string ownerGuid,
    Library_NewDocumentFormModel formModel)
    {
        List<Library_ShelfDbModel> shelfDbModels = [];
        if (formModel.ShelfGuids is null || formModel.ShelfGuids.Length == 0)
        {
            bool isThereDefaultShelf = await libraryDb.Shelves
            .AnyAsync(shelf => shelf.OwnerGuid == ownerGuid && shelf.Title == "Default Shelf");
            if (!isThereDefaultShelf)
            {
                await CreateDefaultLibraryAndShelf(libraryDb, ownerGuid);
            }

            Library_ShelfDbModel? defaultShelfDbModel = await libraryDb.Shelves
            .FirstOrDefaultAsync(shelf => shelf.OwnerGuid == ownerGuid && shelf.Title == "Default Shelf");
            if (defaultShelfDbModel is null)
            {
                return new ProcessResult()
                {
                    Success = false,
                    ErrorTitle = "No Shelf",
                    ErrorDescription = "There's No shelf to contain the new document!",
                };
            }
            shelfDbModels.Add(defaultShelfDbModel);
        }
        else
        {
            shelfDbModels = await libraryDb.Shelves
            .Where(shelf => shelf.OwnerGuid == ownerGuid && formModel.ShelfGuids.Contains(shelf.Guid))
            .ToListAsync();
        }

        Library_DocumentDbModel documentDbModel = new()
        {
            Description = formModel.Decription,
            OwnerGuid = ownerGuid,
            Shelves = shelfDbModels,
            Title = formModel.Title,
        };

        await libraryDb.Documents.AddAsync(documentDbModel);
        await libraryDb.SaveChangesAsync();

        if (formModel.Image is not null)
        {
            string documentImagePath = Path.Combine(Storage_Document.FullName, documentDbModel.Guid, "image");
            using (FileStream fs = System.IO.File.Create(documentImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
        }

        return new ProcessResult()
        {
            Success = true,
            ResultObject = documentDbModel,
        };
    }

    public async Task CreateDefaultLibraryAndShelf(Library_DbContext libraryDb,
    string ownerGuid)
    {
        // creating Default library
        Library_NewLibraryFormModel libraryFormModel = new()
        {
            Title = "Default Library",
            Decription = "Containing all shelves that doesn't belong to anyother libraries."
        };
        var createDefaultLibraryResult = await CreateNewLibrary(libraryDb, ownerGuid, libraryFormModel);

        Library_LibraryDbModel? defaultLibrary;
        if (createDefaultLibraryResult.Success &&
        createDefaultLibraryResult.ResultObject is not null)
        {
            defaultLibrary = (Library_LibraryDbModel)createDefaultLibraryResult.ResultObject;
        }
        else
        {
            defaultLibrary = await libraryDb.Libraries.FirstOrDefaultAsync(lib =>
            lib.OwnerGuid == ownerGuid && lib.Title == "Default Library");
        }
        if (defaultLibrary is null)
        {
            //log
            Console.WriteLine($"\n***** /Identity/CreateDefaultLibraryAndShelf, defaultLibrary is null! Couldn't create Default library for '{ownerGuid}'");
        }
        else
        {
            // creating Default shelf in Default library
            Library_NewShelfFormModel shelfFormModel = new()
            {
                Title = "Default Shelf",
                Decription = "Containing all documents that doesn't belong to anyother shelves.",
                LibraryGuid = defaultLibrary.Guid,
            };
            var createDefaultShelfResult = await CreateNewShelf(libraryDb, ownerGuid, shelfFormModel);
            if (!createDefaultShelfResult.Success)
            {
                //log
                Console.WriteLine($"\n***** /Identity/CreateDefaultLibraryAndShelf, Couldn't create Default shelf for '{ownerGuid}'!");
            }
        }
    }


}
public class ProcessResult
{
    public bool Success { get; set; } = false;
    public string? ErrorTitle { get; set; } = null;
    public string? ErrorDescription { get; set; } = null;
    public object? ResultObject { get; set; } = null;
}


//********************************************************************************
//************************************ DataModels ********************************
public class Library_LibraryCardModel
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[] ShelvesTitles { get; set; } = [];
    public bool HasImage { get; set; } = false;
    //public string OwnerUsername { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
public class Library_ShelfCardModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public string LibraryTitle { get; set; } = null!;
    public Library_DocumentCardModel[] DocumentCardModels { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public int TotalNumberOfShelfDocuments { get; set; }
}
public class Library_DocumentCardModel
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string[] Headers { get; set; } = [];
    public bool HasImage { get; set; }
    public string OwnerGuid { get; set; } = null!;
}
public class Library_DocumentPageModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool HasImage { get; set; }
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;
    public Library_VersionBrief? RelatedVersions { get; set; }
    public Library_ShelfBrief[] Shelves { get; set; } = [];
    public Library_DocumentElementModel[] Elements { get; set; } = [];
    public string[] Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
public class Library_DocumentElementModel
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
    public string? Description { get; set; }
    public Library_DocumentBrief[] Documents { get; set; } = [];
}
public class Library_DocumentBrief
{
    public string Guid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
public class Library_VersionBrief
{
    public string DocumentGuid { get; set; } = null!;
    public string VersionName { get; set; } = null!;
}

public class Library_NewLibraryFormModel
{
    [StringLength(30, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(200)]
    public string? Decription { get; set; } = null;
}
public class Library_NewShelfFormModel
{
    [StringLength(30, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(200)]
    public string? Decription { get; set; } = null;

    [StringLength(32)]
    public string LibraryGuid { get; set; } = null!;
}
public class Library_NewDocumentFormModel
{
    [StringLength(30, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500, MinimumLength = 5)]
    public string Decription { get; set; } = null!;

    [MaxArrayLength(10)]
    [StringLength(32)]
    public string[]? ShelfGuids { get; set; } = null;

    public IFormFile? Image { get; set; }
}
