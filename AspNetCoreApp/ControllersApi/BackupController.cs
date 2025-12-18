using System.IO.Compression;
using System.Text.Json;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class BackupController : ControllerBase
{
    readonly UserManager<Identity_UserDbModel> userManager;
    readonly Library_DbContext libraryDb;
    readonly Review_DbContext reviewDb;
    readonly Notification_DbContext notifDb;

    readonly Backup_Process backupProcess;


    public BackupController(UserManager<Identity_UserDbModel> userManager,
    Library_DbContext libraryDb, Review_DbContext reviewDb, Notification_DbContext notifDb,
    Backup_Process backupProcess)
    {
        this.userManager = userManager;
        this.libraryDb = libraryDb;
        this.reviewDb = reviewDb;
        this.notifDb = notifDb;

        this.backupProcess = backupProcess;
    }





    [HttpGet]
    [Authorize(Roles = "Backup_Admins")]
    public async Task<IActionResult> GetBackupStatus()
    {
        Backup_Status? status = null;
        if (System.IO.File.Exists(backupProcess.StatusFilePath))
        {
            status = JsonSerializer.Deserialize<Backup_Status>(backupProcess.StatusFilePath);
        }
        if (status is null)
        {
            status = new();
        }
        return Ok(status);
    }

    [HttpGet]
    [Authorize(Roles = "Backup_Admins")]
    public async Task<IActionResult> DownloadBackupFile()
    {
        string backupFilePath = Path.Combine(backupProcess.Backup_Directory.FullName, backupProcess.BackupFileName);
        if (System.IO.File.Exists(backupFilePath))
        {
            DateTime createdAt = System.IO.File.GetLastWriteTimeUtc(backupFilePath);
            string downloadFileName = createdAt.ToString("YYYY_MM_dd_HH_mm_ss") + "_" + backupProcess.BackupFileName;
            return PhysicalFile(backupFilePath, "application/octet-stream", downloadFileName, true);
        }
        return NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "Backup_Admins")]
    public async Task<IActionResult> GenerateFullBackup()
    {
        _ = backupProcess.BackupFullProcess(userManager, libraryDb, reviewDb, notifDb);
        return Ok(new { success = true });
    }


}