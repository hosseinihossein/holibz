using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public enum Backup_StatusEnum
{
    Deleting_Old_Seeds_Started,
    Deleting_Old_Seeds_Completed,
    Deleting_Removed_Entitiies_Directories_Started,
    Deleting_Removed_Entitiies_Directories_Completed,
    Not_Started,
    Generating_Seed_Started,
    Generating_Seed_Completed,
    Creating_Zip_File_Started,
    Creating_Zip_File_Completed,
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
    public int FileSize { get; set; }
    public string FileName { get; set; } = null!;?
}

public class Backup_Process
{
    public readonly DirectoryInfo Backup_Directory;
    public readonly string StatusFilePath;
    readonly string SeedFileName;
    public readonly string BackupFileName = "backup.zip";
    readonly DirectoryInfo Storage_Directory;

    readonly Identity_Process identityProcess;
    readonly Library_Process libraryProcess;
    readonly Review_Process reviewProcess;
    readonly Notification_Process notifProcess;





    public Backup_Process(IWebHostEnvironment _env, Identity_Process identityProcess,
    Library_Process libraryProcess, Review_Process reviewProcess, Notification_Process notifProcess,
    IConfiguration config)
    {
        Backup_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Backup"));
        StatusFilePath = Path.Combine(Backup_Directory.FullName, "status.json");
        SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
        Storage_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage"));

        this.identityProcess = identityProcess;
        this.libraryProcess = libraryProcess;
        this.reviewProcess = reviewProcess;
        this.notifProcess = notifProcess;
    }





