using System.ComponentModel.DataAnnotations;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class LibraryController : ControllerBase
{
    readonly Library_DbContext libraryDb;
    readonly UserManager<Identity_UserDbModel> userManager;
    readonly DirectoryInfo Storage_Library;
    readonly DirectoryInfo Storage_Shelf;
    readonly DirectoryInfo Storage_Document;





    public LibraryController(Library_DbContext _libraryDb, UserManager<Identity_UserDbModel> _userManager,
    IWebHostEnvironment _env)
    {
        libraryDb = _libraryDb;
        userManager = _userManager;
        Storage_Library = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Library"));
        Storage_Shelf = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Shelf"));
        Storage_Document = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Document"));
    }





    [HttpGet]
    public async Task<IActionResult> List([FromQuery][StringLength(32)] string userGuid)
    {
        var librariesInfo = await libraryDb.Libraries
        .Include(lib => lib.Shelves)
        .Where(lib => lib.OwnerGuid == userGuid)
        .Select(lib => new
        {
            lib.Guid,
            lib.Title,
            lib.Description,
            ShelvesTitles = lib.Shelves.Select(shelf => shelf.Title),
            lib.CreatedAt,
            lib.OwnerGuid,
        })
        .ToListAsync();

        List<Library_LibraryCardModel> cardModelList = [];
        foreach (var libraryInfo in librariesInfo)
        {
            string libraryImagePath =
            Path.Combine(Storage_Library.FullName, "Images", libraryInfo.Guid);

            Library_LibraryCardModel libCard = new()
            {
                Guid = libraryInfo.Guid,
                Title = libraryInfo.Title,
                Description = libraryInfo.Description,
                ShelvesTitles = libraryInfo.ShelvesTitles.ToArray(),
                //OwnerUsername = owner.UserName!,
                OwnerGuid = libraryInfo.OwnerGuid,
                HasImage = System.IO.File.Exists(libraryImagePath),
                CreatedAt = libraryInfo.CreatedAt,
            };

            cardModelList.Add(libCard);
        }

        return Ok(cardModelList.ToArray());
    }

    [HttpGet]
    public async Task<IActionResult> LibraryModel([FromQuery][StringLength(32)] string libraryGuid)
    {
        var libraryInfo = await libraryDb.Libraries
        .Include(lib => lib.Shelves)
        .Where(lib => lib.Guid == libraryGuid)
        .Select(lib => new
        {
            lib.Guid,
            lib.Title,
            lib.Description,
            ShelvesTitles = lib.Shelves.Select(shelf => shelf.Title),
            lib.OwnerGuid,
            lib.CreatedAt,
        })
        .FirstOrDefaultAsync();

        if (libraryInfo is null)
        {
            return NotFound();
        }

        /*Identity_UserDbModel? owner =
        await userManager.Users.FirstOrDefaultAsync(user => user.UserGuid == libraryInfo.OwnerGuid);
        if (owner is null)
        {
            ModelState.AddModelError("userGuid", "Couldn't find the owner!");
            return BadRequest(ModelState);
        }*/

        string libraryImagePath =
            Path.Combine(Storage_Library.FullName, "Images", libraryInfo.Guid);

        Library_LibraryCardModel libModel = new()
        {
            Guid = libraryInfo.Guid,
            Title = libraryInfo.Title,
            Description = libraryInfo.Description,
            ShelvesTitles = libraryInfo.ShelvesTitles.ToArray(),
            //OwnerUsername = owner.UserName!,
            OwnerGuid = libraryInfo.OwnerGuid,
            HasImage = System.IO.File.Exists(libraryImagePath),
            CreatedAt = libraryInfo.CreatedAt,
        };

        return Ok(libModel);
    }

    [HttpGet]
    public async Task<IActionResult> ShelfList([FromQuery][StringLength(32)] string libraryGuid)
    {
        var shelfCardModels = await libraryDb.Shelves
        .Include(shelf => shelf.Library)
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Elements)
        .Where(shelf => shelf.Library.Guid == libraryGuid)
        .Select(shelf => new Library_ShelfCardModel()
        {
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            DocumentCardModels = shelf.Documents
            .OrderByDescending(doc => doc.CreatedAt)
            .Take(10)
            .Select(doc => new Library_DocumentCardModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value).ToArray(),
                OwnerGuid = doc.OwnerGuid,
                Title = doc.Title,
            }).ToArray(),
            Guid = shelf.Guid,
            LibraryTitle = shelf.Library.Title,
            Title = shelf.Title,
            OwnerGuid = shelf.OwnerGuid,
        })
        .AsSplitQuery()
        .ToArrayAsync();

        foreach (var shelfCardModel in shelfCardModels)
        {
            foreach (var documentCardModel in shelfCardModel.DocumentCardModels)
            {
                documentCardModel.HasImage =
                System.IO.File.Exists(Path.Combine(Storage_Document.FullName, "Images", documentCardModel.Guid));
            }
        }

        return Ok(shelfCardModels);
    }

    /*[HttpGet]
    public async Task<IActionResult> ShelfModel([FromQuery][StringLength(32)] string shelfGuid)
    {
        
    }*/

    [HttpGet]
    public async Task<IActionResult> DocumentCardList([FromQuery][StringLength(32)] string shelfGuid)
    {
        Library_DocumentCardModel[] documentCardModels = await libraryDb.Shelves
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Elements)
        .Where(shelf => shelf.Guid == shelfGuid)
        .SelectMany(shelf => shelf.Documents)
        .Select(doc => new Library_DocumentCardModel()
        {
            Description = doc.Description,
            Guid = doc.Guid,
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value).ToArray(),
            OwnerGuid = doc.OwnerGuid,
            Title = doc.Title,
        })
        //.AsSplitQuery()//we dont need assplitquery because include and theninclide are not in the same level
        .ToArrayAsync();

        foreach (var documentCardModel in documentCardModels)
        {
            documentCardModel.HasImage =
            System.IO.File.Exists(Path.Combine(Storage_Document.FullName, "Images", documentCardModel.Guid));
        }

        return Ok(documentCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> DocumentCardModel([FromQuery][StringLength(32)] string documentGuid)
    {
        Library_DocumentCardModel? documentCardModel = await libraryDb.Documents
        .Include(doc => doc.Elements)
        .Where(doc => doc.Guid == documentGuid)
        .Select(doc => new Library_DocumentCardModel()
        {
            Guid = doc.Guid,
            Description = doc.Description,
            Title = doc.Title,
            OwnerGuid = doc.OwnerGuid,
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value).ToArray(),
        })
        .FirstOrDefaultAsync();

        if (documentCardModel is null)
        {
            return NotFound();
        }

        documentCardModel.HasImage =
        System.IO.File.Exists(Path.Combine(Storage_Document.FullName, "Images", documentCardModel.Guid));

        return Ok(documentCardModel);
    }





    [HttpGet]
    public async Task<IActionResult> TotalNumberOfDocuments([FromQuery][StringLength(32)] string userGuid)
    {
        Identity_UserDbModel? owner =
        await userManager.Users.FirstOrDefaultAsync(user => user.UserGuid == userGuid);
        if (owner is null)
        {
            ModelState.AddModelError("userGuid", "Couldn't find the owner!");
            return BadRequest(ModelState);
        }

        int totalNumberOfUserDocuments = await libraryDb.Documents
        .Where(doc => doc.OwnerGuid == owner.UserGuid)
        .CountAsync();

        return Ok(new { totalNumberOfUserDocuments });
    }






}