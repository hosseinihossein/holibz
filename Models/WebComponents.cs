using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace holibz.Models;
public class WebComponents_DbContext : DbContext
{
    public DbSet<WebComponents_ItemDbModel> Items { get; set; } = null!;
    public DbSet<WebComponents_TagDbModel> Tags { get; set; } = null!;

    public WebComponents_DbContext(DbContextOptions<WebComponents_DbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //***** many-to-many , Item to Tag
        modelBuilder.Entity<WebComponents_ItemDbModel>()
        .HasMany(item => item.TagDbModels)
        .WithMany(tag => tag.ItemDbModels);
    }
}

public class WebComponents_ItemDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = string.Empty;
    public string DeveloperGuid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    //public string ReviewGuid { get; set; } = string.Empty;
    public List<WebComponents_TagDbModel> TagDbModels { get; set; } = new();
    public DateTime Date { get; set; } = DateTime.Now;

    public override bool Equals(object? obj)
    {
        if (obj is not null && obj!.GetType() == typeof(WebComponents_ItemDbModel))
        {
            var temp = obj! as WebComponents_ItemDbModel;
            if (temp!.Id == this.Id) return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id;
    }
}
public class WebComponents_TagDbModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<WebComponents_ItemDbModel> ItemDbModels { get; set; } = new();

    public override bool Equals(object? obj)
    {
        if (obj is not null && obj!.GetType() == typeof(WebComponents_TagDbModel))
        {
            var temp = obj! as WebComponents_TagDbModel;
            if (temp!.Id == this.Id) return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public class WebComponents_ElasticsearchModel
{
    public string Guid { get; set; } = string.Empty;
    public string DeveloperGuid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
}

public class WebComponents_NewItemFormModel
{
    [StringLength(50)]
    public string Title { get; set; } = string.Empty;
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    [StringLength(100)]
    public string SelectedTags { get; set; } = string.Empty;
    [Required]
    public IFormFile? ZipFile { get; set; }
    public string? TagsJson { get; set; }
}

public class WebComponents_ItemModel
{
    public string Guid { get; set; } = string.Empty;
    public Identity_UserModel? Developer { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    //public string ReviewGuid { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string SearchScore { get; set; } = string.Empty;
}

public class WebComponents_EditItemTagsModel
{
    public List<string> AllTags { get; set; } = [];
    public List<string> CurrentTags { get; set; } = [];
    public string ItemGuid { get; set; } = string.Empty;
}

public class WebComponents_IndexModel
{
    public List<WebComponents_ItemModel> Items { get; set; } = new();
    public string TagsJson { get; set; } = string.Empty;
    public string SelectedTags { get; set; } = string.Empty;
    public string DeveloperUserName { get; set; } = string.Empty;
    public string SearchPhrase { get; set; } = string.Empty;
}