    public async Task BackupFullProcess(UserManager<Identity_UserDbModel> userManager,
    Library_DbContext libraryDb, Review_DbContext reviewDb, Notification_DbContext notifDb)
    {
        //define backup status
        Backup_Status status = new();

        //delete old seed files, then deleted entities directories can be distinguished
        status.Overall_Status = Backup_StatusEnum.Deleting_Old_Seeds_Started.ToString();
        string statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
        DeleteOldSeeds();
        status.Overall_Status += " , " + Backup_StatusEnum.Deleting_Old_Seeds_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        //****************** Generate Identity Seed ******************
        status.Identity_SeedStatus = Backup_StatusEnum.Generating_Seed_Started.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
        await GenerateIdentitySeed(userManager);
        status.Identity_SeedStatus = Backup_StatusEnum.Generating_Seed_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        //****************** Generate Library Seed ******************
        status.Library_SeedStatus = Backup_StatusEnum.Generating_Seed_Started.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
        await GenerateLibrarySeed(libraryDb);
        status.Library_SeedStatus = Backup_StatusEnum.Generating_Seed_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        //****************** Generate Review Seed ******************
        status.Review_SeedStatus = Backup_StatusEnum.Generating_Seed_Started.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
        await GenerateReviewSeed(reviewDb);
        status.Review_SeedStatus = Backup_StatusEnum.Generating_Seed_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        //****************** Generate Notification Seed ******************
        status.Notification_SeedStatus = Backup_StatusEnum.Generating_Seed_Started.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
        await GenerateNotificationSeed(notifDb);
        status.Notification_SeedStatus = Backup_StatusEnum.Generating_Seed_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        //delete directories without seed file
        status.Overall_Status += " , " + Backup_StatusEnum.Deleting_Removed_Entitiies_Directories_Started.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
        DeleteRemovedEntetiesDirectories();
        status.Overall_Status += " , " + Backup_StatusEnum.Deleting_Removed_Entitiies_Directories_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        //zip the Storage directory
        status.Overall_Status += " , " + Backup_StatusEnum.Creating_Zip_File_Started.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        string backupFilePath = Path.Combine(Backup_Directory.FullName, BackupFileName);
        ZipFile.CreateFromDirectory(Storage_Directory.FullName, backupFilePath);

        status.Overall_Status += " , " + Backup_StatusEnum.Creating_Zip_File_Completed.ToString();
        FileInfo backupFileInfo = new FileInfo(backupFilePath);
        if (backupFileInfo.Exists)
        {
            status.FileSize = (int)backupFileInfo.Length / 1024 / 1024;//size in MB
        }
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

    }
    public async Task GenerateIdentitySeed(UserManager<Identity_UserDbModel> userManager)
    {
        //***** Identity *****
        await userManager.Users.ForEachAsync(async u =>
        {
            await identityProcess.Update_UserSeed(u, userManager);
        });
    }
    public async Task GenerateLibrarySeed(Library_DbContext libraryDb)
    {
        //***** Owner *****
        await libraryDb.Owners.Select(o => o.Guid).ForEachAsync(async ownerGuid =>
        {
            await libraryProcess.Update_OwnerSeed(ownerGuid, libraryDb);
        });
        //***** Library *****
        await libraryDb.Libraries.Select(l => l.Guid).ForEachAsync(async libGuid =>
        {
            await libraryProcess.Update_LibrarySeed(libGuid, libraryDb);
        });
        //***** Shelf *****
        await libraryDb.Shelves.Select(sh => sh.Guid).ForEachAsync(async shelfGuid =>
        {
            await libraryProcess.Update_ShelfSeed(shelfGuid, libraryDb);
        });
        //***** Document *****
        await libraryDb.Documents.Select(d => d.Guid).ForEachAsync(async docGuid =>
        {
            await libraryProcess.Update_DocumentSeed(docGuid, libraryDb);
        });
        //***** Element *****
        await libraryDb.Elements.Select(el => el.Guid).ForEachAsync(async elementGuid =>
        {
            await libraryProcess.Update_ElementSeed(elementGuid, libraryDb);
        });
        //***** RelatedVersions *****
        await libraryDb.RelatedVersions.Select(rv => rv.Guid).ForEachAsync(async rvGuid =>
        {
            await libraryProcess.Update_RelatedVersionsSeed(rvGuid, libraryDb);
        });
        //***** Tag *****
        await libraryDb.Tags.Select(t => t.Name).ForEachAsync(async tagName =>
        {
            await libraryProcess.Update_TagSeed(tagName, libraryDb);
        });
    }
    public async Task GenerateReviewSeed(Review_DbContext reviewDb)
    {
        //***** Review *****
        await reviewDb.Reviews.Select(r => r.SubjectGuid).ForEachAsync(async subjectGuid =>
        {
            await reviewProcess.Update_ReviewSeed(subjectGuid, reviewDb);
        });
        //***** Comment *****
        await reviewDb.Comments.Select(c => c.Guid).ForEachAsync(async commentGuid =>
        {
            await reviewProcess.Update_CommentSeed(commentGuid, reviewDb);
        });
    }
    public async Task GenerateNotificationSeed(Notification_DbContext notifDb)
    {
        //***** Notification *****
        await notifDb.Notifications.Select(n => n.Guid).ForEachAsync(async notifGuid =>
        {
            await notifProcess.Update_NotificationSeed(notifGuid, notifDb);
        });
    }
    public void DeleteOldSeeds()
    {
        IEnumerable<FileInfo> oldSeedFiles =
        Storage_Directory.EnumerateFiles(SeedFileName, SearchOption.AllDirectories);
        foreach (FileInfo fileInfo in oldSeedFiles)
        {
            try
            {
                fileInfo.Delete();
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** {e.Message} *****");
            }
        }
    }
    public void DeleteRemovedEntetiesDirectories()
    {
        IEnumerable<DirectoryInfo> directories =
        Storage_Directory.EnumerateDirectories("*", SearchOption.AllDirectories);
        foreach (DirectoryInfo directoryInfo in directories)
        {
            //if there's no seed file in it and in its children
            if (directoryInfo.Exists &&
            directoryInfo.GetFiles(SeedFileName, SearchOption.AllDirectories).Length == 0)
            {
                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine($"\n     ***** {e.Message} *****");
                }
            }
        }
    }

    public async Task GenerateBackupZipFile()
    {
        //define backup status
        Backup_Status status = new();

        //zip the Storage directory
        status.Overall_Status += " , " + Backup_StatusEnum.Creating_Zip_File_Started.ToString();
        string statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);

        string backupFilePath = Path.Combine(Backup_Directory.FullName, BackupFileName);
        ZipFile.CreateFromDirectory(Storage_Directory.FullName, backupFilePath);

        status.Overall_Status += " , " + Backup_StatusEnum.Creating_Zip_File_Completed.ToString();
        FileInfo backupFileInfo = new FileInfo(backupFilePath);
        if (backupFileInfo.Exists)
        {
            status.FileSize = (int)backupFileInfo.Length / 1024 / 1024;//size in MB
        }
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(StatusFilePath, statusJson);
    }

}
/*public class Backup_Result
{
    public bool Success { get; set; }
    public string? Description { get; set; }
}*/