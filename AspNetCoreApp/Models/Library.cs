using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
}
public class Library_RelatedVersionsDbModel
{
    public int ID { get; set; }
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
public class Library_process //singleton service
{
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
    Library_NewLibrayFormModel formModel)
    {
        if (!await libraryDb.Libraries.AnyAsync(lib =>
            lib.OwnerGuid == ownerGuid && lib.Title == formModel.Title))
        {
            Library_LibraryDbModel libraryDbModel = new()
            {
                Title = formModel.Title,
                OwnerGuid = ownerGuid,
                Description = formModel.Decription,
            };

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

            Library_ShelfDbModel shelfDbModel = new()
            {
                Title = formModel.Title,
                OwnerGuid = ownerGuid,
                Description = formModel.Decription,
                Library = libraryContainer,
            };

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
    public string OwnerUsername { get; set; } = null!;
}
public class Library_NewLibrayFormModel
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
    public string? LibraryGuid { get; set; } = null;
}

