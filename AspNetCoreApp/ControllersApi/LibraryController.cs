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
    readonly DirectoryInfo Storage_Element;
    readonly Library_Process libraryProcess;




    public LibraryController(Library_DbContext _libraryDb, UserManager<Identity_UserDbModel> _userManager,
    Library_Process _libraryProcess)
    {
        libraryDb = _libraryDb;
        userManager = _userManager;
        Storage_Library = _libraryProcess.Storage_Library;
        Storage_Shelf = _libraryProcess.Storage_Shelf;
        Storage_Document = _libraryProcess.Storage_Document;
        Storage_Element = _libraryProcess.Storage_Element;
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
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.OwnerGuid,
                Title = doc.Title,
            }).ToArray(),
            Guid = shelf.Guid,
            Library = new Library_LibraryBrief()
            {
                Guid = shelf.Library.Guid,
                Title = shelf.Library.Title,
            },
            Title = shelf.Title,
            OwnerGuid = shelf.OwnerGuid,
            TotalNumberOfShelfDocuments = shelf.Documents.Count,
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
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.OwnerGuid,
                Title = doc.Title,
            }).ToArray(),
            Guid = shelf.Guid,
            Library = new Library_LibraryBrief()
            {
                Guid = shelf.Library.Guid,
                Title = shelf.Library.Title,
            },
            Title = shelf.Title,
            OwnerGuid = shelf.OwnerGuid,
            TotalNumberOfShelfDocuments = shelf.Documents.Count,
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
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
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
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
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
    public async Task<IActionResult> DocumentPageModel([FromQuery][StringLength(32)] string documentGuid)
    {
        var customDocumentModel = await libraryDb.Documents
        .Include(doc => doc.Shelves)
        .Include(doc => doc.Elements)
        .Include(doc => doc.Tags)
        .Include(doc => doc.RelatedVersions)
        .Where(doc => doc.Guid == documentGuid)
        .Select(doc => new
        {
            doc.Guid,
            doc.OwnerGuid,
            doc.Title,
            doc.Description,
            doc.Version,
            doc.RelatedVersions,
            ShelvesGuids = doc.Shelves.Select(shelf => shelf.Guid),
            doc.Elements,
            Tags = doc.Tags.Select(tag => tag.Name),
            doc.CreatedAt,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (customDocumentModel is null)
        {
            return NotFound($"Theres no document with id '{documentGuid}' in Db!");
        }

        Library_VersionBrief[]? versionBriefs = [];
        if (customDocumentModel.RelatedVersions is not null)
        {
            versionBriefs = await libraryDb.RelatedVersions
            .Include(rv => rv.Documents)
            .Where(rv => rv.Guid == customDocumentModel.RelatedVersions.Guid)
            .Select(rv => rv.Documents
                .Select(doc => new Library_VersionBrief()
                {
                    DocumentGuid = doc.Guid,
                    VersionName = doc.Version,
                })
                .ToArray()
            )
            .FirstOrDefaultAsync();
        }
        if (versionBriefs is null || versionBriefs.Length == 0)
        {
            versionBriefs = [new Library_VersionBrief()
            {
                DocumentGuid = customDocumentModel.Guid,
                VersionName = customDocumentModel.Version,
            }];
        }

        Library_ShelfBrief[] shelfBriefs = await libraryDb.Shelves
        .Include(shelf => shelf.Documents)
        .Include(shelf => shelf.Library)
        .Where(shelf => customDocumentModel.ShelvesGuids.Contains(shelf.Guid))
        .Select(shelf => new Library_ShelfBrief()
        {
            //Description = shelf.Description,
            LibraryTitle = shelf.Library.Title,
            Owner = new Library_OwnerBrief() { UserGuid = shelf.OwnerGuid },
            Guid = shelf.Guid,
            Title = shelf.Title,
            Documents = shelf.Documents.Select(doc => new Library_DocumentBrief()
            {
                //Description = doc.Description,
                Guid = doc.Guid,
                Title = doc.Title,
            }).ToArray(),
        })
        .AsSplitQuery()
        .ToArrayAsync();

        foreach (var shelfBrief in shelfBriefs)
        {
            shelfBrief.Owner.UserName = await userManager.Users
            .Where(u => u.UserGuid == shelfBrief.Owner.UserGuid)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync() ?? "_";
        }

        Library_DocumentPageModel documentPageModel = new()
        {
            CreatedAt = customDocumentModel.CreatedAt,
            Description = customDocumentModel.Description,
            Elements = customDocumentModel.Elements.Select(elementDbModel => new Library_ElementModel()
            {
                Guid = elementDbModel.Guid,
                Order = elementDbModel.Order,
                OwnerGuid = elementDbModel.OwnerGuid,
                Title = elementDbModel.Title,
                Type = elementDbModel.Type,
                UpdatedAt = elementDbModel.UpdatedAt,
                Value = elementDbModel.Value ??
                    $"/api/Library/ElementFile?elementGuid={elementDbModel.Guid}&elementFileName={elementDbModel.FileName}",
            }).ToArray(),
            Guid = customDocumentModel.Guid,
            Owner = new Library_OwnerBrief()
            {
                UserGuid = customDocumentModel.OwnerGuid,
                UserName = await userManager.Users.Where(u => u.UserGuid == customDocumentModel.OwnerGuid)
                .Select(u => u.UserName).FirstOrDefaultAsync() ?? "_",
            },
            RelatedVersions = versionBriefs,
            Shelves = shelfBriefs,
            Tags = customDocumentModel.Tags.ToArray(),
            Title = customDocumentModel.Title,
            Version = customDocumentModel.Version,
            HasImage = System.IO.File.Exists(Path
                .Combine(Storage_Document.FullName, customDocumentModel.Guid, "image")
            ),
        };

        return Ok(documentPageModel);
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument([FromQuery][StringLength(32)] string documentGuid)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .FirstOrDefaultAsync(doc => doc.Guid == documentGuid);
        if (documentDbModel is null)
        {
            return NotFound($"There's no document with guid '{documentGuid}'!");
        }

        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        if (user.UserGuid != documentDbModel.OwnerGuid)
        {
            ModelState.AddModelError("Authorization", "You're Not allowed to delete this document!");
            return BadRequest(ModelState);
        }

        //delete directory path from Storage_Document
        string directoryPath = Path.Combine(Storage_Document.FullName, documentDbModel.Guid);
        try
        {
            System.IO.Directory.Delete(directoryPath, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"\n***** {e.Message}");
        }

        //remove from Db
        libraryDb.Documents.Remove(documentDbModel);
        await libraryDb.SaveChangesAsync();

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> ElementFile([FromQuery][StringLength(32)] string elementGuid,
    [FromQuery][StringLength(50)] string? elementFileName)
    {
        if (string.IsNullOrWhiteSpace(elementFileName))
        {
            elementFileName = await libraryDb.Elements
            .Where(el => el.Guid == elementGuid)
            .Select(el => el.FileName)
            .FirstOrDefaultAsync();

            if (elementFileName is null)
            {
                return NotFound("There's no element file name with the specified guid on Db!");
            }
        }

        string filePath = Path.Combine(Storage_Element.FullName, elementGuid, elementFileName);
        if (System.IO.File.Exists(filePath))
        {
            return PhysicalFile(filePath, "application/octet-stream", elementFileName, true);
        }
        return NotFound("The element file Not found!");
    }

    [HttpPost]
    public async Task<IActionResult> EditElements([FromBody] Library_EditElementFormModel[] formModels)
    {
        if (ModelState.IsValid)
        {
            IEnumerable<string> elementGuids = formModels.Select(m => m.Guid);

            List<Library_ElementDbModel> elementDbModels = await libraryDb.Elements
            .Where(el => elementGuids.Contains(el.Guid))
            .ToListAsync();

            foreach (var elementDbModel in elementDbModels)
            {
                var formModel = formModels.FirstOrDefault(fm => fm.Guid == elementDbModel.Guid);
                if (formModel is not null)
                {
                    elementDbModel.Order = formModel.Order;
                    elementDbModel.Title = formModel.Title ?? elementDbModel.Title;
                    elementDbModel.Value = formModel.Value ?? elementDbModel.Value;
                }
            }

            await libraryDb.SaveChangesAsync();

            List<Library_ElementDbModel> allDocumentElements = (await libraryDb.Elements
            .Include(el => el.Document)
            .ThenInclude(doc => doc.Elements)
            .Where(el => el.Id == elementDbModels.First().Id)
            .Select(el => el.Document.Elements)
            .FirstOrDefaultAsync())!;

            Library_ElementModel[] elementModelArray = allDocumentElements
            .Select(elementDbModel => new Library_ElementModel()
            {
                Guid = elementDbModel.Guid,
                Order = elementDbModel.Order,
                OwnerGuid = elementDbModel.OwnerGuid,
                Title = elementDbModel.Title,
                Type = elementDbModel.Type,
                UpdatedAt = elementDbModel.UpdatedAt,
                Value = elementDbModel.Value ??
                    $"/api/Library/ElementFile?elementGuid={elementDbModel.Guid}&elementFileName={elementDbModel.FileName}",
            })
            .ToArray();

            return Ok(new { success = true, elements = elementModelArray });
        }

        return BadRequest(ModelState);
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

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewElement(Library_NewElementFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.UserName == User.Identity!.Name!)
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewElement(libraryDb, userGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_ElementDbModel elementDbModel = (Library_ElementDbModel)result.ResultObject;

                var elementModel = new Library_ElementModel()
                {
                    Guid = elementDbModel.Guid,
                    Order = elementDbModel.Order,
                    OwnerGuid = elementDbModel.OwnerGuid,
                    Title = elementDbModel.Title,
                    Type = elementDbModel.Type,
                    UpdatedAt = elementDbModel.UpdatedAt,
                    Value = elementDbModel.Value ??
                    $"/api/Library/ElementFile?elementGuid={elementDbModel.Guid}&elementFileName={elementDbModel.FileName}",
                };

                return Ok(elementModel);
            }

            ModelState.AddModelError(result.ErrorTitle ?? "New Element Error", result.ErrorDescription ?? "Error Description");
            return BadRequest(ModelState);
        }
        return BadRequest(ModelState);
    }

}