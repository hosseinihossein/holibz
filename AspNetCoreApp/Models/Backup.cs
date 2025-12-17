using Microsoft.AspNetCore.Identity;

namespace AspNetCoreApp.Models;

public enum Backup_StatusEnum
{
    Deleting_Old_Seeds_Started,
    Deleting_Old_Seeds_Completed,
    Not_Started,
    Generating_Seed,
    Seed_Generated,
    Failed,
}
public class Backup_Status
{
    public string Overall_Status { get; set; } = Backup_StatusEnum.Deleting_Old_Seeds_Started.ToString();
    public string Identity_SeedStatus { get; set; } = Backup_StatusEnum.Not_Started.ToString();
    public string Library_SeedStatus { get; set; } = Backup_StatusEnum.Not_Started.ToString();
    public string Review_SeedStatus { get; set; } = Backup_StatusEnum.Not_Started.ToString();
    public string Notification_SeedStatus { get; set; } = Backup_StatusEnum.Not_Started.ToString();
    public string[] Description { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Backup_Process
{
    public readonly DirectoryInfo Backup_Directory;
    public readonly string StatusFilePath;





    public Backup_Process(IWebHostEnvironment _env)
    {
        Backup_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Backup"));
        StatusFilePath = Path.Combine(Backup_Directory.FullName, "status.json");
    }





    /*
    public async Task<Backup_Result> GenerateBackup_Identity(UserManager<Identity_UserDbModel> userManager)
    {

    }
    public async Task<Backup_Result> GenerateBackup_Library() { }
    public async Task<Backup_Result> GenerateBackup_Review() { }
    public async Task<Backup_Result> GenerateBackup_Notification() { }
    */

}
/*public class Backup_Result
{
    public bool Success { get; set; }
    public string? Description { get; set; }
}*/