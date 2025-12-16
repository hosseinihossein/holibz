using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    readonly DirectoryInfo Backup_Directory;
    readonly FileInfo StatusFileInfo;

    public BackupController(IWebHostEnvironment _env, UserManager<Identity_UserDbModel> userManager,
    Identity_Process identityProcess, Library_DbContext libraryDb, Library_Process libraryProcess,
    Review_DbContext reviewDb, Review_Process reviewProcess)
    {
        this.userManager = userManager;
        this.identityProcess = identityProcess;
        this.libraryDb = libraryDb;
        this.libraryProcess = libraryProcess;
        this.reviewDb = reviewDb;
        this.reviewProcess = reviewProcess;

        this.Backup_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Backup"));
        System.IO.File.Create(Path.Combine(Backup_Directory.FullName, "status.json"));
    }





    [HttpGet]
    public async Task<IActionResult> GetFullBackup()
    {

    }
}