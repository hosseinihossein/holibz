using System.ComponentModel.DataAnnotations.Schema;

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
    public List<Library_DocumentDbModel> OtherVersions = [];
    public List<Library_ShelfDbModel> Shelves { get; set; } = [];
    public List<Library_ElementDbModel> Elements { get; set; } = [];
    public List<Library_TagDbModel> Tags { get; set; } = [];
}
public class Library_ElementDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public string OwnerGuid { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Title { get; set; } = null;
    public List<Library_DocumentDbModel> Document { get; set; } = null!;
}
public class Library_TagDbModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Library_DocumentDbModel> Documents { get; set; } = [];
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