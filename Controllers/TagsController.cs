using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using holibz.Models;
using System.Text;

namespace holibz.Controllers;

[AutoValidateAntiforgeryToken]
[Authorize(Roles = "WebComponents_Admins")]
public class TagsController : Controller
{
    readonly WebComponents_DbContext webComponentsDb;
    readonly UserManager<Identity_UserModel> userManager;
    readonly IWebHostEnvironment env;
    readonly char ds = Path.DirectorySeparatorChar;
    //readonly ElasticsearchClient esClient;

    public TagsController(WebComponents_DbContext _webComponentsDb, UserManager<Identity_UserModel> _userManager,
    IWebHostEnvironment _env/*, ElasticsearchClient esClient*/)
    {
        webComponentsDb = _webComponentsDb;
        userManager = _userManager;
        env = _env;
        //this.esClient = esClient;
    }


    public async Task<IActionResult> TagList()
    {
        List<string> tagList = (await webComponentsDb.Tags.ToListAsync()).Select(t => t.Name).ToList();
        return View(tagList);
    }


    public async Task<IActionResult> SubmitNewTag(string tagname)
    {
        tagname = tagname.Replace(" ", "_").ToLower().Trim();

        if (await webComponentsDb.Tags.FirstOrDefaultAsync(t => t.Name == tagname) is null)
        {
            WebComponents_TagDbModel tagDbModel = new()
            {
                Name = tagname
            };
            await webComponentsDb.Tags.AddAsync(tagDbModel);
            await webComponentsDb.SaveChangesAsync();
        }
        return RedirectToAction(nameof(TagList));
    }


    public async Task<IActionResult> DeleteTag(string tagname)
    {
        WebComponents_TagDbModel? tagDbModel = await webComponentsDb.Tags.FirstOrDefaultAsync(t => t.Name == tagname);
        if (tagDbModel is not null)
        {
            webComponentsDb.Tags.Remove(tagDbModel);
            await webComponentsDb.SaveChangesAsync();
        }

        return RedirectToAction(nameof(TagList));
    }

    //******************************* Backup *********************************

    public IActionResult Backup()
    {
        string backupDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}Tags{ds}Backup";
        string backupZipFilePath = $"{backupDirectory}{ds}backup.zip";
        if (System.IO.File.Exists(backupZipFilePath))
        {
            ViewBag.BackupDate = System.IO.File.GetCreationTime(backupZipFilePath);
        }

        ViewBag.ControllerName = "Tags";
        return View();
    }

    public async Task<IActionResult> RenewBackup()
    {
        string mainDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}Tags{ds}Backup";
        if (!Directory.Exists(mainDirectoryPath))
        {
            Directory.CreateDirectory(mainDirectoryPath);
        }

        string dataDirectoryPath = $"{mainDirectoryPath}{ds}Data";
        if (Directory.Exists(dataDirectoryPath))
        {
            Directory.Delete(dataDirectoryPath, true);
        }
        Directory.CreateDirectory(dataDirectoryPath);

        string backupZipFilePath = $"{mainDirectoryPath}{ds}backup.zip";
        if (System.IO.File.Exists(backupZipFilePath))
        {
            System.IO.File.Delete(backupZipFilePath);
        }

        List<string> model = await webComponentsDb.Tags.Select(t => t.Name).ToListAsync();
        string json = JsonSerializer.Serialize(model);
        string jsonFilePath = dataDirectoryPath + ds + "tagsList.json";
        await System.IO.File.WriteAllTextAsync(jsonFilePath, json);

        ZipFile.CreateFromDirectory(dataDirectoryPath, backupZipFilePath);

        return RedirectToAction(nameof(Backup));
    }

    public IActionResult DownloadBackup()
    {
        string backupDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}Tags{ds}Backup";
        if (Directory.Exists(backupDirectory))
        {
            string backupZipFilePath = $"{backupDirectory}{ds}backup.zip";
            if (System.IO.File.Exists(backupZipFilePath))
            {
                return PhysicalFile(backupZipFilePath, "Application/zip");
            }
        }
        object o = "backup file Not found!";
        ViewBag.ResultState = "danger";
        return View("Result", o);
    }

    public IActionResult DeleteBackup()
    {
        string backupDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}Tags{ds}Backup";
        if (Directory.Exists(backupDirectory))
        {
            string backupDataDirectoryPath = $"{backupDirectory}{ds}Data";
            if (Directory.Exists(backupDataDirectoryPath))
            {
                Directory.Delete(backupDataDirectoryPath, true);
            }

            string backupZipFilePath = $"{backupDirectory}{ds}backup.zip";
            if (System.IO.File.Exists(backupZipFilePath))
            {
                System.IO.File.Delete(backupZipFilePath);
            }
        }

        return RedirectToAction(nameof(Backup));
    }

    [HttpPost]
    public async Task<IActionResult> UploadBackup(IFormFile backupZipFile)
    {
        if (ModelState.IsValid)
        {
            string mainDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}Tags{ds}Backup";
            if (!Directory.Exists(mainDirectoryPath))
            {
                Directory.CreateDirectory(mainDirectoryPath);
            }

            string dataDirectoryPath = $"{mainDirectoryPath}{ds}Data";
            if (Directory.Exists(dataDirectoryPath))
            {
                Directory.Delete(dataDirectoryPath, true);
            }
            Directory.CreateDirectory(dataDirectoryPath);

            string backupZipFilePath = $"{mainDirectoryPath}{ds}backup.zip";
            if (System.IO.File.Exists(backupZipFilePath))
            {
                System.IO.File.Delete(backupZipFilePath);
            }

            using (FileStream fs = System.IO.File.Create(backupZipFilePath))
            {
                await backupZipFile.CopyToAsync(fs);
            }

            ZipFile.ExtractToDirectory(backupZipFilePath, dataDirectoryPath);
        }
        return RedirectToAction(nameof(Backup));
    }

    public async Task<IActionResult> RecoverLastBackup()
    {
        string mainDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}Tags{ds}Backup";
        string dataDirectoryPath = $"{mainDirectoryPath}{ds}Data";
        string jsonFilePath = dataDirectoryPath + ds + "tagsList.json";
        string json = await System.IO.File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8);
        List<string>? model = JsonSerializer.Deserialize<List<string>>(json);
        if (model is null)
        {
            ViewBag.ResultState = "info";
            object o1 = $"Backup file was empty!";
            return View("Result", o1);
        }
        foreach (string tag in model)
        {
            WebComponents_TagDbModel? tagDbModel = await webComponentsDb.Tags.FirstOrDefaultAsync(t => t.Name == tag);
            if (tagDbModel is null)
            {
                tagDbModel = new() { Name = tag };
                await webComponentsDb.Tags.AddAsync(tagDbModel);
            }
        }
        await webComponentsDb.SaveChangesAsync();

        ViewBag.ResultState = "success";
        object o = $"Tags recovery completed successfully.";
        return View("Result", o);
    }

    //************************************************************************

}