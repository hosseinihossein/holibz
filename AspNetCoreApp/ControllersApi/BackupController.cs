using System.Text.Json;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class BackupController : ControllerBase
{
    readonly UserManager<Identity_UserDbModel> userManager;
    readonly Identity_Process identityProcess;
    readonly Library_DbContext libraryDb;
    readonly Library_Process libraryProcess;
    readonly Review_DbContext reviewDb;
    readonly Review_Process reviewProcess;
    readonly Notification_DbContext notifDb;
    readonly Notification_Process notifProcess;

    readonly Backup_Process backupProcess;
    readonly string SeedFileName;
    readonly DirectoryInfo Storage_Directory;


    public BackupController(UserManager<Identity_UserDbModel> userManager,
    Identity_Process identityProcess, Library_DbContext libraryDb, Library_Process libraryProcess,
    Review_DbContext reviewDb, Review_Process reviewProcess, Notification_DbContext notifDb,
    Notification_Process notifProcess, Backup_Process backupProcess, IConfiguration config,
    IWebHostEnvironment _env)
    {
        this.userManager = userManager;
        this.identityProcess = identityProcess;
        this.libraryDb = libraryDb;
        this.libraryProcess = libraryProcess;
        this.reviewDb = reviewDb;
        this.reviewProcess = reviewProcess;
        this.notifDb = notifDb;
        this.notifProcess = notifProcess;

        this.backupProcess = backupProcess;
        SeedFileName = config["SeedFileName"] ?? "holibzSeedData.json";
        Storage_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage"));
    }





    [HttpGet]
    public async Task<IActionResult> GenerateFullBackup()
    {
        //define backup status
        Backup_Status status = new();

        //delete old seed files, then deleted entities directories can be distinguished
        status.Overall_Status = Backup_StatusEnum.Deleting_Old_Seeds_Started.ToString();
        string statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);
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
                Console.WriteLine($"\n     ***** {e.Message} *****");
                status.Description = [.. status.Description, e.Message];
            }
        }
        status.Overall_Status = Backup_StatusEnum.Deleting_Old_Seeds_Completed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);

        //****************** Generate Identity Seed ******************
        status.Identity_SeedStatus = Backup_StatusEnum.Generating_Seed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);
        await GenerateIdentitySeed();
        status.Identity_SeedStatus = Backup_StatusEnum.Seed_Generated.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);

        //****************** Generate Library Seed ******************
        status.Library_SeedStatus = Backup_StatusEnum.Generating_Seed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);
        await GenerateLibrarySeed();
        status.Library_SeedStatus = Backup_StatusEnum.Seed_Generated.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);

        //****************** Generate Review Seed ******************
        status.Review_SeedStatus = Backup_StatusEnum.Generating_Seed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);
        await GenerateReviewSeed();
        status.Review_SeedStatus = Backup_StatusEnum.Seed_Generated.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);

        //****************** Generate Notification Seed ******************
        status.Notification_SeedStatus = Backup_StatusEnum.Generating_Seed.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);
        await GenerateNotificationSeed();
        status.Notification_SeedStatus = Backup_StatusEnum.Seed_Generated.ToString();
        statusJson = JsonSerializer.Serialize(status);
        await System.IO.File.WriteAllTextAsync(backupProcess.StatusFilePath, statusJson);

        //delete directories without seed file
        Storage_Directory.EnumerateDirectories("*", SearchOption.AllDirectories);
    }

    private async Task GenerateIdentitySeed()
    {
        //***** Identity *****
        await userManager.Users.ForEachAsync(async u =>
        {
            await identityProcess.Update_UserSeed(u, userManager);
        });
    }
    private async Task GenerateLibrarySeed()
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
    private async Task GenerateReviewSeed()
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
    private async Task GenerateNotificationSeed()
    {
        //***** Notification *****
        await notifDb.Notifications.Select(n => n.Guid).ForEachAsync(async notifGuid =>
        {
            await notifProcess.Update_NotificationSeed(notifGuid, notifDb);
        });
    }
}