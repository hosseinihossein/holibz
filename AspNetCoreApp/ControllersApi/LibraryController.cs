using System.ComponentModel.DataAnnotations;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    readonly Library_Process libraryProcess;




    public LibraryController(Library_DbContext _libraryDb, UserManager<Identity_UserDbModel> _userManager,
    Library_Process _libraryProcess)
    {
        libraryDb = _libraryDb;
        userManager = _userManager;
        Storage_Library = _libraryProcess.Storage_Library;
        Storage_Shelf = _libraryProcess.Storage_Shelf;
        Storage_Document = _libraryProcess.Storage_Document;
        libraryProcess = _libraryProcess;
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
            Path.Combine(Storage_Library.FullName, libraryInfo.Guid, "image");

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
            Path.Combine(Storage_Library.FullName, libraryInfo.Guid, "image");

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
    public IActionResult LibraryImage([FromQuery][StringLength(32)] string libraryGuid)
    {
        string imagePath = Path.Combine(Storage_Library.FullName, libraryGuid, "image");
        if (System.IO.File.Exists(imagePath))
        {
            return PhysicalFile(imagePath, "application/octet-stream", "libraryImage", true);
        }
        return NotFound("Library Image Not Found!");
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
            string shelfImagePath =
            Path.Combine(Storage_Shelf.FullName, shelfCardModel.Guid, "image");
            shelfCardModel.HasImage = System.IO.File.Exists(shelfImagePath);

            foreach (var documentCardModel in shelfCardModel.DocumentCardModels)
            {
                documentCardModel.HasImage =
                System.IO.File.Exists(Path.Combine(Storage_Document.FullName, documentCardModel.Guid, "image"));
            }
        }

        return Ok(shelfCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> ShelfModel([FromQuery][StringLength(32)] string shelfGuid)
    {
        var shelfCardModel = await libraryDb.Shelves
        .Include(shelf => shelf.Library)
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Elements)
        .Where(shelf => shelf.Guid == shelfGuid)
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
        .FirstOrDefaultAsync();

        if (shelfCardModel is null)
        {
            return NotFound();
        }

        string shelfImagePath =
            Path.Combine(Storage_Shelf.FullName, shelfCardModel.Guid, "image");
        shelfCardModel.HasImage = System.IO.File.Exists(shelfImagePath);

        foreach (var documentCardModel in shelfCardModel.DocumentCardModels)
        {
            documentCardModel.HasImage =
            System.IO.File.Exists(Path.Combine(Storage_Document.FullName, documentCardModel.Guid, "image"));
        }

        return Ok(shelfCardModel);
    }

    [HttpGet]
    public IActionResult ShelfImage([FromQuery][StringLength(32)] string shelfGuid)
    {
        string imagePath = Path.Combine(Storage_Shelf.FullName, shelfGuid, "image");
        if (System.IO.File.Exists(imagePath))
        {
            return PhysicalFile(imagePath, "application/octet-stream", "shelfImage", true);
        }
        return NotFound("Shelf Image Not Found!");
    }

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
            System.IO.File.Exists(Path.Combine(Storage_Document.FullName, documentCardModel.Guid, "image"));
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
        System.IO.File.Exists(Path.Combine(Storage_Document.FullName, documentCardModel.Guid, "image"));

        return Ok(documentCardModel);
    }

    [HttpGet]
    public IActionResult DocumentImage([FromQuery][StringLength(32)] string documentGuid)
    {
        string imagePath = Path.Combine(Storage_Document.FullName, documentGuid, "image");
        if (System.IO.File.Exists(imagePath))
        {
            return PhysicalFile(imagePath, "application/octet-stream", "documentImage", true);
        }
        return NotFound("Document Image Not Found!");
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





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewLibrary(Library_NewLibraryFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.UserName == User.Identity!.Name!)
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewLibrary(libraryDb, userGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_LibraryDbModel libraryDbModel = (Library_LibraryDbModel)result.ResultObject;
                return Ok(new { success = true, libraryGuid = libraryDbModel.Guid });
            }

            ModelState.AddModelError(result.ErrorTitle ?? "New Library Error", result.ErrorDescription ?? "Error Description");
            return BadRequest(ModelState);

        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewShelf(Library_NewShelfFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.UserName == User.Identity!.Name!)
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewShelf(libraryDb, userGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_ShelfDbModel shelfDbModel = (Library_ShelfDbModel)result.ResultObject;
                return Ok(new { success = true, shelfGuid = shelfDbModel.Guid });
            }

            ModelState.AddModelError(result.ErrorTitle ?? "New Shelf Error", result.ErrorDescription ?? "Error Description");
            return BadRequest(ModelState);

        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewDocument(Library_NewDocumentFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.UserName == User.Identity!.Name!)
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewDocument(libraryDb, userGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_DocumentDbModel documentDbModel = (Library_DocumentDbModel)result.ResultObject;
                return Ok(new { success = true, documentGuid = documentDbModel.Guid });
            }

            ModelState.AddModelError(result.ErrorTitle ?? "New Document Error", result.ErrorDescription ?? "Error Description");
            return BadRequest(ModelState);

        }
        return BadRequest(ModelState);
    }


}