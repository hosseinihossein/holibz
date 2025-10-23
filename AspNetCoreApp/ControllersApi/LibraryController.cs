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
    public async Task<IActionResult> List([FromQuery] string userGuid)
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
                HasImage = System.IO.File.Exists(libraryImagePath),
                CreatedAt = libraryInfo.CreatedAt,
            };

            cardModelList.Add(libCard);
        }

        return Ok(cardModelList.ToArray());
    }

    [HttpGet]
    public async Task<IActionResult> LibraryModel([FromQuery] string libraryGuid)
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
            HasImage = System.IO.File.Exists(libraryImagePath),
            CreatedAt = libraryInfo.CreatedAt,
        };

        return Ok(libModel);
    }

    [HttpGet]
    public async Task<IActionResult> ShelfList([FromQuery] string userGuid)
    {
        Identity_UserDbModel? owner =
        await userManager.Users.FirstOrDefaultAsync(user => user.UserGuid == userGuid);
        if (owner is null)
        {
            ModelState.AddModelError("userGuid", "Couldn't find the owner!");
            return BadRequest(ModelState);
        }

        var shelvesInfo = await libraryDb.Shelves
        .Include(shelf => shelf.Documents)
        .Where(shelf => shelf.OwnerGuid == userGuid)
        .Select(shelf => new
        {
            shelf.CreatedAt,
            shelf.Description,
            /*DocumentsBriefs = shelf.Documents.Select(doc => new Library_DocumentMiniModel()
            {
                Title = doc.Title,
                Guid = doc.Guid,
                Decsription = doc.Description,
                //HasImg = System.IO.File.Exists(Path.Combine(Storage_Document.FullName, "Images", doc.Guid))
            }),*/
            shelf.Guid,
            LibraryTitle = shelf.Library.Title,
            shelf.Title,
        })
        .ToListAsync();


        List<Library_ShelfModel> shelfModels = [];
        foreach (var shelfInfo in shelvesInfo)
        {
            /*foreach (var doc in shelfInfo.DocumentsBriefs)
            {
                doc.HasImg = System.IO.File.Exists(Path.Combine(Storage_Document.FullName, "Images", doc.Guid));
            }*/
            Library_ShelfModel shelfModel = new()
            {
                CreatedAt = shelfInfo.CreatedAt,
                Description = shelfInfo.Description,
                //DocumentsBriefs = shelfInfo.DocumentsBriefs.ToArray(),
                Guid = shelfInfo.Guid,
                LibraryTitle = shelfInfo.LibraryTitle,
                Title = shelfInfo.Title,
                OwnerGuid = owner.UserGuid!,
            };
            shelfModels.Add(shelfModel);
        }

        return Ok(shelfModels);
    }




    [HttpGet]
    public async Task<IActionResult> TotalNumberOfDocuments([FromQuery] string userGuid)
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