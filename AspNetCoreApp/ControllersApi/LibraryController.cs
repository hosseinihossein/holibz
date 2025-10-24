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
        Identity_UserDbModel? owner =
        await userManager.Users.FirstOrDefaultAsync(user => user.UserGuid == userGuid);
        if (owner is null)
        {
            ModelState.AddModelError("userGuid", "Couldn't find the owner!");
            return BadRequest(ModelState);
        }

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
        })
        .ToListAsync();

        List<Library_LibraryModel> cardModelList = [];
        foreach (var libraryInfo in librariesInfo)
        {
            string libraryImagePath =
            Path.Combine(Storage_Library.FullName, "Images", libraryInfo.Guid);

            Library_LibraryModel libCard = new()
            {
                Guid = libraryInfo.Guid,
                Title = libraryInfo.Title,
                Description = libraryInfo.Description,
                ShelvesTitles = libraryInfo.ShelvesTitles.ToArray(),
                OwnerUsername = owner.UserName!,
                OwnerGuid = owner.UserGuid,
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

        Identity_UserDbModel? owner =
        await userManager.Users.FirstOrDefaultAsync(user => user.UserGuid == libraryInfo.OwnerGuid);
        if (owner is null)
        {
            ModelState.AddModelError("userGuid", "Couldn't find the owner!");
            return BadRequest(ModelState);
        }

        string libraryImagePath =
            Path.Combine(Storage_Library.FullName, "Images", libraryInfo.Guid);

        Library_LibraryModel libModel = new()
        {
            Guid = libraryInfo.Guid,
            Title = libraryInfo.Title,
            Description = libraryInfo.Description,
            ShelvesTitles = libraryInfo.ShelvesTitles.ToArray(),
            OwnerUsername = owner.UserName!,
            OwnerGuid = libraryInfo.OwnerGuid,
            HasImage = System.IO.File.Exists(libraryImagePath),
            CreatedAt = libraryInfo.CreatedAt,
        };

        return Ok(libModel);
    }

    [HttpGet]
    public async Task<IActionResult> ShelfList([FromQuery][StringLength(32)] string libraryGuid)
    {
        var shelvesInfo = await libraryDb.Shelves
        .Include(shelf => shelf.Library)
        .Include(shelf => shelf.Documents)
        .Where(shelf => shelf.Library.Guid == libraryGuid)
        .Select(shelf => new Library_ShelfModel()
        {
            /*shelf.CreatedAt,
            shelf.Description,
            shelf.Guid,
            LibraryTitle = shelf.Library.Title,
            shelf.Title,
            shelf.OwnerGuid,
            DocumentsGuids = shelf.Documents.Select(doc => doc.Guid),*/
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            DocumentsGuids = shelf.Documents.Select(doc => doc.Guid).ToArray(),
            Guid = shelf.Guid,
            LibraryTitle = shelf.Library.Title,
            Title = shelf.Title,
            OwnerGuid = shelf.OwnerGuid,
        })
        .AsSplitQuery()
        .ToArrayAsync();

        /*List<Library_ShelfModel> shelfModels = [];
        foreach (var shelfInfo in shelvesInfo)
        {
            Library_ShelfModel shelfModel = new()
            {
                CreatedAt = shelfInfo.CreatedAt,
                Description = shelfInfo.Description,
                DocumentsGuids = shelfInfo.DocumentsGuids.ToArray(),
                Guid = shelfInfo.Guid,
                LibraryTitle = shelfInfo.LibraryTitle,
                Title = shelfInfo.Title,
                OwnerGuid = shelfInfo.OwnerGuid,
            };
            shelfModels.Add(shelfModel);
        }
        return Ok(shelfModels);*/
        return Ok(shelvesInfo);
    }

    /*[HttpGet]
    public async Task<IActionResult> DocumentList([FromQuery][StringLength(32)] string shelfGuid)
    {

    }*/

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