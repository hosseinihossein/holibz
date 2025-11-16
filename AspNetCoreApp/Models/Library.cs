using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
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
        //*************************** Relationships *********************************
        //*********** Libraries-Shelves Many-To-Many *********
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasMany<Library_ShelfDbModel>(l => l.Shelves)
        .WithMany(sh => sh.Libraries);

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
        //.OnDelete(DeleteBehavior.Cascade);//default for required entities

        //*********** Tags-Documents Many-To-Many *********
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasMany<Library_TagDbModel>(d => d.Tags)
        .WithMany(t => t.Documents);

        //*************************** Index Columns *********************************
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasIndex(lib => lib.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_LibraryDbModel>()
        .HasIndex(lib => lib.OwnerGuid);

        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasIndex(shelf => shelf.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_ShelfDbModel>()
        .HasIndex(shelf => shelf.OwnerGuid);

        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasIndex(doc => doc.Guid)
        .IsUnique(true);
        modelBuilder.Entity<Library_DocumentDbModel>()
        .HasIndex(doc => doc.OwnerGuid);

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
    public readonly DirectoryInfo Storage_Libraries;
    public readonly DirectoryInfo Storage_Shelves;
    public readonly DirectoryInfo Storage_Documents;
    public readonly DirectoryInfo Storage_Elements;
    readonly DirectoryInfo Storage_Tags;
    readonly DirectoryInfo Storage_RelatedVersions;
    readonly FileNameValidator fileNameValidator;

    public Library_Process(IWebHostEnvironment _env, FileNameValidator _fileNameValidator)
    {
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

    public async Task<Library_ProcessResult> CreateNewLibrary(Library_DbContext libraryDb, string ownerGuid,
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
                DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
                string libraryImagePath = Path.Combine(libraryDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(libraryImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }
            }

            // seed
            //_ = Update_LibrarySeed(libraryDbModel);

            return new Library_ProcessResult() { Success = true, ResultObject = libraryDbModel };
        }
        return new Library_ProcessResult()
        {
            ErrorTitle = "Title Conflict",
            ErrorDescription = $"There's already been a library with title '{formModel.Title}'!"
        };
    }
    public async Task<Library_ProcessResult> CreateNewShelf(Library_DbContext libraryDb, string ownerGuid,
    Library_NewShelfFormModel formModel)
    {
        Library_ShelfDbModel shelfDbModel;
        if (formModel.Title == "Default Shelf")
        {
            if (await libraryDb.Shelves.Include(shelf => shelf.Libraries).AnyAsync(shelf =>
            shelf.OwnerGuid == ownerGuid && shelf.Guid == "DefaultShelf"))
            {
                return new Library_ProcessResult()
                {
                    ErrorTitle = "Default Shelf",
                    ErrorDescription = $"There's already been a default shelf!"
                };
            }

            var defaultLibrary = await libraryDb.Libraries
            .FirstOrDefaultAsync(lib => lib.OwnerGuid == ownerGuid && lib.Guid == "DefaultLibrary");
            if (defaultLibrary is null)
            {
                // creating Default library
                var createDefaultLibraryResult = await CreateDefaultLibrary(libraryDb, ownerGuid);

                if (createDefaultLibraryResult.Success &&
                createDefaultLibraryResult.ResultObject is not null)
                {
                    defaultLibrary = (Library_LibraryDbModel)createDefaultLibraryResult.ResultObject;
                }
                else
                {
                    //log
                    Console.WriteLine($"\n***** Cloudnt find and create default library for the user with guid '{ownerGuid}'!");
                    return createDefaultLibraryResult;
                }
            }

            shelfDbModel = new()
            {
                Title = formModel.Title,
                OwnerGuid = ownerGuid,
                Description = formModel.Description,
                Guid = "DefaultShelf",
                Libraries = [defaultLibrary],
            };
        }
        else
        {
            List<Library_LibraryDbModel> parentLibraries = [];
            if (formModel.LibraryGuids.Length > 0)
            {
                parentLibraries = await libraryDb.Libraries
                .Where(lib => formModel.LibraryGuids.Contains(lib.Guid))
                .ToListAsync();
            }

            /*if (parentLibraries is null || parentLibraries.Count == 0)
            {
                var defaultLibrary = await libraryDb.Libraries
                .FirstOrDefaultAsync(lib => lib.OwnerGuid == ownerGuid && lib.Guid == "DefaultLibrary");
                if (defaultLibrary is null)
                {
                    // creating Default library
                    var createDefaultLibraryResult = await CreateDefaultLibrary(libraryDb, ownerGuid);

                    if (createDefaultLibraryResult.Success &&
                    createDefaultLibraryResult.ResultObject is not null)
                    {
                        defaultLibrary = (Library_LibraryDbModel)createDefaultLibraryResult.ResultObject;
                    }
                    else
                    {
                        //log
                        Console.WriteLine($"\n***** Cloudnt find and create default library for the user with guid '{ownerGuid}'!");
                        return createDefaultLibraryResult;
                    }
                }
                parentLibraries = [defaultLibrary];
            }*/

            shelfDbModel = new()
            {
                Title = formModel.Title,
                OwnerGuid = ownerGuid,
                Description = formModel.Description,
                Libraries = parentLibraries,
            };
        }

        await libraryDb.Shelves.AddAsync(shelfDbModel);
        await libraryDb.SaveChangesAsync();

        if (formModel.Image is not null)
        {
            DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
            string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(shelfImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
        }

        return new Library_ProcessResult() { Success = true, ResultObject = shelfDbModel };
    }
    public async Task<Library_ProcessResult> CreateNewDocument(Library_DbContext libraryDb, string ownerGuid,
    Library_NewDocumentFormModel formModel)
    {
        List<Library_ShelfDbModel> shelfDbModels = [];
        if (formModel.ShelfGuids is null || formModel.ShelfGuids.Length == 0)
        {
            bool isThereDefaultShelf = await libraryDb.Shelves
            .AnyAsync(shelf => shelf.OwnerGuid == ownerGuid && shelf.Guid == "DefaultShelf");
            if (!isThereDefaultShelf)
            {
                await CreateDefaultShelf(libraryDb, ownerGuid);
            }

            Library_ShelfDbModel? defaultShelfDbModel = await libraryDb.Shelves
            .FirstOrDefaultAsync(shelf => shelf.OwnerGuid == ownerGuid && shelf.Guid == "DefaultShelf");
            if (defaultShelfDbModel is null)
            {
                return new Library_ProcessResult()
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
            DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, documentDbModel.Guid));
            string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
            using (FileStream fs = System.IO.File.Create(documentImagePath))
            {
                await formModel.Image.CopyToAsync(fs);
            }
        }

        // seed
        //_ = Update_DocumentSeed(documentDbModel);

        return new Library_ProcessResult()
        {
            Success = true,
            ResultObject = documentDbModel,
        };
    }
    public async Task<Library_ProcessResult> CreateNewElement(Library_DbContext libraryDb, string ownerGuid,
    Library_NewElementFormModel formModel)
    {
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
                Document = documentDbmodel,
                Order = formModel.Order,
                OwnerGuid = ownerGuid,
                Title = formModel.Title,
                Type = formModel.Type,
                Value = formModel.Value,
            };

            await libraryDb.Elements.AddAsync(elementDbmodel);
            await libraryDb.SaveChangesAsync();

            // seed
            //_ = Update_ElementSeed(elementDbmodel);

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
                Document = documentDbmodel,
                Order = formModel.Order,
                OwnerGuid = ownerGuid,
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

            // seed
            //_ = Update_ElementSeed(elementDbmodel);

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


    public async Task<Library_ProcessResult> CreateDefaultLibrary(Library_DbContext libraryDb, string ownerGuid)
    {
        // creating Default library
        Library_NewLibraryFormModel defaultLibraryFormModel = new()
        {
            Title = "Default Library",
            Description = "Containing all shelves that doesn't belong to anyother libraries."
        };
        return await CreateNewLibrary(libraryDb, ownerGuid, defaultLibraryFormModel);
    }
    public async Task<Library_ProcessResult> CreateDefaultShelf(Library_DbContext libraryDb, string ownerGuid)
    {
        // creating Default shelf in Default library
        Library_NewShelfFormModel defaultShelfFormModel = new()
        {
            Title = "Default Shelf",
            Description = "Containing all documents that doesn't belong to anyother shelves.",
        };
        return await CreateNewShelf(libraryDb, ownerGuid, defaultShelfFormModel);
    }

    //************************************ seed Library data **********************************
    /*
        public async Task Update_LibrarySeed(Library_LibraryDbModel libraryDbModel)
        {
            Library_LibrarySeedModel seedModel = new(libraryDbModel);
            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
            string seedPath = Path.Combine(seedDirectory.FullName, "data.json");
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_LibrarySeed(string libraryGuid)
        {
            string seedPath = Path.Combine(Storage_Libraries.FullName, libraryGuid, "data.json");
            if (File.Exists(seedPath))
            {
                File.Delete(seedPath);
            }
        }
        public async Task Seed_LibrariesToDb(Library_DbContext libraryDb)
        {
            foreach (var libraryDirectory in Storage_Libraries.EnumerateDirectories())
            {
                var libraryDbModel = await libraryDb.Libraries
                .FirstOrDefaultAsync(lib => lib.Guid == libraryDirectory.Name);
                if (libraryDbModel is not null) continue;

                //here librariDbmodel is null
                string seedPath = Path.Combine(Storage_Libraries.FullName, libraryDirectory.Name, "data.json");
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
                catch
                {
                    //log
                    Console.WriteLine($"\n***** an exception occured during deserializing library seed data! libraryGuid: '{libraryDirectory.Name}'");
                    continue;
                }
                if (seedModel is not null)
                {
                    libraryDbModel = await seedModel.Create_LibraryDbModel(libraryDb);
                    await libraryDb.Libraries.AddAsync(libraryDbModel);
                }
            }
            await libraryDb.SaveChangesAsync();
        }

        //************************************ seed Shelf data **********************************
        public async Task Update_ShelfSeed(Library_ShelfDbModel shelfDbModel)
        {
            Library_ShelfSeedModel seedModel = new(shelfDbModel);
            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
            string seedPath = Path.Combine(seedDirectory.FullName, "data.json");
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_ShelfSeed(string shelfGuid)
        {
            string seedPath = Path.Combine(Storage_Shelves.FullName, shelfGuid, "data.json");
            if (File.Exists(seedPath))
            {
                File.Delete(seedPath);
            }
        }
        public async Task Seed_ShelvesToDb(Library_DbContext libraryDb)
        {
            foreach (var shelfDirectory in Storage_Shelves.EnumerateDirectories())
            {
                var shelfDbModel = await libraryDb.Shelves
                .FirstOrDefaultAsync(shelf => shelf.Guid == shelfDirectory.Name);
                if (shelfDbModel is not null) continue;

                //here shelfDbModel is null
                string seedPath = Path.Combine(Storage_Shelves.FullName, shelfDirectory.Name, "data.json");
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
                catch
                {
                    //log
                    Console.WriteLine($"\n***** an exception occured during deserializing shelf seed data! shelfGuid: '{shelfDirectory.Name}'");
                    continue;
                }
                if (seedModel is not null)
                {
                    shelfDbModel = await seedModel.Create_ShelfDbModel(libraryDb);
                    await libraryDb.Shelves.AddAsync(shelfDbModel);
                }
            }
            await libraryDb.SaveChangesAsync();
        }

        //************************************ seed Document data **********************************
        public async Task Update_DocumentSeed(Library_DocumentDbModel documentDbModel)
        {
            Library_DocumentSeedModel seedModel = new(documentDbModel);
            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, documentDbModel.Guid));
            string seedPath = Path.Combine(seedDirectory.FullName, "data.json");
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_DocumentSeed(string documentGuid)
        {
            string seedPath = Path.Combine(Storage_Documents.FullName, documentGuid, "data.json");
            if (File.Exists(seedPath))
            {
                File.Delete(seedPath);
            }
        }
        public async Task Seed_DocumentsToDb(Library_DbContext libraryDb)
        {
            foreach (var documentDirectory in Storage_Documents.EnumerateDirectories())
            {
                var documentDbModel = await libraryDb.Documents
                .FirstOrDefaultAsync(doc => doc.Guid == documentDirectory.Name);
                if (documentDbModel is not null) continue;

                //here documentDbModel is null
                string seedPath = Path.Combine(Storage_Documents.FullName, documentDirectory.Name, "data.json");
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
                catch
                {
                    //log
                    Console.WriteLine($"\n***** an exception occured during deserializing document seed data! documentGuid: '{documentDirectory.Name}'");
                    continue;
                }
                if (seedModel is not null)
                {
                    documentDbModel = await seedModel.Create_DocumentDbModel(libraryDb);
                    await libraryDb.Documents.AddAsync(documentDbModel);
                }
            }
            await libraryDb.SaveChangesAsync();
        }

        //************************************ seed Element data **********************************
        public async Task Update_ElementSeed(Library_ElementDbModel elementDbModel)
        {
            Library_ElementSeedModel seedModel = new(elementDbModel);
            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Elements.FullName, elementDbModel.Guid));
            string seedPath = Path.Combine(seedDirectory.FullName, "data.json");
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_ElementSeed(string elementGuid)
        {
            string seedPath = Path.Combine(Storage_Elements.FullName, elementGuid, "data.json");
            if (File.Exists(seedPath))
            {
                File.Delete(seedPath);
            }
        }
        public async Task Seed_ElementsToDb(Library_DbContext libraryDb)
        {
            foreach (var elementDirectory in Storage_Elements.EnumerateDirectories())
            {
                var elementDbModel = await libraryDb.Elements
                .FirstOrDefaultAsync(el => el.Guid == elementDirectory.Name);
                if (elementDbModel is not null) continue;

                //here elementDbModel is null
                string seedPath = Path.Combine(Storage_Elements.FullName, elementDirectory.Name, "data.json");
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
                catch
                {
                    //log
                    Console.WriteLine($"\n***** an exception occured during deserializing element seed data! elementGuid: '{elementDirectory.Name}'");
                    continue;
                }
                if (seedModel is not null)
                {
                    elementDbModel = await seedModel.Create_ElementDbModel(libraryDb);
                    if (elementDbModel is null)
                    {
                        Console.WriteLine($"\n***** Parent document for the element is null! You first need to seed the parent document.");
                        continue;
                    }
                    await libraryDb.Elements.AddAsync(elementDbModel);
                }
            }
            await libraryDb.SaveChangesAsync();
        }

        //************************************ seed RelatedVersions data **********************************
        public async Task Update_RelatedVersionsSeed(Library_RelatedVersionsDbModel rvDbModel)
        {
            Library_RelatedVersionsSeedModel seedModel = new(rvDbModel);
            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_RelatedVersions.FullName, rvDbModel.Guid));
            string seedPath = Path.Combine(seedDirectory.FullName, "data.json");
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_RelatedVersionsSeed(string rvGuid)
        {
            string seedPath = Path.Combine(Storage_RelatedVersions.FullName, rvGuid, "data.json");
            if (File.Exists(seedPath))
            {
                File.Delete(seedPath);
            }
        }
        public async Task Seed_RelatedVersionsToDb(Library_DbContext libraryDb)
        {
            foreach (var rvDirectory in Storage_RelatedVersions.EnumerateDirectories())
            {
                var rvDbModel = await libraryDb.RelatedVersions
                .FirstOrDefaultAsync(el => el.Guid == rvDirectory.Name);
                if (rvDbModel is not null) continue;

                //here rvDbModel is null
                string seedPath = Path.Combine(Storage_RelatedVersions.FullName, rvDirectory.Name, "data.json");
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
                catch
                {
                    //log
                    Console.WriteLine($"\n***** an exception occured during deserializing related versions seed data! rvGuid: '{rvDirectory.Name}'");
                    continue;
                }
                if (seedModel is not null)
                {
                    rvDbModel = await seedModel.Create_RelatedVersionsDbModel(libraryDb);
                    await libraryDb.RelatedVersions.AddAsync(rvDbModel);
                }
            }
            await libraryDb.SaveChangesAsync();
        }

        //************************************ seed Tag data **********************************
        public async Task Update_TagSeed(Library_TagDbModel tagDbModel)
        {
            Library_TagSeedModel seedModel = new(tagDbModel);
            string json = JsonSerializer.Serialize(seedModel);
            DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Tags.FullName, tagDbModel.Name));
            string seedPath = Path.Combine(seedDirectory.FullName, "data.json");
            await File.WriteAllTextAsync(seedPath, json);
        }
        public void Delete_TagSeed(string tagGuid)
        {
            string seedPath = Path.Combine(Storage_Tags.FullName, tagGuid, "data.json");
            if (File.Exists(seedPath))
            {
                File.Delete(seedPath);
            }
        }
        public async Task Seed_TagsToDb(Library_DbContext libraryDb)
        {
            foreach (var tagDirectory in Storage_Tags.EnumerateDirectories())
            {
                var tagDbModel = await libraryDb.Tags
                .FirstOrDefaultAsync(tag => tag.Name == tagDirectory.Name);
                if (tagDbModel is not null) continue;

                //here rvDbModel is null
                string seedPath = Path.Combine(Storage_Tags.FullName, tagDirectory.Name, "data.json");
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
                catch
                {
                    //log
                    Console.WriteLine($"\n***** an exception occured during deserializing tag seed data! tagName: '{tagDirectory.Name}'");
                    continue;
                }
                if (seedModel is not null)
                {
                    tagDbModel = await seedModel.Create_TagDbModel(libraryDb);
                    await libraryDb.Tags.AddAsync(tagDbModel);
                }
            }
            await libraryDb.SaveChangesAsync();
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

//************************************ Seed Data Models ********************************
public class Library_LibrarySeedModel
{
    public string Guid { get; set; } = null!;
    public string OwnerGuid { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public string[] ShelvesGuids { get; set; } = [];
    public DateTime CreatedAt { get; set; }

    public Library_LibrarySeedModel() { }
    public Library_LibrarySeedModel(Library_LibraryDbModel libraryDbModel)
    {
        Guid = libraryDbModel.Guid;
        OwnerGuid = libraryDbModel.OwnerGuid;
        Title = libraryDbModel.Title;
        Description = libraryDbModel.Description;
        ShelvesGuids = libraryDbModel.Shelves.Select(shelf => shelf.Guid).ToArray();
        CreatedAt = libraryDbModel.CreatedAt;
    }

    public async Task<Library_LibraryDbModel> Create_LibraryDbModel(Library_DbContext libraryDb)
    {
        List<Library_ShelfDbModel> shelves = await libraryDb.Shelves
        .Where(shelf => ShelvesGuids.Contains(shelf.Guid))
        .ToListAsync();

        Library_LibraryDbModel libraryDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Guid = Guid,
            OwnerGuid = OwnerGuid,
            Shelves = shelves,
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
    public string[] LibrariesGuids { get; set; } = [];
    public string[] DocumentsGuids { get; set; } = [];
    public DateTime CreatedAt { get; set; }

    public Library_ShelfSeedModel() { }
    public Library_ShelfSeedModel(Library_ShelfDbModel shelfDbModel)
    {
        Guid = shelfDbModel.Guid;
        OwnerGuid = shelfDbModel.OwnerGuid;
        Title = shelfDbModel.Title;
        Description = shelfDbModel.Description;
        LibrariesGuids = shelfDbModel.Libraries.Select(lib => lib.Guid).ToArray();
        DocumentsGuids = shelfDbModel.Documents.Select(doc => doc.Guid).ToArray();
        CreatedAt = shelfDbModel.CreatedAt;
    }

    public async Task<Library_ShelfDbModel> Create_ShelfDbModel(Library_DbContext libraryDb)
    {
        List<Library_LibraryDbModel> libraries = await libraryDb.Libraries
        .Where(lib => LibrariesGuids.Contains(lib.Guid))
        .ToListAsync();

        List<Library_DocumentDbModel> documents = await libraryDb.Documents
        .Where(doc => DocumentsGuids.Contains(doc.Guid))
        .ToListAsync();

        Library_ShelfDbModel shelfDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Documents = documents,
            Guid = Guid,
            Libraries = libraries,
            OwnerGuid = OwnerGuid,
            Title = Title,
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
    public string? RelatedVersionsGuid { get; set; }
    public string[] ShelvesGuids { get; set; } = [];
    public string[] ElementsGuids { get; set; } = [];
    public string[] TagsNames { get; set; } = [];
    public DateTime CreatedAt { get; set; }

    public Library_DocumentSeedModel() { }
    public Library_DocumentSeedModel(Library_DocumentDbModel documentDbModel)
    {
        Guid = documentDbModel.Guid;
        OwnerGuid = documentDbModel.OwnerGuid;
        Title = documentDbModel.Title;
        Description = documentDbModel.Description;
        Version = documentDbModel.Version;
        RelatedVersionsGuid = documentDbModel.RelatedVersions?.Guid;
        ShelvesGuids = documentDbModel.Shelves.Select(shelf => shelf.Guid).ToArray();
        ElementsGuids = documentDbModel.Elements.Select(el => el.Guid).ToArray();
        TagsNames = documentDbModel.Tags.Select(tag => tag.Name).ToArray();
        CreatedAt = documentDbModel.CreatedAt;
    }

    public async Task<Library_DocumentDbModel> Create_DocumentDbModel(Library_DbContext libraryDb)
    {
        Library_RelatedVersionsDbModel? relaredVersionsDbmodel = await libraryDb.RelatedVersions
        .FirstOrDefaultAsync(rv => rv.Guid == RelatedVersionsGuid);

        List<Library_ShelfDbModel> shelves = await libraryDb.Shelves
        .Where(shelf => ShelvesGuids.Contains(shelf.Guid))
        .ToListAsync();

        List<Library_ElementDbModel> elements = await libraryDb.Elements
        .Where(el => ElementsGuids.Contains(el.Guid))
        .ToListAsync();

        List<Library_TagDbModel> tags = await libraryDb.Tags
        .Where(tag => TagsNames.Contains(tag.Name))
        .ToListAsync();

        Library_DocumentDbModel documentDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            Elements = elements,
            Guid = Guid,
            OwnerGuid = OwnerGuid,
            RelatedVersions = relaredVersionsDbmodel,
            Shelves = shelves,
            Tags = tags,
            Title = Title,
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
    public string DocumentGuid { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }

    public Library_ElementSeedModel() { }
    public Library_ElementSeedModel(Library_ElementDbModel elementDbModel)
    {
        Guid = elementDbModel.Guid;
        OwnerGuid = elementDbModel.OwnerGuid;
        Type = elementDbModel.Type;
        Value = elementDbModel.Value;
        Title = elementDbModel.Title;
        FileName = elementDbModel.FileName;
        Order = elementDbModel.Order;
        DocumentGuid = elementDbModel.Document.Guid;
        UpdatedAt = elementDbModel.UpdatedAt;
    }

    public async Task<Library_ElementDbModel?> Create_ElementDbModel(Library_DbContext libraryDb)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .FirstOrDefaultAsync(doc => doc.Guid == DocumentGuid);

        if (documentDbModel is null)
        {
            return null;
        }

        Library_ElementDbModel elementDbModel = new()
        {
            Document = documentDbModel,
            FileName = FileName,
            Guid = Guid,
            Order = Order,
            OwnerGuid = OwnerGuid,
            Title = Title,
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

    public Library_RelatedVersionsSeedModel() { }
    public Library_RelatedVersionsSeedModel(Library_RelatedVersionsDbModel relatedVersionsDbModel)
    {
        Guid = relatedVersionsDbModel.Guid;
        DocumentsGuids = relatedVersionsDbModel.Documents.Select(doc => doc.Guid).ToArray();
    }

    public async Task<Library_RelatedVersionsDbModel> Create_RelatedVersionsDbModel(Library_DbContext libraryDb)
    {
        List<Library_DocumentDbModel> documents = await libraryDb.Documents
        .Where(doc => DocumentsGuids.Contains(doc.Guid))
        .ToListAsync();

        Library_RelatedVersionsDbModel relatedversionsDbmodel = new()
        {
            Documents = documents,
            Guid = Guid,
        };

        return relatedversionsDbmodel;
    }
}

public class Library_TagSeedModel
{
    public string Name { get; set; } = null!;
    public string[] DocumentsGuids { get; set; } = [];

    public Library_TagSeedModel() { }
    public Library_TagSeedModel(Library_TagDbModel tagDbmodel)
    {
        Name = tagDbmodel.Name;
        DocumentsGuids = tagDbmodel.Documents.Select(doc => doc.Guid).ToArray();
    }

    public async Task<Library_TagDbModel> Create_TagDbModel(Library_DbContext libraryDb)
    {
        List<Library_DocumentDbModel> documents = await libraryDb.Documents
        .Where(doc => DocumentsGuids.Contains(doc.Guid))
        .ToListAsync();

        Library_TagDbModel tagDbModel = new()
        {
            Documents = documents,
            Name = Name,
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

