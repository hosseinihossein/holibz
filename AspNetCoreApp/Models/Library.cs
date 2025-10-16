using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

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
public class TagNameBuilder
{
    public string? Build(string value)
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
}