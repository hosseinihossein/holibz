using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Encodings.Web;
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
    public List<Library_LibraryDbModel> Libraries { get; set; } = [];
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
    public string? Value { get; set; } = null;
    public string? Title { get; set; } = null;
    public string? FileName { get; set; } = null;
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
    public DbSet<Library_RelatedVersionsDbModel> RelatedVersions { get; set; } = null!;
    public DbSet<Library_ElementDbModel> Elements { get; set; } = null!;
    public DbSet<Library_TagDbModel> Tags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //*********** Libraries-Shelves Many-To-Many *********
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasMany<Library_ShelfDbModel>(l => l.Shelves)
        .WithMany(sh => sh.Libraries);
        //.IsRequired(true);
        //.OnDelete(DeleteBehavior.Cascade);

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
    public readonly DirectoryInfo Storage_Element;
    public readonly FileNameValidator fileNameValidator;

    public Library_Process(IWebHostEnvironment _env, FileNameValidator _fileNameValidator)
    {
        Storage_Library = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library"));
        Storage_Shelf = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Shelf"));
        Storage_Document = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Document"));
        Storage_Element = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Element"));
        fileNameValidator = _fileNameValidator;
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
                    Description = formModel.Description,
                    Guid = "DefaultLibrary",
                };
            }
            else
            {
                libraryDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Description,
                };
            }

            await libraryDb.Libraries.AddAsync(libraryDbModel);
            await libraryDb.SaveChangesAsync();

            if (formModel.Image is not null)
            {
                DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Library.FullName, libraryDbModel.Guid));
                string libraryImagePath = Path.Combine(libraryDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(libraryImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }
            }

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
            List<Library_LibraryDbModel> parentLibraries = await libraryDb.Libraries
            .Where(lib => formModel.LibraryGuids.Contains(lib.Guid))
            .ToListAsync();

            Library_ShelfDbModel shelfDbModel;
            if (formModel.Title == "Default Shelf")
            {
                shelfDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Description,
                    Guid = "DefaultShelf",
                };
            }
            else
            {
                shelfDbModel = new()
                {
                    Title = formModel.Title,
                    OwnerGuid = ownerGuid,
                    Description = formModel.Description,
                };
            }

            shelfDbModel.Libraries = parentLibraries;

            await libraryDb.Shelves.AddAsync(shelfDbModel);
            await libraryDb.SaveChangesAsync();

            if (formModel.Image is not null)
            {
                DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelf.FullName, shelfDbModel.Guid));
                string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(shelfImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }
            }

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
            .AnyAsync(shelf => shelf.OwnerGuid == ownerGuid && shelf.Guid == "DefaultShelf");
            if (!isThereDefaultShelf)
            {
                await CreateDefaultLibraryAndShelf(libraryDb, ownerGuid);
            }

            Library_ShelfDbModel? defaultShelfDbModel = await libraryDb.Shelves
            .FirstOrDefaultAsync(shelf => shelf.OwnerGuid == ownerGuid && shelf.Guid == "DefaultShelf");
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
            Description = formModel.Description,
            OwnerGuid = ownerGuid,
            Shelves = shelfDbModels,
            Title = formModel.Title,
        };

        await libraryDb.Documents.AddAsync(documentDbModel);
        await libraryDb.SaveChangesAsync();

        if (formModel.Image is not null)
        {
            DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Document.FullName, documentDbModel.Guid));
            string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
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
    public async Task<ProcessResult> CreateNewElement(Library_DbContext libraryDb, string ownerGuid,
    Library_NewElementFormModel formModel)
    {
        Library_DocumentDbModel? documentDbmodel = await libraryDb.Documents
        .FirstOrDefaultAsync(doc => doc.Guid == formModel.DocumentGuid);
        if (documentDbmodel is null)
        {
            return new ProcessResult()
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
                Document = documentDbmodel,
                Order = formModel.Order,
                OwnerGuid = ownerGuid,
                Title = formModel.Title,
                Type = formModel.Type,
                Value = formModel.Value,
            };

            await libraryDb.Elements.AddAsync(elementDbmodel);
            await libraryDb.SaveChangesAsync();

            return new ProcessResult()
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
                Document = documentDbmodel,
                Order = formModel.Order,
                OwnerGuid = ownerGuid,
                Title = formModel.Title,
                Type = formModel.Type,
                FileName = validFileName,
            };

            await libraryDb.Elements.AddAsync(elementDbmodel);
            await libraryDb.SaveChangesAsync();

            DirectoryInfo elementDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Element.FullName, elementDbmodel.Guid));
            string elementFilePath = Path.Combine(elementDirectoryInfo.FullName, validFileName);
            using (FileStream fs = System.IO.File.Create(elementFilePath))
            {
                await formModel.File.CopyToAsync(fs);
            }

            return new ProcessResult()
            {
                Success = true,
                ResultObject = elementDbmodel,
            };
        }

        return new ProcessResult()
        {
            Success = false,
            ErrorTitle = "Element Type",
            ErrorDescription = $"The element Type is unknown! Element type: '{formModel.Type}'",
        };
    }

    public async Task CreateDefaultLibraryAndShelf(Library_DbContext libraryDb,
    string ownerGuid)
    {
        // creating Default library
        Library_NewLibraryFormModel defaultLibraryFormModel = new()
        {
            Title = "Default Library",
            Description = "Containing all shelves that doesn't belong to anyother libraries."
        };
        var createDefaultLibraryResult = await CreateNewLibrary(libraryDb, ownerGuid, defaultLibraryFormModel);

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
            Library_NewShelfFormModel defaultShelfFormModel = new()
            {
                Title = "Default Shelf",
                Description = "Containing all documents that doesn't belong to anyother shelves.",
                LibraryGuids = [defaultLibrary.Guid],
            };
            var createDefaultShelfResult = await CreateNewShelf(libraryDb, ownerGuid, defaultShelfFormModel);
            /*if (!createDefaultShelfResult.Success)
            {
                //log
                Console.WriteLine($"\n***** {createDefaultShelfResult.ErrorTitle}: {createDefaultShelfResult.ErrorDescription}");
            }*/
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
    public Library_LibraryBrief[] Libraries { get; set; } = [];
    public Library_DocumentCardModel[] DocumentCardModels { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public int TotalNumberOfShelfDocuments { get; set; }
    public bool HasImage { get; set; }
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
    public Library_OwnerBrief Owner { get; set; } = null!;
    //public Library_LibraryBrief Library { get; set; } = null!;//could be 
    public string Title { get; set; } = null!;
    public bool HasImage { get; set; }
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
    public string[] LibraryGuids { get; set; } = [];

    public IFormFile? Image { get; set; }
}
public class Library_NewDocumentFormModel
{
    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;

    //[MaxArrayLength(10)]
    //[StringLength(32)]
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

