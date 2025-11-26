using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public class Notification_UserDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = null!;
    public List<Notification_NotificationDbModel> Notifications { get; set; } = [];
}
public class Notification_NotificationDbModel
{
    public int Id { get; set; }
    public string Guid { get; set; } = System.Guid.NewGuid().ToString().Replace("-", "");
    public string EventGuid { get; set; } = null!;
    public Notification_UserDbModel Owner { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; } = null;
    public string? Link { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification_DbContext : DbContext
{
    public Notification_DbContext(DbContextOptions<Notification_DbContext> options) : base(options) { }

    public DbSet<Notification_UserDbModel> Users { get; set; } = null!;
    public DbSet<Notification_NotificationDbModel> Notifications { get; set; } = null!;

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
        .HasIndex(notif => notif.EventGuid)
        .IsUnique(true);
    }
}