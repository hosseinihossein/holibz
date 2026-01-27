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
        if (System.IO.File.Exists(backupProcess.Backup_Status_FilePath))
        {
            string json = await System.IO.File.ReadAllTextAsync(backupProcess.Backup_Status_FilePath);
            try
            {
                status = JsonSerializer.Deserialize<Backup_Status>(json);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine(e.Message);
            }
        }
        status ??= new();
        return Ok(status);
    }

    [HttpGet]
    [Authorize(Roles = "Backup_Admins")]
    public async Task<IActionResult> DownloadBackupFile()
    {
        Backup_Status? status = null;
        if (System.IO.File.Exists(backupProcess.Backup_Status_FilePath))
        {
            string json = await System.IO.File.ReadAllTextAsync(backupProcess.Backup_Status_FilePath);
            status = JsonSerializer.Deserialize<Backup_Status>(json);
        }
        if (status is null)
        {
            return NotFound("status file not found!");
        }
        string backupFilePath = Path.Combine(backupProcess.Backup_Directory.FullName, status.File_Name ?? backupProcess.BackupFileNameWithoutDate);
        if (System.IO.File.Exists(backupFilePath))
        {
            /*In ASP.NET Core, when you return a file using PhysicalFile, File, or FileContentResult, 
            the framework automatically sets the Content-Disposition header to attachment if 
            you pass a fileDownloadName.*/
            /*
            Content-Disposition: inline; Display the content in the browser.
            Content-Disposition: attachment; Prompt the user to download the file.
            */
            return PhysicalFile(backupFilePath, "application/octet-stream", status.File_Name, true);
        }
        return NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "Backup_Admins")]
    public IActionResult GenerateBackupFile()
    {
        _ = backupProcess.Generate_Backup_ZipFile();
        return Ok();
    }

    [HttpDelete]
    [Authorize(Roles = "Backup_Admins")]
    public IActionResult DeleteBackupFile()
    {
        if (backupProcess.Backup_Db_Directory.Exists)
        {
            try
            {
                backupProcess.Backup_Db_Directory.Delete(true);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** couldn't delete directory {backupProcess.Backup_Db_Directory.FullName} *****");
                Console.WriteLine(e.Message);
            }
        }

        if (backupProcess.Backup_Directory.Exists)
        {
            try
            {
                backupProcess.Backup_Directory.Delete(true);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** couldn't delete directory {backupProcess.Backup_Directory.FullName} *****");
                Console.WriteLine(e.Message);
            }
        }

        return Ok();
    }



    /*[HttpGet]
    [Authorize(Roles = "Backup_Admins")]
    public async Task<IActionResult> ProcessFullBackup()
    {
        _ = backupProcess.BackupFullProcess(userManager, libraryDb, reviewDb, notifDb);
        return Ok();
    }*/


}