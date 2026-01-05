using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.ControllersApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class LibraryController : ControllerBase
{
    readonly Library_DbContext libraryDb;
    readonly UserManager<Identity_UserDbModel> userManager;
    readonly DirectoryInfo Storage_Libraries;
    readonly DirectoryInfo Storage_Shelves;
    readonly DirectoryInfo Storage_Documents;
    readonly DirectoryInfo Storage_Elements;
    readonly Library_Process libraryProcess;




    public LibraryController(Library_DbContext _libraryDb, UserManager<Identity_UserDbModel> _userManager,
    Library_Process _libraryProcess)
    {
        libraryDb = _libraryDb;
        userManager = _userManager;
        Storage_Libraries = _libraryProcess.Storage_Libraries;
        Storage_Shelves = _libraryProcess.Storage_Shelves;
        Storage_Documents = _libraryProcess.Storage_Documents;
        Storage_Elements = _libraryProcess.Storage_Elements;
        libraryProcess = _libraryProcess;
    }





    [HttpGet]
    public async Task<IActionResult> List([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_LibraryCard_ViewModel[] libraryCardModels = await libraryDb.Owners
        .Where(owner => owner.Guid == ownerGuid_Guid)
        .SelectMany(owner => owner.Libraries)
        .Select(lib => new Library_LibraryCard_ViewModel()
        {
            Guid = lib.Guid,
            Title = lib.Title,
            Description = lib.Description,
            ShelvesTitles = lib.Shelves.
                Select(ls => ls.Shelf)
                .OrderBy(shelf => shelf.Id)
                .Take(10)
                .Select(shelf => shelf.Title)
                .ToArray(),
            CreatedAt = lib.CreatedAt,
            OwnerGuid = lib.Owner.Guid,
            IntegrityVersion = lib.IntegrityVersion,
            HasImage = lib.HasImage,
            IsDefault = lib.Guid == lib.Owner.DefaultLibraryGuid,
        })
        .AsSplitQuery()
        .ToArrayAsync();

        return Ok(libraryCardModels);
    }
    [HttpGet]
    public async Task<IActionResult> BriefList([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_LibraryBrief_ViewModel[] libraryBrifModels = await libraryDb.Owners
        .Where(owner => owner.Guid == ownerGuid_Guid)
        .SelectMany(owner => owner.Libraries)
        .Select(lib => new Library_LibraryBrief_ViewModel()
        {
            Guid = lib.Guid,
            Title = lib.Title,
        })
        .ToArrayAsync();

        return Ok(libraryBrifModels);
    }

    [HttpGet]
    public async Task<IActionResult> LibrariesGuids([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] userLibrariesGuids = await libraryDb.Owners
        .Where(o => o.Guid == ownerGuid_Guid)
        .SelectMany(o => o.Libraries)
        .Select(lib => lib.Guid)
        .ToArrayAsync();

        return Ok(userLibrariesGuids);
    }

    [HttpGet]
    public async Task<IActionResult> LibraryModel([FromQuery][StringLength(32)] string libraryGuid)
    {
        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_LibraryCard_ViewModel? libraryCardModel = await libraryDb.Libraries
        .Where(lib => lib.Guid == libraryGuid_Guid)
        .Select(lib => new Library_LibraryCard_ViewModel()
        {
            Guid = lib.Guid,
            Title = lib.Title,
            Description = lib.Description,
            ShelvesTitles = lib.Shelves
                .Select(ls => ls.Shelf)
                .OrderBy(shelf => shelf.Id)
                .Take(10)
                .Select(shelf => shelf.Title)
                .ToArray(),
            OwnerGuid = lib.Owner.Guid,
            CreatedAt = lib.CreatedAt,
            IntegrityVersion = lib.IntegrityVersion,
            HasImage = lib.HasImage,
            IsDefault = lib.Guid == lib.Owner.DefaultLibraryGuid,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (libraryCardModel is null)
        {
            return NotFound();
        }

        return Ok(libraryCardModel);
    }

    [HttpGet]
    public IActionResult LibraryImage([FromQuery][StringLength(32)] string libraryGuid)
    {
        string imagePath = Path.Combine(Storage_Libraries.FullName, libraryGuid, "image");
        if (System.IO.File.Exists(imagePath))
        {
            return PhysicalFile(imagePath, "application/octet-stream", "libraryImage", true);
        }
        return NotFound("Library Image Not Found!");
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLibrary([FromQuery][StringLength(32)] string libraryGuid)
    {
        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var libraryInfo = await libraryDb.Libraries
        .Where(lib => lib.Guid == libraryGuid_Guid)
        .Select(lib => new
        {
            id = lib.Id,
            ownerGuid = lib.Owner.Guid,
            isDefault = lib.Guid == lib.Owner.DefaultLibraryGuid,
            ownerDefaultLibraryId = lib.Owner.Libraries
                .Where(ownerLibs => ownerLibs.Guid == lib.Owner.DefaultLibraryGuid)
                .Select(ownerLibs => ownerLibs.Id)
                .First(),
            shelvesInfo = lib.Shelves
                .Select(ls => ls.Shelf)
                .Select(shelf => new
                {
                    id = shelf.Id,
                    numberOfParentLibs = shelf.ParentLibraries.Count,
                })
                .ToArray(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (libraryInfo is null)
        {
            ModelState.AddModelError("Guid", $"Couldn't find any library with guid '{libraryGuid}'!");
            return BadRequest(ModelState);
        }

        Guid myGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;

        if (myGuid != libraryInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the library!");
            return BadRequest(ModelState);
        }

        if (libraryInfo.isDefault)
        {
            ModelState.AddModelError("Default Library", $"Cannot Delete default library");
            return BadRequest(ModelState);
        }

        //delete library
        await libraryDb.Libraries.Where(lib => lib.Id == libraryInfo.id).ExecuteDeleteAsync();

        //delete from storage
        libraryProcess.Delete_LibraryDirectory(libraryGuid);

        //set the delault library as the parent of the non-parent shelves
        foreach (var shelfInfo in libraryInfo.shelvesInfo)
        {
            if (shelfInfo.numberOfParentLibs == 1)
            {
                Library_LibraryShelf_DbModel libShelf = new()
                {
                    LibraryId = libraryInfo.ownerDefaultLibraryId,
                    ShelfId = shelfInfo.id,
                };
                libraryDb.LibraryShelves.Add(libShelf);
            }
        }
        //save
        await libraryDb.SaveChangesAsync();

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> ShelfList([FromQuery][StringLength(32)] string libraryGuid)
    {
        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_ShelfCard_ViewModel[] shelfCardModels = await libraryDb.Libraries
        .Where(lib => lib.Guid == libraryGuid_Guid)
        .SelectMany(lib => lib.Shelves)
        .Select(ls => ls.Shelf)
        .Select(shelf => new Library_ShelfCard_ViewModel()
        {
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            DocumentCardModels = shelf.Documents
            .Select(sd => sd.Document)
            .OrderBy(doc => doc.Id)
            .Take(10)
            .Select(doc => new Library_DocumentCard_ViewModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.Owner.Guid,
                Title = doc.Title,
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
                VersionName = doc.Version == "Default" ? null : doc.Version,
            }).ToArray(),
            Guid = shelf.Guid,
            Libraries = shelf.ParentLibraries
            .Select(ls => ls.Library)
            .Select(parentLib => new Library_LibraryBrief_ViewModel()
            {
                Guid = parentLib.Guid,
                Title = parentLib.Title,
            }).ToArray(),
            Title = shelf.Title,
            OwnerGuid = shelf.Owner.Guid,
            TotalNumberOfShelfDocuments = shelf.Documents.Count,
            HasImage = shelf.HasImage,
            IntegrityVersion = shelf.IntegrityVersion,
            IsDefault = shelf.Guid == shelf.Owner.DefaultShelfGuid,
        })
        .AsSplitQuery()
        .ToArrayAsync();

        return Ok(shelfCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> UserShelvesGuids([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] shelfGuids = await libraryDb.Owners
        .Where(o => o.Guid == ownerGuid_Guid)
        .SelectMany(o => o.Shelves)
        .Select(shelf => shelf.Guid)
        .ToArrayAsync();

        return Ok(shelfGuids);
    }
    [HttpGet]
    public async Task<IActionResult> ShelvesGuids([FromQuery][StringLength(32)] string libraryGuid)
    {
        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] shelfGuids = await libraryDb.Libraries
        .Where(lib => lib.Guid == libraryGuid_Guid)
        .SelectMany(lib => lib.Shelves)
        .Select(ls => ls.Shelf)
        .Select(shelf => shelf.Guid)
        .ToArrayAsync();

        return Ok(shelfGuids);
    }

    [HttpGet]
    public async Task<IActionResult> UserShelfList([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var userShelfModels = await libraryDb.Owners
        .Where(owner => owner.Guid == ownerGuid_Guid)
        .SelectMany(owner => owner.Shelves)
        .Select(shelf => new
        {
            shelf.Guid,
            shelf.Title,
            Libraries = shelf.ParentLibraries
            .Select(ls => ls.Library)
            .Select(parentLib => new Library_LibraryBrief_ViewModel()
            {
                Guid = parentLib.Guid,
                Title = parentLib.Title,
            }).ToArray(),
            Documents = shelf.Documents
            .Select(sd => sd.Document)
            .Select(doc => new Library_DocumentBrief_ViewModel()
            {
                Guid = doc.Guid,
                Title = doc.Title,
            }).ToArray(),
        })
        .AsSplitQuery()
        .ToArrayAsync();

        return Ok(userShelfModels);
    }

    [HttpGet]
    public async Task<IActionResult> ShelfModel([FromQuery][StringLength(32)] string shelfGuid)
    {
        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_ShelfCard_ViewModel? shelfCardModel = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == shelfGuid_Guid)
        .Select(shelf => new Library_ShelfCard_ViewModel()
        {
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            DocumentCardModels = shelf.Documents
            .Select(sd => sd.Document)
            .OrderBy(doc => doc.Id)
            .Take(10)
            .Select(doc => new Library_DocumentCard_ViewModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.Owner.Guid,
                Title = doc.Title,
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
                VersionName = doc.Version == "Default" ? null : doc.Version,
            }).ToArray(),
            Guid = shelf.Guid,
            Libraries = shelf.ParentLibraries
            .Select(ls => ls.Library)
            .Select(parentLib => new Library_LibraryBrief_ViewModel()
            {
                Guid = parentLib.Guid,
                Title = parentLib.Title,
            }).ToArray(),
            Title = shelf.Title,
            OwnerGuid = shelf.Owner.Guid,
            TotalNumberOfShelfDocuments = shelf.Documents.Count,
            HasImage = shelf.HasImage,
            IntegrityVersion = shelf.IntegrityVersion,
            IsDefault = shelf.Guid == shelf.Owner.DefaultShelfGuid,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();


        if (shelfCardModel is null)
        {
            return NotFound();
        }

        return Ok(shelfCardModel);
    }

    [HttpGet]
    public IActionResult ShelfImage([FromQuery][StringLength(32)] string shelfGuid)
    {
        string imagePath = Path.Combine(Storage_Shelves.FullName, shelfGuid, "image");
        if (System.IO.File.Exists(imagePath))
        {
            return PhysicalFile(imagePath, "application/octet-stream", "shelfImage", true);
        }
        return NotFound("Shelf Image Not Found!");
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShelf([FromQuery][StringLength(32)] string shelfGuid)
    {
        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var shelfDbInfo = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == shelfGuid_Guid)
        .Select(shelf => new
        {
            id = shelf.Id,
            ownerGuid = shelf.Owner.Guid,
            isDefaultShelf = shelf.Guid == shelf.Owner.DefaultShelfGuid,
            ownerDefaultShelfId = shelf.Owner.Shelves
                .Where(sh => sh.Guid == shelf.Owner.DefaultShelfGuid)
                .Select(sh => sh.Id)
                .First(),
            documentsInfo = shelf.Documents
            .Select(sd => sd.Document)
            .Select(doc => new
            {
                id = doc.Id,
                numberOfParentShelves = doc.ParentShelves.Count,
            }),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (shelfDbInfo is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified shelf!");
            return BadRequest(ModelState);
        }

        Guid myGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (myGuid != shelfDbInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the shelf!");
            return BadRequest(ModelState);
        }

        if (shelfDbInfo.isDefaultShelf)
        {
            ModelState.AddModelError("Default Shelf", "Cannot delete the default shelf!");
            return BadRequest(ModelState);
        }

        //remove the shelf
        await libraryDb.Shelves.Where(shelf => shelf.Id == shelfDbInfo.id).ExecuteDeleteAsync();

        //remove fromstorage
        libraryProcess.Delete_ShelfDirectory(shelfGuid);

        //set the default shelf as the parent of its non-parent documents
        foreach (var docInfo in shelfDbInfo.documentsInfo)
        {
            if (docInfo.numberOfParentShelves == 1)
            {
                Library_ShelfDocument_DbModel shelfDoc = new()
                {
                    ShelfId = shelfDbInfo.ownerDefaultShelfId,
                    DocumentId = docInfo.id,
                };
                libraryDb.ShelfDocuments.Add(shelfDoc);
            }
        }
        //save
        await libraryDb.SaveChangesAsync();

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> DocumentCardList([FromQuery][StringLength(32)] string shelfGuid)
    {
        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_DocumentCard_ViewModel[] documentCardModels = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == shelfGuid_Guid)
        .SelectMany(shelf => shelf.Documents)
        .Select(sd => sd.Document)
        .Select(doc => new Library_DocumentCard_ViewModel()
        {
            Description = doc.Description,
            Guid = doc.Guid,
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
            OwnerGuid = doc.Owner.Guid,
            Title = doc.Title,
            HasImage = doc.HasImage,
            IntegrityVersion = doc.IntegrityVersion,
            VersionName = doc.Version == "Default" ? null : doc.Version,
        })
        .AsSplitQuery()
        .ToArrayAsync();

        return Ok(documentCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> UserDocumentsGuids([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] documentsGuids = await libraryDb.Owners
        .Where(o => o.Guid == ownerGuid_Guid)
        .SelectMany(o => o.Documents)
        .Select(doc => doc.Guid)
        .ToArrayAsync();

        return Ok(documentsGuids);
    }
    [HttpGet]
    public async Task<IActionResult> DocumentsGuids([FromQuery][StringLength(32)] string shelfGuid)
    {
        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] documentsGuids = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == shelfGuid_Guid)
        .SelectMany(shelf => shelf.Documents)
        .Select(sd => sd.Document)
        .Select(doc => doc.Guid)
        .ToArrayAsync();

        return Ok(documentsGuids);
    }

    [HttpGet]
    public async Task<IActionResult> DocumentCardModel([FromQuery][StringLength(32)] string documentGuid)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_DocumentCard_ViewModel? documentCardModel = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid)
        .Select(doc => new Library_DocumentCard_ViewModel()
        {
            Guid = doc.Guid,
            Description = doc.Description,
            Title = doc.Title,
            OwnerGuid = doc.Owner.Guid,
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
            HasImage = doc.HasImage,
            IntegrityVersion = doc.IntegrityVersion,
            VersionName = doc.Version == "Default" ? null : doc.Version,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (documentCardModel is null)
        {
            return NotFound();
        }

        return Ok(documentCardModel);
    }

    [HttpGet]
    public IActionResult DocumentImage([FromQuery][StringLength(32)] string documentGuid)
    {
        string imagePath = Path.Combine(Storage_Documents.FullName, documentGuid, "image");
        if (System.IO.File.Exists(imagePath))
        {
            return PhysicalFile(imagePath, "application/octet-stream", "documentImage", true);
        }
        return NotFound("Document Image Not Found!");
    }

    [HttpGet]
    public async Task<IActionResult> DocumentPageModel([FromQuery][StringLength(32)] string documentGuid)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_DocumentPage_ViewModel? documentPageModel = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid)
        .Select(doc => new Library_DocumentPage_ViewModel()
        {
            CreatedAt = doc.CreatedAt,
            Description = doc.Description,
            Guid = doc.Guid,
            HasImage = doc.HasImage,
            Tags = doc.Tags
            .Select(dt => dt.Tag)
            .Select(tag => tag.Name).ToArray(),
            Title = doc.Title,
            Version = doc.Version,
            Owner = new Library_OwnerBrief_ViewModel()
            {
                UserGuid = doc.Owner.Guid,
                UserName = doc.Owner.NormalizedUserName,//instead of "_"
            },
            Elements = doc.Elements.Select(el => new Library_Element_ViewModel()
            {
                Guid = el.Guid,
                Order = el.Order,
                OwnerGuid = el.Owner.Guid,
                Title = el.Title,
                Type = el.Type,
                UpdatedAt = el.UpdatedAt,
                Value = el.Value ??
                    $"/api/Library/ElementFile?elementGuid={el.Guid.ToString("N")}&elementFileName={el.FileName}",
            }).ToArray(),
            RelatedVersions = doc.RelatedVersions == null ?
            new Library_VersionBrief_ViewModel[0] :
            doc.RelatedVersions!.Documents.Select(rvDoc => new Library_VersionBrief_ViewModel()
            {
                DocumentGuid = rvDoc.Guid,
                VersionName = rvDoc.Version,
            }).ToArray(),
            Shelves = doc.ParentShelves
            .Select(sd => sd.Shelf)
            .Select(parentshelf => new Library_ShelfBrief_ViewModel()
            {
                Documents = parentshelf.Documents
                .Select(sd => sd.Document)
                .Select(parentShelfDoc => new Library_DocumentBrief_ViewModel()
                {
                    Guid = parentShelfDoc.Guid,
                    Title = parentShelfDoc.Title,
                }).ToArray(),
                Guid = parentshelf.Guid,
                Libraries = parentshelf.ParentLibraries
                .Select(ls => ls.Library)
                .Select(parentShelfParentLib => new Library_LibraryBrief_ViewModel()
                {
                    Guid = parentShelfParentLib.Guid,
                    Title = parentShelfParentLib.Title,
                }).ToArray(),
                Title = parentshelf.Title,
            }).ToArray(),
            IntegrityVersion = doc.IntegrityVersion,
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (documentPageModel is null)
        {
            ModelState.AddModelError("documentGuid", "Couldn't find any document with the specified guid!");
            return BadRequest(ModelState);
        }


        /*documentPageModel.Owner.UserName = (await userManager.Users
        .Where(u => u.UserGuid == documentPageModel.Owner.UserGuid)
        .Select(u => u.UserName)
        .FirstOrDefaultAsync())!;*/
        documentPageModel.Owner.UserName = documentPageModel.Owner.UserName.ToLower();

        return Ok(documentPageModel);
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument([FromQuery][StringLength(32)] string documentGuid,
    [FromServices] Review_DbContext reviewDb, [FromServices] Review_Process reviewProcess,
    [FromServices] Notification_DbContext notifDb, [FromServices] Notification_Process notifProcess)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var documentDbInfo = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid)
        .Select(doc => new
        {
            doc.Id,
            ownerGuid = doc.Owner.Guid,
            elementsGuids = doc.Elements.Select(el => el.Guid),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (documentDbInfo is null)
        {
            return NotFound($"There's no document with guid '{documentGuid}'!");
        }

        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        if (myGuid != documentDbInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the author of the document is allowed to delete it!");
            return BadRequest(ModelState);
        }

        //remove from Db
        await libraryDb.Documents.Where(doc => doc.Id == documentDbInfo.Id).ExecuteDeleteAsync();

        //storage
        libraryProcess.Delete_DocumentDirectory(documentGuid);
        //delete directory path of elements from storage
        foreach (Guid elementGuid in documentDbInfo.elementsGuids)
        {
            libraryProcess.Delete_ElementDirectory(elementGuid.ToString("N"));
        }

        //delete review and notif
        await reviewProcess.DeleteReview(reviewDb, documentGuid_Guid, notifDb, notifProcess);

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> ElementFile([FromQuery][StringLength(32)] string elementGuid,
    [FromQuery][StringLength(60)] string? elementFileName)
    {
        if (!Guid.TryParseExact(elementGuid, "N", out Guid elementGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(elementFileName))
        {
            elementFileName = await libraryDb.Elements
            .Where(el => el.Guid == elementGuid_Guid)
            .Select(el => el.FileName)
            .FirstOrDefaultAsync();

            if (elementFileName is null)
            {
                return NotFound("There's no element file name with the specified guid on Db!");
            }
        }

        string filePath = Path.Combine(Storage_Elements.FullName, elementGuid, elementFileName);
        if (System.IO.File.Exists(filePath))
        {
            return PhysicalFile(filePath, "application/octet-stream", elementFileName, true);
        }
        return NotFound("The element file Not found!");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditElements([FromQuery][StringLength(32)] string parentDocumentGuid,
    [FromBody] Library_EditElement_FormModel[] formModels)
    {
        if (ModelState.IsValid)
        {
            if (!Guid.TryParseExact(parentDocumentGuid, "N", out Guid parentDocumentGuid_Guid))
            {
                ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
                return BadRequest(ModelState);
            }

            Guid myGuid = (await userManager.Users
            .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(user => user.UserGuid)
            .FirstOrDefaultAsync())!;

            //IEnumerable<Guid> elementGuids = formModels.Select(m => m.Guid);

            List<Library_ElementDbModel> allElementsDbModels = await libraryDb.Documents
            .Where(doc => doc.Guid == parentDocumentGuid_Guid && doc.Owner.Guid == myGuid)
            .SelectMany(doc => doc.Elements)
            .ToListAsync();

            //make sure all edited elements belong to the same document
            if (!allElementsDbModels.Any())
            {
                ModelState.AddModelError("Authorization or elementGuids", "Only the owner can edit the elements! " +
                "or Couldn't find the specified elements!");
                return BadRequest(ModelState);
            }

            //************* delete elements *************
            bool needToReorder = false;

            IEnumerable<Library_EditElement_FormModel> deletedFormModels =
            formModels.Where(fm => fm.Delete ?? false);

            IEnumerable<Guid> deletedFormModelsGuids = deletedFormModels
            .Select(fm => fm.Guid);

            List<Library_ElementDbModel> deletedElements = allElementsDbModels
            .Where(el => deletedFormModelsGuids.Contains(el.Guid)).ToList();

            if (deletedElements.Count != 0)
            {
                libraryDb.Elements.RemoveRange(deletedElements);
                foreach (var deletedElement in deletedElements)
                {
                    //storage
                    libraryProcess.Delete_ElementDirectory(deletedElement.Guid.ToString("N"));
                }
                needToReorder = true;
                allElementsDbModels = allElementsDbModels.Except(deletedElements).ToList();
            }

            //*********** edit elements ***********
            IEnumerable<Library_EditElement_FormModel> editedFormModels =
            formModels.Except(deletedFormModels);

            //edit
            foreach (var formModel in editedFormModels)
            {
                Library_ElementDbModel? elementDbModel = allElementsDbModels
                .FirstOrDefault(el => el.Guid == formModel.Guid);
                if (elementDbModel is null) continue;

                if (formModel.Order is not null && formModel.Order.HasValue)
                {
                    elementDbModel.Order = formModel.Order.Value;
                }
                if (formModel.Title is not null)
                {
                    elementDbModel.Title = formModel.Title;
                }
                if (formModel.Value is not null)
                {
                    elementDbModel.Value = formModel.Value;
                }
            }

            if (needToReorder)
            {
                var orderedElements = allElementsDbModels.OrderBy(el => el.Order).ToList();
                for (int i = 0; i < orderedElements.Count; i++)
                {
                    orderedElements[i].Order = i;
                }
            }

            //save
            await libraryDb.SaveChangesAsync();

            //create response
            Library_Element_ViewModel[] elementModelArray = allElementsDbModels
            .Select(elementDbModel => new Library_Element_ViewModel()
            {
                Guid = elementDbModel.Guid,
                Order = elementDbModel.Order,
                OwnerGuid = myGuid,
                Title = elementDbModel.Title,
                Type = elementDbModel.Type,
                UpdatedAt = elementDbModel.UpdatedAt,
                Value = elementDbModel.Value ??
                    $"/api/Library/ElementFile?elementGuid={elementDbModel.Guid.ToString("N")}&elementFileName={elementDbModel.FileName}",
            })
            .ToArray();

            return Ok(new { success = true, elements = elementModelArray });
        }

        return BadRequest(ModelState);
    }





    [HttpGet]
    public async Task<IActionResult> TotalNumberOfDocuments([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        int totalNumberOfUserDocuments = await libraryDb.Owners
        .Where(owner => owner.Guid == ownerGuid_Guid)
        .Select(owner => owner.Documents.Count)
        .FirstOrDefaultAsync();

        return Ok(new { totalNumberOfUserDocuments });
    }

    [HttpGet]
    public async Task<IActionResult> TotalNumberOfShelves([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        int totalNumberOfUserShelves = await libraryDb.Owners
        .Where(owner => owner.Guid == ownerGuid_Guid)
        .Select(owner => owner.Shelves.Count)
        .FirstOrDefaultAsync();

        return Ok(new { totalNumberOfUserShelves });
    }





    [HttpPost]
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewLibrary(Library_NewLibrary_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Guid myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewLibrary(libraryDb, myGuid, formModel);
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
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewShelf(Library_NewShelf_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Guid userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
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
    [Authorize]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewDocument(Library_NewDocument_FormModel formModel,
    [FromServices] Review_Process reviewProcess, [FromServices] Review_DbContext reviewDb)
    {
        if (ModelState.IsValid)
        {
            Guid myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewDocument(libraryDb, myGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_DocumentDbModel documentDbModel = (Library_DocumentDbModel)result.ResultObject;

                await reviewProcess.CreateNewReview(reviewDb, documentDbModel.Guid, myGuid);

                return Ok(new { success = true, documentGuid = documentDbModel.Guid });
            }

            ModelState.AddModelError(result.ErrorTitle ?? "New Document Error", result.ErrorDescription ?? "Error Description");
            return BadRequest(ModelState);

        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewElement(Library_NewElement_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Guid myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewElement(libraryDb, myGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_ElementDbModel elementDbModel = (Library_ElementDbModel)result.ResultObject;

                var elementModel = new Library_Element_ViewModel()
                {
                    Guid = elementDbModel.Guid,
                    Order = elementDbModel.Order,
                    OwnerGuid = elementDbModel.Owner.Guid,
                    Title = elementDbModel.Title,
                    Type = elementDbModel.Type,
                    UpdatedAt = elementDbModel.UpdatedAt,
                    Value = elementDbModel.Value ??
                    $"/api/Library/ElementFile?elementGuid={elementDbModel.Guid.ToString("N")}&elementFileName={elementDbModel.FileName}",
                };

                return Ok(elementModel);
            }

            ModelState.AddModelError(result.ErrorTitle ?? "New Element Error", result.ErrorDescription ?? "Error Description");
            return BadRequest(ModelState);
        }
        return BadRequest(ModelState);
    }





    [HttpPost]
    [Authorize]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentIntroduction(
        [FromForm] Library_EditIntroduction_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
            .Include(doc => doc.Owner)
            .FirstOrDefaultAsync(doc => doc.Guid == formModel.Guid);
            if (documentDbModel is null)
            {
                ModelState.AddModelError("Guid", "Couldn't find the specified document!");
                return BadRequest(ModelState);
            }

            Guid myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (myGuid != documentDbModel.Owner.Guid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the document!");
                return BadRequest(ModelState);
            }

            documentDbModel.Title = formModel.Title;
            documentDbModel.Description = formModel.Description;

            if (formModel.Image is not null)
            {
                DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(
                    Path.Combine(Storage_Documents.FullName, documentDbModel.Guid.ToString("N"))
                );
                string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(documentImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }

                documentDbModel.HasImage = true;
                documentDbModel.IntegrityVersion += 1;
            }

            await libraryDb.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                introduction = new
                {
                    documentDbModel.Title,
                    documentDbModel.Description,
                    documentDbModel.HasImage,
                    documentDbModel.IntegrityVersion,
                },
            });
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLibraryIntroduction(
        [FromForm] Library_EditIntroduction_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
            .Include(lib => lib.Owner)
            .FirstOrDefaultAsync(lib => lib.Guid == formModel.Guid);

            if (libraryDbModel is null)
            {
                ModelState.AddModelError("Guid", "Couldn't find the specified library!");
                return BadRequest(ModelState);
            }

            Guid userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            if (userGuid != libraryDbModel.Owner.Guid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the library!");
                return BadRequest(ModelState);
            }

            if (libraryDbModel.Guid == libraryDbModel.Owner.DefaultLibraryGuid)
            {
                ModelState.AddModelError("Default Library", "Cannot edit the default library!");
                return BadRequest(ModelState);
            }

            libraryDbModel.Title = formModel.Title;
            libraryDbModel.Description = string.IsNullOrWhiteSpace(formModel.Description) ? null : formModel.Description;
            //await libraryDb.SaveChangesAsync();

            if (formModel.Image is not null)
            {
                DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(
                    Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid.ToString("N"))
                );
                string libraryImagePath = Path.Combine(libraryDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(libraryImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }

                libraryDbModel.HasImage = true;
                libraryDbModel.IntegrityVersion += 1;
            }

            await libraryDb.SaveChangesAsync();

            //seed
            //await libraryProcess.Update_LibrarySeed(libraryDbModel.Guid, libraryDb);

            return Ok(new
            {
                success = true,
                introduction = new
                {
                    libraryDbModel.Title,
                    libraryDbModel.Description,
                    libraryDbModel.HasImage,
                    libraryDbModel.IntegrityVersion,
                },
            });
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShelfIntroduction(
        [FromForm] Library_EditIntroduction_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
            .Include(shelf => shelf.Owner)
            .FirstOrDefaultAsync(shelf => shelf.Guid == formModel.Guid);
            if (shelfDbModel is null)
            {
                ModelState.AddModelError("Guid", "Couldn't find the specified shelf!");
                return BadRequest(ModelState);
            }

            Guid userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (userGuid != shelfDbModel.Owner.Guid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the shelf!");
                return BadRequest(ModelState);
            }

            if (shelfDbModel.Guid == shelfDbModel.Owner.DefaultShelfGuid)
            {
                ModelState.AddModelError("Default Shelf", "Cannot edit the default shelf!");
                return BadRequest(ModelState);
            }

            shelfDbModel.Title = formModel.Title;
            shelfDbModel.Description = string.IsNullOrWhiteSpace(formModel.Description) ? null : formModel.Description;

            if (formModel.Image is not null)
            {
                DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(
                    Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid.ToString("N"))
                );
                string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(shelfImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }

                shelfDbModel.HasImage = true;
                shelfDbModel.IntegrityVersion += 1;
            }

            await libraryDb.SaveChangesAsync();

            //seed
            //await libraryProcess.Update_ShelfSeed(shelfDbModel.Guid, libraryDb);

            return Ok(new
            {
                success = true,
                introduction = new
                {
                    shelfDbModel.Title,
                    shelfDbModel.Description,
                    shelfDbModel.HasImage,
                    shelfDbModel.IntegrityVersion,
                },
            });
        }

        return BadRequest(ModelState);
    }





    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocumentIntroductionImage([FromQuery][StringLength(32)]
    string documentGuid)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var documentDbInfo = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid)
        .Select(doc => new
        {
            doc.Id,
            ownerGuid = doc.Owner.Guid,
        })
        .FirstOrDefaultAsync();

        if (documentDbInfo is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified document!");
            return BadRequest(ModelState);
        }

        Guid myGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (myGuid != documentDbInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the image of the document's introduction!");
            return BadRequest(ModelState);
        }

        string documentImagePath = Path.Combine(Storage_Documents.FullName, documentGuid, "image");
        if (System.IO.File.Exists(documentImagePath))
        {
            try
            {
                System.IO.File.Delete(documentImagePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok(new { success = false });
            }
        }

        await libraryDb.Documents.Where(doc => doc.Id == documentDbInfo.Id)
        .ExecuteUpdateAsync(setter => setter
            .SetProperty(doc => doc.HasImage, false)
            .SetProperty(doc => doc._integrityVersion, (byte)0)
        );

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLibraryIntroductionImage([FromQuery][StringLength(32)]
    string libraryGuid)
    {
        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var libraryDbInfo = await libraryDb.Libraries
        .Where(lib => lib.Guid == libraryGuid_Guid)
        .Select(lib => new
        {
            lib.Id,
            ownerGuid = lib.Owner.Guid,
        })
        .FirstOrDefaultAsync();

        if (libraryDbInfo is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified library!");
            return BadRequest(ModelState);
        }

        Guid myGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (myGuid != libraryDbInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the image of the library's introduction!");
            return BadRequest(ModelState);
        }

        string libraryImagePath = Path.Combine(Storage_Libraries.FullName, libraryGuid, "image");
        if (System.IO.File.Exists(libraryImagePath))
        {
            try
            {
                System.IO.File.Delete(libraryImagePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok(new { success = false });
            }
        }

        await libraryDb.Libraries.Where(lib => lib.Id == libraryDbInfo.Id)
        .ExecuteUpdateAsync(setter => setter
            .SetProperty(lib => lib.HasImage, false)
            .SetProperty(lib => lib._integrityVersion, (byte)0)
        );

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShelfIntroductionImage([FromQuery][StringLength(32)]
    string shelfGuid)
    {
        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var shelfDbInfo = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == shelfGuid_Guid)
        .Select(shelf => new
        {
            shelf.Id,
            ownerGuid = shelf.Owner.Guid,
        })
        .FirstOrDefaultAsync();

        if (shelfDbInfo is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified shelf!");
            return BadRequest(ModelState);
        }

        Guid myGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (myGuid != shelfDbInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the image of the shelf's introduction!");
            return BadRequest(ModelState);
        }

        string shelfImagePath = Path.Combine(Storage_Shelves.FullName, shelfGuid, "image");
        if (System.IO.File.Exists(shelfImagePath))
        {
            try
            {
                System.IO.File.Delete(shelfImagePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok(new { success = false });
            }
        }

        await libraryDb.Shelves.Where(shelf => shelf.Id == shelfDbInfo.Id)
        .ExecuteUpdateAsync(setter => setter
            .SetProperty(shelf => shelf.HasImage, false)
            .SetProperty(shelf => shelf._integrityVersion, (byte)0)
        );

        return Ok(new { success = true });
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentParentShelves([FromBody]
    Library_DocumentParentShelves_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            var documentDbInfo = await libraryDb.Documents
            .Where(doc => doc.Guid == formModel.DocumentGuid)
            .Select(doc => new
            {
                doc.Id,
                ownerGuid = doc.Owner.Guid,
                //currentParentShelfIds = doc.ParentShelves.Select(shelfDoc => shelfDoc.Shelf.Id),
                specifiedParentShelfIds = doc.Owner.Shelves
                    .Where(shelf => formModel.ShelfGuids.Contains(shelf.Guid))
                    .Select(shelf => shelf.Id),
                ownerDefaultShelfId = doc.Owner.Shelves
                    .Where(shelf => shelf.Guid == doc.Owner.DefaultShelfGuid)
                    .Select(shelf => shelf.Id)
                    .First(),
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync();

            if (documentDbInfo is null)
            {
                ModelState.AddModelError("document", "Couldn't find the specified document for the user!");
                return BadRequest(ModelState);
            }

            Guid myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (myGuid != documentDbInfo.ownerGuid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the parent shelves of the document!");
                return BadRequest(ModelState);
            }

            //delete current join tables
            await libraryDb.ShelfDocuments.Where(shelfDoc => shelfDoc.DocumentId == documentDbInfo.Id)
            .ExecuteDeleteAsync();

            //create join tables
            IEnumerable<Library_ShelfDocument_DbModel> shelfDocRels = documentDbInfo.specifiedParentShelfIds
            .Select(id => new Library_ShelfDocument_DbModel()
            {
                ShelfId = id,
                DocumentId = documentDbInfo.Id,
            });
            if (!shelfDocRels.Any())
            {
                Library_ShelfDocument_DbModel defaultShelfDocRel = new Library_ShelfDocument_DbModel()
                {
                    ShelfId = documentDbInfo.ownerDefaultShelfId,
                    DocumentId = documentDbInfo.Id,
                };
                shelfDocRels = [defaultShelfDocRel];
            }
            //add
            foreach (Library_ShelfDocument_DbModel shelfDoc in shelfDocRels)
            {
                libraryDb.ShelfDocuments.Add(shelfDoc);
            }
            //save
            await libraryDb.SaveChangesAsync();

            //view model
            var parentShelves = await libraryDb.Shelves
            .Where(shelf => documentDbInfo.specifiedParentShelfIds.Contains(shelf.Id))
            .Select(shelf => new
            {
                shelf.Guid,
                shelf.Title,
                Libraries = shelf.ParentLibraries
                .Select(ls => ls.Library)
                .Select(parentLib => new Library_LibraryBrief_ViewModel()
                {
                    Guid = parentLib.Guid,
                    Title = parentLib.Title,
                }).ToArray(),
                Documents = shelf.Documents
                .Select(sd => sd.Document)
                .Select(doc => new Library_DocumentBrief_ViewModel()
                {
                    Guid = doc.Guid,
                    Title = doc.Title,
                }).ToArray(),
            })
            .AsSplitQuery()
            .ToArrayAsync();

            return Ok(new { success = true, parentShelves });
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShelfParentLibraries([FromBody]
    Library_ShelfParentLibraries_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            var shelfDbInfo = await libraryDb.Shelves
            .Where(shelf => shelf.Guid == formModel.ShelfGuid)
            .Select(shelf => new
            {
                shelf.Id,
                ownerGuid = shelf.Owner.Guid,
                newParentLibrariesIds = shelf.Owner.Libraries
                    .Where(lib => formModel.LibraryGuids.Contains(lib.Guid))
                    .Select(lib => lib.Id),
                ownerDefaultLibraryId = shelf.Owner.Libraries
                    .Where(lib => lib.Guid == shelf.Owner.DefaultLibraryGuid)
                    .Select(lib => lib.Id)
                    .First(),
                isDefaultShelf = shelf.Guid == shelf.Owner.DefaultShelfGuid,
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync();

            if (shelfDbInfo is null)
            {
                ModelState.AddModelError("shelf", "Couldn't find the specified shelf for the user!");
                return BadRequest(ModelState);
            }

            Guid myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (myGuid != shelfDbInfo.ownerGuid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the parent libraries of the shelf!");
                return BadRequest(ModelState);
            }

            if (shelfDbInfo.isDefaultShelf)
            {
                ModelState.AddModelError("Default Shelf", "Cannot edit the default shelf!");
                return BadRequest(ModelState);
            }

            //delete current join tables
            await libraryDb.LibraryShelves.Where(libShelf => libShelf.ShelfId == shelfDbInfo.Id)
            .ExecuteDeleteAsync();

            //create join tables
            IEnumerable<Library_LibraryShelf_DbModel> libShelfRels = shelfDbInfo.newParentLibrariesIds
            .Select(id => new Library_LibraryShelf_DbModel()
            {
                LibraryId = id,
                ShelfId = shelfDbInfo.Id,
            });
            if (!libShelfRels.Any())
            {
                Library_LibraryShelf_DbModel defaultLibShelfRel = new()
                {
                    LibraryId = shelfDbInfo.ownerDefaultLibraryId,
                    ShelfId = shelfDbInfo.Id,
                };
                libShelfRels = [defaultLibShelfRel];
            }
            //add
            foreach (Library_LibraryShelf_DbModel libShelf in libShelfRels)
            {
                libraryDb.LibraryShelves.Add(libShelf);
            }
            //save
            await libraryDb.SaveChangesAsync();

            return Ok(new { success = true });
        }

        return BadRequest(ModelState);
    }





    [HttpGet]
    public async Task<IActionResult> GetOwnerModel([FromQuery][StringLength(32)] string ownerGuid)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_Owner_ViewModel? ownerModel = await userManager.Users
        .Where(u => u.UserGuid == ownerGuid_Guid)
        .Select(user => new Library_Owner_ViewModel()
        {
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Guid = user.UserGuid,
            Username = user.UserName!,
        })
        .FirstOrDefaultAsync();

        if (ownerModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        return Ok(ownerModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProfileInfo([FromQuery][StringLength(32)] string userGuid)
    {
        if (!Guid.TryParseExact(userGuid, "N", out Guid userGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_OwnerProfileStatics_ViewModel? userProfileInfo = await libraryDb.Owners
        .Where(o => o.Guid == userGuid_Guid)
        .Select(o => new Library_OwnerProfileStatics_ViewModel()
        {
            NumberOfDocuments = o.Documents.Count(),
            NumberOfFavoriteDocuments = o.FavoriteDocuments.Count,
            NumberOfFavoriteLibraries = o.FavoriteLibraries.Count,
            NumberOfFavoriteShelves = o.FavoriteShelves.Count,
            NumberOfFollowers = o.Followers.Count,
            NumberOfFollowings = o.Followings.Count,
            NumberOfLibraries = o.Libraries.Count,
            NumberOfShelves = o.Shelves.Count,
        })
        .FirstOrDefaultAsync();

        if (userProfileInfo is null)
        {
            ModelState.AddModelError("userGuid", "Couldn't find any library owner with the specified userGuid!");
            return BadRequest(ModelState);
        }

        return Ok(userProfileInfo);
    }





    //*************************** followship *************************
    [HttpGet]
    public async Task<IActionResult> GetFollowers([FromQuery][StringLength(32)] string ownerGuid,
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        bool ownerExist = await libraryDb.Owners
        .AnyAsync(owner => owner.Guid == ownerGuid_Guid);

        if (!ownerExist)
        {
            ModelState.AddModelError("ownerGuid", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = userManager.NormalizeName(filter.Trim());
        }

        List<Guid> followersGuids_MutualsWithMyFollowings = [];
        List<Guid> followersGuids_Others = [];
        if (myGuid is not null && myGuid.HasValue)
        {
            followersGuids_MutualsWithMyFollowings = await libraryDb.Owners
            .Where(o => o.Guid == ownerGuid_Guid)
            .SelectMany(o => o.Followers)
            .Select(ff => ff.Follower)
            .Where(fer => filter == null || fer.NormalizedUserName.Contains(filter))
            .Where(fer => fer.Followers.Any(ferff => ferff.Follower.Guid == myGuid))
            .OrderBy(o => o.Id)
            .Select(o => o.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            //am i a follower
            bool iFollow = await libraryDb.Owners
            .Where(o => o.Guid == ownerGuid_Guid)
            .SelectMany(o => o.Followers)
            .AnyAsync(ff => ff.Follower.Guid == myGuid &&
                (filter == null || ff.Follower.NormalizedUserName.Contains(filter))
            );
            if (iFollow)
            {
                followersGuids_MutualsWithMyFollowings = [myGuid.Value, .. followersGuids_MutualsWithMyFollowings];
            }

            if (followersGuids_MutualsWithMyFollowings.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsIntersectFollowers = await libraryDb.Owners
                .Where(o => o.Guid == ownerGuid_Guid)
                .SelectMany(o => o.Followers)
                .Select(ff => ff.Follower)
                .Where(fer => filter == null || fer.NormalizedUserName.Contains(filter))
                .Where(fer => fer.Followers.Any(ferff => ferff.Follower.Guid == myGuid))
                .CountAsync();
                int numberOfSkipOthersFollowers = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsIntersectFollowers;
                if (numberOfSkipOthersFollowers < 0) numberOfSkipOthersFollowers = 0;

                int numberOfNeededOthersFollowers = bunchSize - followersGuids_MutualsWithMyFollowings.Count;

                followersGuids_Others = await libraryDb.Owners
                .Where(o => o.Guid == ownerGuid_Guid)
                .SelectMany(o => o.Followers)
                .Select(ff => ff.Follower)
                .Where(fer => filter == null || fer.NormalizedUserName.Contains(filter))
                .Where(fer => !fer.Followers.Any(ferff => ferff.Follower.Guid == myGuid))
                .OrderBy(o => o.Id)
                .Select(o => o.Guid)
                .Skip(numberOfSkipOthersFollowers)
                .Take(numberOfNeededOthersFollowers)
                .ToListAsync();
            }
        }
        else
        {
            followersGuids_Others = await libraryDb.Owners
            .Where(o => o.Guid == ownerGuid_Guid)
            .SelectMany(o => o.Followers)
            .Select(ff => ff.Follower)
            .Where(fer => filter == null || fer.NormalizedUserName.Contains(filter))
            .OrderBy(o => o.Id)
            .Select(f => f.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }


        List<Guid> followersGuids = [.. followersGuids_MutualsWithMyFollowings, .. followersGuids_Others];

        Library_Owner_ViewModel[] followers_OwnerModel = await userManager.Users
        .Where(user => followersGuids.Contains(user.UserGuid))
        .Select(user => new Library_Owner_ViewModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArrayAsync();

        return Ok(followers_OwnerModel);
    }
    [HttpGet]
    public async Task<IActionResult> GetFollowings([FromQuery][StringLength(32)] string ownerGuid,
    [FromQuery] int? bunchIndex, [FromQuery][StringLength(30)] string? filter)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        bool ownerExist = await libraryDb.Owners
        .AnyAsync(owner => owner.Guid == ownerGuid_Guid);

        if (!ownerExist)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filter = null;
        }
        else
        {
            filter = userManager.NormalizeName(filter.Trim());
        }

        List<Guid> followingsGuids_MutualsWithMyFollowings = [];
        List<Guid> followingsGuids_Others = [];
        if (myGuid is not null && myGuid.HasValue && ownerGuid_Guid != myGuid)
        {
            followingsGuids_MutualsWithMyFollowings = await libraryDb.Owners
            .Where(o => o.Guid == ownerGuid_Guid)
            .SelectMany(o => o.Followings)
            .Select(ff => ff.Following)
            .Where(fing => filter == null || fing.NormalizedUserName.Contains(filter))
            .Where(fing => fing.Followers.Any(fingff => fingff.Follower.Guid == myGuid))
            .OrderBy(o => o.Id)
            .Select(o => o.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            //am i followed
            bool followedMe = await libraryDb.Owners
            .Where(o => o.Guid == ownerGuid_Guid)
            .SelectMany(o => o.Followings)
            .AnyAsync(ff => ff.Following.Guid == myGuid &&
                (filter == null || ff.Following.NormalizedUserName.Contains(filter))
            );
            if (followedMe)
            {
                followingsGuids_MutualsWithMyFollowings = [myGuid.Value, .. followingsGuids_MutualsWithMyFollowings];
            }

            if (followingsGuids_MutualsWithMyFollowings.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsIntersectFollowers = await libraryDb.Owners
                .Where(o => o.Guid == ownerGuid_Guid)
                .SelectMany(o => o.Followings)
                .Select(ff => ff.Following)
                .Where(fing => filter == null || fing.NormalizedUserName.Contains(filter))
                .Where(fing => fing.Followers.Any(fingff => fingff.Follower.Guid == myGuid))
                .CountAsync();
                int numberOfSkipOthersFollowers = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsIntersectFollowers;
                if (numberOfSkipOthersFollowers < 0) numberOfSkipOthersFollowers = 0;

                int numberOfNeededOthersFollowers = bunchSize - followingsGuids_MutualsWithMyFollowings.Count;

                followingsGuids_Others = await libraryDb.Owners
                .Where(o => o.Guid == ownerGuid_Guid)
                .SelectMany(o => o.Followings)
                .Select(ff => ff.Following)
                .Where(fing => filter == null || fing.NormalizedUserName.Contains(filter))
                .Where(fing => !fing.Followers.Any(fingff => fingff.Follower.Guid == myGuid))
                .OrderBy(o => o.Id)
                .Select(o => o.Guid)
                .Skip(numberOfSkipOthersFollowers)
                .Take(numberOfNeededOthersFollowers)
                .ToListAsync();
            }
        }
        else
        {
            followingsGuids_Others = await libraryDb.Owners
            .Where(o => o.Guid == ownerGuid_Guid)
            .SelectMany(o => o.Followings)
            .Select(ff => ff.Following)
            .Where(f => filter == null || f.NormalizedUserName.Contains(filter))
            .OrderBy(o => o.Id)
            .Select(f => f.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> followingsGuids = [.. followingsGuids_MutualsWithMyFollowings, .. followingsGuids_Others];

        Library_Owner_ViewModel[] followings_OwnerModel = await userManager.Users
        .Where(user => followingsGuids.Contains(user.UserGuid))
        .Select(user => new Library_Owner_ViewModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArrayAsync();

        return Ok(followings_OwnerModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow([FromQuery][StringLength(32)] string ownerGuid,
    Review_DbContext reviewDb)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        //fetch and create
        var followingDbInfo = await libraryDb.Owners
        .Where(owner => owner.Guid == ownerGuid_Guid)
        .Select(owner => new { Id = owner.Id })
        .FirstOrDefaultAsync();

        if (followingDbInfo is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        Guid myGuid = await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstAsync();

        if (myGuid == ownerGuid_Guid)
        {
            ModelState.AddModelError("following", "You cannot follow yourself!");
            return BadRequest(ModelState);
        }

        //fetch and create
        var followerDbInfo = await libraryDb.Owners
        .Where(owner => owner.Guid == myGuid)
        .Select(owner => new { Id = owner.Id })
        .FirstAsync();

        //create join table
        Library_FollowerFollowing_DbModel followerFollowing = new()
        {
            FollowerId = followerDbInfo.Id,
            FollowingId = followingDbInfo.Id,
        };
        //add
        libraryDb.FollowerFollowings.Add(followerFollowing);
        //save
        await libraryDb.SaveChangesAsync();

        //***** review *****
        //fetch and create
        var followerDbInfo_Review = await reviewDb.Users
        .Where(u => u.Guid == ownerGuid_Guid)
        .Select(u => new { Id = u.Id })
        .FirstAsync();

        var followingDbInfo_Review = await reviewDb.Users
        .Where(u => u.Guid == myGuid)
        .Select(u => new { Id = u.Id })
        .FirstAsync();

        //create join table
        Review_FollowerFollowing_DbModel followerFollowing_Review = new()
        {
            FollowerId = followerDbInfo_Review.Id,
            FollowingId = followingDbInfo_Review.Id,
        };
        //add
        reviewDb.FollowerFollowings.Add(followerFollowing_Review);
        //save
        await reviewDb.SaveChangesAsync();

        return Ok(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnFollow([FromQuery][StringLength(32)] string ownerGuid,
    Review_DbContext reviewDb)
    {
        if (!Guid.TryParseExact(ownerGuid, "N", out Guid ownerGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid myGuid = await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstAsync();

        if (myGuid == ownerGuid_Guid)
        {
            ModelState.AddModelError("unFollowing", "You cannot unFollow yourself!");
            return BadRequest(ModelState);
        }

        //***** library *****
        //delete join table
        await libraryDb.FollowerFollowings
        .Where(ff => ff.Follower.Guid == myGuid && ff.Following.Guid == ownerGuid_Guid)
        .ExecuteDeleteAsync();

        //***** review *****
        //delete join table
        await reviewDb.FollowerFollowings
        .Where(ff => ff.Follower.Guid == myGuid && ff.Following.Guid == ownerGuid_Guid)
        .ExecuteDeleteAsync();

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> GetFavoriteLibrariesGuids([FromQuery][StringLength(32)] string userGuid)
    {
        if (!Guid.TryParseExact(userGuid, "N", out Guid userGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] favoriteLibrariesGuids = await libraryDb.Owners
        .Where(o => o.Guid == userGuid_Guid)
        .SelectMany(o => o.FavoriteLibraries)
        .Select(ul => ul.Library)
        .Select(lib => lib.Guid)
        .ToArrayAsync();

        return Ok(favoriteLibrariesGuids);
    }
    [HttpGet]
    public async Task<IActionResult> GetFavoriteShelvesGuids([FromQuery][StringLength(32)] string userGuid)
    {
        if (!Guid.TryParseExact(userGuid, "N", out Guid userGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] favoriteShelvesGuids = await libraryDb.Owners
        .Where(o => o.Guid == userGuid_Guid)
        .SelectMany(o => o.FavoriteShelves)
        .Select(ush => ush.Shelf)
        .Select(shelf => shelf.Guid)
        .ToArrayAsync();

        return Ok(favoriteShelvesGuids);
    }
    [HttpGet]
    public async Task<IActionResult> GetFavoriteDocumentsGuids([FromQuery][StringLength(32)] string userGuid)
    {
        if (!Guid.TryParseExact(userGuid, "N", out Guid userGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid[] favoriteDocumentsGuids = await libraryDb.Owners
        .Where(o => o.Guid == userGuid_Guid)
        .SelectMany(o => o.FavoriteDocuments)
        .Select(ud => ud.Document)
        .Select(doc => doc.Guid)
        .ToArrayAsync();

        return Ok(favoriteDocumentsGuids);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersInFavorOfLibrary([FromQuery][StringLength(32)] string libraryGuid,
    [FromQuery] int? bunchIndex)
    {
        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        bool libraryExist = await libraryDb.Libraries
        .AnyAsync(lib => lib.Guid == libraryGuid_Guid);

        if (!libraryExist)
        {
            ModelState.AddModelError("library", "the specified library Not found!");
            return BadRequest(ModelState);
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        List<Guid> usersInFavorOfGuids_MutualsWithMyFollowings = [];
        List<Guid> UsersInFavorOfGuids_Others = [];
        if (myGuid is not null && myGuid.HasValue)
        {
            usersInFavorOfGuids_MutualsWithMyFollowings = await libraryDb.Libraries
            .Where(lib => lib.Guid == libraryGuid_Guid)
            .SelectMany(lib => lib.InFavorOf)
            .Select(ul => ul.User)
            .Where(u => u.Followers.Any(uff => uff.Follower.Guid == myGuid))
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            //is my favorite
            bool myFavorite = await libraryDb.Libraries
            .Where(lib => lib.Guid == libraryGuid_Guid)
            .SelectMany(lib => lib.InFavorOf)
            .AnyAsync(ul => ul.User.Guid == myGuid);
            if (myFavorite)
            {
                usersInFavorOfGuids_MutualsWithMyFollowings = [myGuid.Value, .. usersInFavorOfGuids_MutualsWithMyFollowings];
            }

            if (usersInFavorOfGuids_MutualsWithMyFollowings.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsIntersectFollowers = await libraryDb.Libraries
                .Where(lib => lib.Guid == libraryGuid_Guid)
                .SelectMany(lib => lib.InFavorOf)
                .Select(ul => ul.User)
                .Where(u => u.Followers.Any(uff => uff.Follower.Guid == myGuid))
                .CountAsync();
                int numberOfSkipOthersFollowers = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsIntersectFollowers;
                if (numberOfSkipOthersFollowers < 0) numberOfSkipOthersFollowers = 0;

                int numberOfNeededOthersFollowers = bunchSize - usersInFavorOfGuids_MutualsWithMyFollowings.Count;

                UsersInFavorOfGuids_Others = await libraryDb.Libraries
                .Where(lib => lib.Guid == libraryGuid_Guid)
                .SelectMany(lib => lib.InFavorOf)
                .Select(ul => ul.User)
                .Where(u => !u.Followers.Any(uff => uff.Follower.Guid == myGuid))
                .OrderBy(u => u.Id)
                .Select(u => u.Guid)
                .Skip(numberOfSkipOthersFollowers)
                .Take(numberOfNeededOthersFollowers)
                .ToListAsync();
            }
        }
        else
        {
            UsersInFavorOfGuids_Others = await libraryDb.Libraries
            .Where(lib => lib.Guid == libraryGuid_Guid)
            .SelectMany(lib => lib.InFavorOf)
            .Select(u => u.User)
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> followingsGuids = [.. usersInFavorOfGuids_MutualsWithMyFollowings, .. UsersInFavorOfGuids_Others];

        Library_Owner_ViewModel[] users_OwnerModel = await userManager.Users
        .Where(user => followingsGuids.Contains(user.UserGuid))
        .Select(user => new Library_Owner_ViewModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArrayAsync();

        return Ok(users_OwnerModel);
    }
    [HttpGet]
    public async Task<IActionResult> GetUsersInFavorOfShelf([FromQuery][StringLength(32)] string shelfGuid,
    [FromQuery] int? bunchIndex)
    {
        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        bool shelfExist = await libraryDb.Shelves
        .AnyAsync(shelf => shelf.Guid == shelfGuid_Guid);

        if (!shelfExist)
        {
            ModelState.AddModelError("shelf", "the specified shelf Not found!");
            return BadRequest(ModelState);
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        List<Guid> usersInFavorOfGuids_MutualsWithMyFollowings = [];
        List<Guid> UsersInFavorOfGuids_Others = [];
        if (myGuid is not null && myGuid.HasValue)
        {
            usersInFavorOfGuids_MutualsWithMyFollowings = await libraryDb.Shelves
            .Where(shelf => shelf.Guid == shelfGuid_Guid)
            .SelectMany(shelf => shelf.InFavorOf)
            .Select(ul => ul.User)
            .Where(u => u.Followers.Any(uff => uff.Follower.Guid == myGuid))
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            //is my favorite
            bool myFavorite = await libraryDb.Shelves
            .Where(shelf => shelf.Guid == shelfGuid_Guid)
            .SelectMany(shelf => shelf.InFavorOf)
            .AnyAsync(ul => ul.User.Guid == myGuid);
            if (myFavorite)
            {
                usersInFavorOfGuids_MutualsWithMyFollowings = [myGuid.Value, .. usersInFavorOfGuids_MutualsWithMyFollowings];
            }

            if (usersInFavorOfGuids_MutualsWithMyFollowings.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsIntersectFollowers = await libraryDb.Shelves
                .Where(shelf => shelf.Guid == shelfGuid_Guid)
                .SelectMany(shelf => shelf.InFavorOf)
                .Select(ul => ul.User)
                .Where(u => u.Followers.Any(uff => uff.Follower.Guid == myGuid))
                .CountAsync();
                int numberOfSkipOthersFollowers = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsIntersectFollowers;
                if (numberOfSkipOthersFollowers < 0) numberOfSkipOthersFollowers = 0;

                int numberOfNeededOthersFollowers = bunchSize - usersInFavorOfGuids_MutualsWithMyFollowings.Count;

                UsersInFavorOfGuids_Others = await libraryDb.Shelves
                .Where(shelf => shelf.Guid == shelfGuid_Guid)
                .SelectMany(shelf => shelf.InFavorOf)
                .Select(ul => ul.User)
                .Where(u => !u.Followers.Any(uff => uff.Follower.Guid == myGuid))
                .OrderBy(u => u.Id)
                .Select(u => u.Guid)
                .Skip(numberOfSkipOthersFollowers)
                .Take(numberOfNeededOthersFollowers)
                .ToListAsync();
            }
        }
        else
        {
            UsersInFavorOfGuids_Others = await libraryDb.Shelves
            .Where(shelf => shelf.Guid == shelfGuid_Guid)
            .SelectMany(shelf => shelf.InFavorOf)
            .Select(u => u.User)
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> followingsGuids = [.. usersInFavorOfGuids_MutualsWithMyFollowings, .. UsersInFavorOfGuids_Others];

        Library_Owner_ViewModel[] users_OwnerModel = await userManager.Users
        .Where(user => followingsGuids.Contains(user.UserGuid))
        .Select(user => new Library_Owner_ViewModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArrayAsync();

        return Ok(users_OwnerModel);
    }
    [HttpGet]
    public async Task<IActionResult> GetUsersInFavorOfDocument([FromQuery][StringLength(32)] string documentGuid,
    [FromQuery] int? bunchIndex)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        bool documentExist = await libraryDb.Documents
        .AnyAsync(doc => doc.Guid == documentGuid_Guid);

        if (!documentExist)
        {
            ModelState.AddModelError("document", "the specified document Not found!");
            return BadRequest(ModelState);
        }

        Guid? myGuid = null;
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null)
        {
            myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();
        }

        bunchIndex ??= 0;
        int bunchSize = 10;

        List<Guid> usersInFavorOfGuids_MutualsWithMyFollowings = [];
        List<Guid> UsersInFavorOfGuids_Others = [];
        if (myGuid is not null && myGuid.HasValue)
        {
            usersInFavorOfGuids_MutualsWithMyFollowings = await libraryDb.Documents
            .Where(doc => doc.Guid == documentGuid_Guid)
            .SelectMany(doc => doc.InFavorOf)
            .Select(ul => ul.User)
            .Where(u => u.Followers.Any(uff => uff.Follower.Guid == myGuid))
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();

            //is my favorite
            bool myFavorite = await libraryDb.Documents
            .Where(doc => doc.Guid == documentGuid_Guid)
            .SelectMany(doc => doc.InFavorOf)
            .AnyAsync(ul => ul.User.Guid == myGuid);
            if (myFavorite)
            {
                usersInFavorOfGuids_MutualsWithMyFollowings = [myGuid.Value, .. usersInFavorOfGuids_MutualsWithMyFollowings];
            }

            if (usersInFavorOfGuids_MutualsWithMyFollowings.Count < bunchSize)
            {
                int totalNumberOfMyFollowingsIntersectFollowers = await libraryDb.Documents
                .Where(doc => doc.Guid == documentGuid_Guid)
                .SelectMany(doc => doc.InFavorOf)
                .Select(ul => ul.User)
                .Where(u => u.Followers.Any(uff => uff.Follower.Guid == myGuid))
                .CountAsync();
                int numberOfSkipOthersFollowers = (bunchIndex.Value * bunchSize) - totalNumberOfMyFollowingsIntersectFollowers;
                if (numberOfSkipOthersFollowers < 0) numberOfSkipOthersFollowers = 0;

                int numberOfNeededOthersFollowers = bunchSize - usersInFavorOfGuids_MutualsWithMyFollowings.Count;

                UsersInFavorOfGuids_Others = await libraryDb.Documents
                .Where(doc => doc.Guid == documentGuid_Guid)
                .SelectMany(doc => doc.InFavorOf)
                .Select(ul => ul.User)
                .Where(u => !u.Followers.Any(uff => uff.Follower.Guid == myGuid))
                .OrderBy(u => u.Id)
                .Select(u => u.Guid)
                .Skip(numberOfSkipOthersFollowers)
                .Take(numberOfNeededOthersFollowers)
                .ToListAsync();
            }
        }
        else
        {
            UsersInFavorOfGuids_Others = await libraryDb.Documents
            .Where(doc => doc.Guid == documentGuid_Guid)
            .SelectMany(doc => doc.InFavorOf)
            .Select(u => u.User)
            .OrderBy(u => u.Id)
            .Select(u => u.Guid)
            .Skip(bunchIndex.Value * bunchSize)
            .Take(bunchSize)
            .ToListAsync();
        }

        List<Guid> followingsGuids = [.. usersInFavorOfGuids_MutualsWithMyFollowings, .. UsersInFavorOfGuids_Others];

        Library_Owner_ViewModel[] users_OwnerModel = await userManager.Users
        .Where(user => followingsGuids.Contains(user.UserGuid))
        .Select(user => new Library_Owner_ViewModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArrayAsync();

        return Ok(users_OwnerModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteLibrary([FromQuery][StringLength(32)] string libraryGuid)
    {
        Guid myGuid = await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstAsync();

        if (!Guid.TryParseExact(libraryGuid, "N", out Guid libraryGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var libInfo = await libraryDb.Libraries
        .Where(lib => lib.Guid == libraryGuid_Guid)
        .Select(lib => new
        {
            lib.Id,
            isMyFavorite = lib.InFavorOf.Any(ulf => ulf.User.Guid == myGuid),
            myId = lib.InFavorOf
                .Where(ulf => ulf.User.Guid == myGuid)
                .Select(ulf => ulf.User.Id)
                .FirstOrDefault(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (libInfo is null)
        {
            ModelState.AddModelError("library", "the specified library Not found!");
            return BadRequest(ModelState);
        }

        if (libInfo.isMyFavorite)
        {
            await libraryDb.UserFavoriteLibraries
            .Where(ufl => ufl.UserId == libInfo.myId && ufl.LibraryId == libInfo.Id)
            .ExecuteDeleteAsync();
        }
        else
        {
            int myId = await libraryDb.Owners.Where(o => o.Guid == myGuid).Select(o => o.Id).FirstAsync();
            Library_UserFavoriteLibrary_DbModel userFavoriteLibrary = new()
            {
                UserId = myId,
                LibraryId = libInfo.Id,
            };
            libraryDb.UserFavoriteLibraries.Add(userFavoriteLibrary);
            await libraryDb.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteShelf([FromQuery][StringLength(32)] string shelfGuid)
    {
        Guid myGuid = await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstAsync();

        if (!Guid.TryParseExact(shelfGuid, "N", out Guid shelfGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var shelfInfo = await libraryDb.Shelves
        .Where(shelf => shelf.Guid == shelfGuid_Guid)
        .Select(shelf => new
        {
            shelf.Id,
            isMyFavorite = shelf.InFavorOf.Any(ulf => ulf.User.Guid == myGuid),
            myId = shelf.InFavorOf
                .Where(ulf => ulf.User.Guid == myGuid)
                .Select(ulf => ulf.User.Id)
                .FirstOrDefault(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (shelfInfo is null)
        {
            ModelState.AddModelError("shelf", "the specified shelf Not found!");
            return BadRequest(ModelState);
        }

        if (shelfInfo.isMyFavorite)
        {
            await libraryDb.UserFavoriteShelves
            .Where(ufs => ufs.UserId == shelfInfo.myId && ufs.ShelfId == shelfInfo.Id)
            .ExecuteDeleteAsync();
        }
        else
        {
            int myId = await libraryDb.Owners.Where(o => o.Guid == myGuid).Select(o => o.Id).FirstAsync();
            Library_UserFavoriteShelf_DbModel userFavoriteShelf = new()
            {
                UserId = myId,
                ShelfId = shelfInfo.Id,
            };
            libraryDb.UserFavoriteShelves.Add(userFavoriteShelf);
            await libraryDb.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteDocument([FromQuery][StringLength(32)] string documentGuid)
    {
        Guid myGuid = await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstAsync();

        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        var docInfo = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid)
        .Select(doc => new
        {
            doc.Id,
            isMyFavorite = doc.InFavorOf.Any(ulf => ulf.User.Guid == myGuid),
            myId = doc.InFavorOf
                .Where(ufd => ufd.User.Guid == myGuid)
                .Select(ufd => ufd.User.Id)
                .FirstOrDefault(),
        })
        .AsSplitQuery()
        .FirstOrDefaultAsync();

        if (docInfo is null)
        {
            ModelState.AddModelError("document", "the specified document Not found!");
            return BadRequest(ModelState);
        }

        if (docInfo.isMyFavorite)
        {
            await libraryDb.UserFavoriteDocuments
            .Where(ufd => ufd.UserId == docInfo.myId && ufd.DocumentId == docInfo.Id)
            .ExecuteDeleteAsync();
        }
        else
        {
            int myId = await libraryDb.Owners.Where(o => o.Guid == myGuid).Select(o => o.Id).FirstAsync();
            Library_UserFavoriteDocument_DbModel userFavoriteDocument = new()
            {
                UserId = myId,
                DocumentId = docInfo.Id,
            };
            libraryDb.UserFavoriteDocuments.Add(userFavoriteDocument);
            await libraryDb.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }





    //************************* tags *************************
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentTags([FromForm] Library_EditTags_FormModel formModel)
    {
        if (ModelState.IsValid)
        {
            var documentDbInfo = await libraryDb.Documents
            .Where(doc => doc.Guid == formModel.DocumentGuid)
            .Select(doc => new
            {
                doc.Id,
                ownerGuid = doc.Owner.Guid,
            })
            .FirstOrDefaultAsync();

            if (documentDbInfo is null)
            {
                ModelState.AddModelError("documentGuid", $"couldn't find any document with the specified guid '{formModel.DocumentGuid}'");
                return BadRequest(ModelState);
            }

            Guid myGuid = await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstAsync();

            if (myGuid != documentDbInfo.ownerGuid)
            {
                ModelState.AddModelError("Authorize", $"Only the owner of the document can edit its tags!");
                return BadRequest(ModelState);
            }

            List<Library_TagDbModel> existingTagDbModels = await libraryDb.Tags
            .Where(tag => formModel.Tags.Contains(tag.Name))
            .ToListAsync();

            List<string> notExistingTagNames = formModel.Tags
            .Except(existingTagDbModels.Select(tag => tag.Name))
            .ToList();

            foreach (string tagName in notExistingTagNames)
            {
                Library_TagDbModel tagDbModel = new() { Name = tagName };
                //libraryDb.Tags.Add(tagDbModel);
                existingTagDbModels.Add(tagDbModel);
            }

            //delete current join tables
            await libraryDb.DocumentTags.Where(docTag => docTag.DocumentId == documentDbInfo.Id)
            .ExecuteDeleteAsync();

            //create and add new join tables
            foreach (Library_TagDbModel tagDbModel in existingTagDbModels)
            {
                //create
                Library_DocumentTag_DbModel documentTag = new()
                {
                    DocumentId = documentDbInfo.Id,
                    Tag = tagDbModel,
                };
                //add
                libraryDb.DocumentTags.Add(documentTag);
            }

            //save
            await libraryDb.SaveChangesAsync();

            //remove every tag without any document
            await libraryDb.Tags.Where(tag => tag.Documents.Count == 0).ExecuteDeleteAsync();

            return Ok(existingTagDbModels.Select(tag => tag.Name).ToArray());
        }
        return BadRequest(ModelState);
    }

    [HttpGet]
    public async Task<IActionResult> GetTagsList([FromQuery][StringLength(32, MinimumLength = 3)] string partialName)
    {
        string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        foreach (char c in partialName)
        {
            if (!allowedCharacters.Contains(c))
            {
                ModelState.AddModelError(partialName, $"No tag name contains the character '{c}'!");
                return BadRequest(ModelState);
            }
        }

        string[] tags = await libraryDb.Tags
        .Where(t => t.Name.Contains(partialName))
        .Select(t => t.Name)
        .ToArrayAsync();

        return Ok(tags);
    }





    //************************* version *************************
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentVersionName([FromQuery][StringLength(32)] string documentGuid,
    [FromQuery][StringLength(32)] string versionName)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        /*var documentDbInfo = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid)
        .Select(doc => new
        {
            doc.Id,
            ownerGuid = doc.Owner.Guid,
        })
        .FirstOrDefaultAsync();

        if (documentDbInfo is null)
        {
            ModelState.AddModelError("documentGuid", "there's no document with the specified guid!");
            return BadRequest(ModelState);
        }*/

        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        /*if (myGuid != documentDbInfo.ownerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can edit document with the specified guid!");
            return BadRequest(ModelState);
        }*/

        /*/create
        Library_DocumentDbModel documentDbModel = new() { Id = documentDbInfo.Id };
        //Attach
        libraryDb.Documents.Attach(documentDbModel);
        //edit
        documentDbModel.Version = versionName;
        //modified
        libraryDb.Documents.Entry(documentDbModel).Property(doc => doc.Version).IsModified = true;
        //save
        await libraryDb.SaveChangesAsync();*/
        int numberOfRowsUpdated = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid && doc.Owner.Guid == myGuid)
        .ExecuteUpdateAsync(setter => setter
            .SetProperty(doc => doc.Version, versionName)
        );
        if (numberOfRowsUpdated == 0)
        {
            ModelState.AddModelError("Authorization or documentGuid",
            "Only the owner can edit document with the specified guid! " +
            "there's no document with the specified guid!");
            return BadRequest(ModelState);
        }
        return Ok(new { version = versionName });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewDocumentVersion([FromQuery][StringLength(32)] string baseDocumentGuid,
    [FromQuery][StringLength(32)] string newVersionName, [FromServices] Review_DbContext reviewDb,
    [FromServices] Review_Process reviewProcess)
    {
        if (!Guid.TryParseExact(baseDocumentGuid, "N", out Guid baseDocumentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Library_DocumentDbModel? baseDocumentDbModel = await libraryDb.Documents
        .AsSplitQuery()
        //.AsNoTracking() //Owner and RelatedVersions needs to be tracked
        .Include(doc => doc.Owner)
        .Include(doc => doc.Elements)
        .Include(doc => doc.ParentShelves)
            .ThenInclude(shelfDoc => shelfDoc.Shelf)
        .Include(doc => doc.RelatedVersions)
        .Include(doc => doc.Tags)
            .ThenInclude(docTag => docTag.Tag)
        .FirstOrDefaultAsync(doc => doc.Guid == baseDocumentGuid_Guid);

        if (baseDocumentDbModel is null)
        {
            ModelState.AddModelError("baseDocumentGuid", "there's no base document with the specified guid!");
            return BadRequest(ModelState);
        }

        var me = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => new { u.UserGuid, u.UserName })
        .FirstAsync();

        if (me.UserGuid != baseDocumentDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the owner of the base document can create a new version of it!");
            return BadRequest(ModelState);
        }

        baseDocumentDbModel.RelatedVersions ??= new();

        Library_DocumentDbModel newDocumentDbModel = new()
        {
            Description = baseDocumentDbModel.Description,
            HasImage = baseDocumentDbModel.HasImage,
            Owner = baseDocumentDbModel.Owner,
            RelatedVersions = baseDocumentDbModel.RelatedVersions,
            Title = baseDocumentDbModel.Title,
            Version = newVersionName,
        };
        libraryDb.Documents.Add(newDocumentDbModel);

        //elements
        List<Library_ElementDbModel> newElements = baseDocumentDbModel.Elements
        .Select(el => new Library_ElementDbModel()
        {
            FileName = el.FileName,
            Order = el.Order,
            Owner = baseDocumentDbModel.Owner,
            Title = el.Title,
            Type = el.Type,
            Value = el.Value,
            ParentDocument = newDocumentDbModel,
        })
        .ToList();
        libraryDb.Elements.AddRange(newElements);

        //tags
        IEnumerable<Library_DocumentTag_DbModel> newDocumentTagRels = baseDocumentDbModel.Tags
        .Select(dt => new Library_DocumentTag_DbModel()
        {
            Document = newDocumentDbModel,
            Tag = dt.Tag,
        });
        libraryDb.DocumentTags.AddRange(newDocumentTagRels);

        //parentShelves
        IEnumerable<Library_ShelfDocument_DbModel> newShelfDocumentRels =
        baseDocumentDbModel.ParentShelves
        .Select(sd => new Library_ShelfDocument_DbModel()
        {
            Shelf = sd.Shelf,
            Document = newDocumentDbModel,
        });
        libraryDb.ShelfDocuments.AddRange(newShelfDocumentRels);

        //save
        await libraryDb.SaveChangesAsync();

        //copy introduction image to the new directory
        if (baseDocumentDbModel.HasImage)
        {
            string baseDocumentImagePath =
            Path.Combine(Storage_Documents.FullName, baseDocumentDbModel.Guid.ToString("N"), "image");
            if (System.IO.File.Exists(baseDocumentImagePath))
            {
                //create new document directory
                DirectoryInfo newDocumentirectoryInfo = Directory.CreateDirectory(
                    Path.Combine(Storage_Documents.FullName, newDocumentDbModel.Guid.ToString("N"))
                );
                string newDocumentImagePath = Path.Combine(newDocumentirectoryInfo.FullName, "image");
                //copy
                System.IO.File.Copy(baseDocumentImagePath, newDocumentImagePath);
            }
        }

        //copy element files and images to the new directory
        List<Library_ElementDbModel> fileElements = baseDocumentDbModel.Elements
        .Where(el => el.FileName != null).ToList();
        foreach (Library_ElementDbModel fileElement in fileElements)
        {
            string baseElementFilePath =
            Path.Combine(Storage_Elements.FullName, fileElement.Guid.ToString("N"), fileElement.FileName!);
            if (System.IO.File.Exists(baseElementFilePath))
            {
                Library_ElementDbModel newCorespondElement = newElements
                .Single(el => el.FileName == fileElement.FileName &&
                el.Order == fileElement.Order &&
                el.Type == fileElement.Type);
                //create new element directory
                DirectoryInfo newElementDirectoryInfo = Directory.CreateDirectory(
                    Path.Combine(Storage_Elements.FullName, newCorespondElement.Guid.ToString("N"))
                );
                string newElementFilePath =
                Path.Combine(newElementDirectoryInfo.FullName, newCorespondElement.FileName!);
                //copy
                System.IO.File.Copy(baseElementFilePath, newElementFilePath);
                //if the file is image create different size files of it
            }
        }

        //review
        await reviewProcess.CreateNewReview(reviewDb, newDocumentDbModel.Guid, me.UserGuid);

        return Ok(new { newVersionGuid = newDocumentDbModel.Guid });

    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVersionRelationship([FromQuery][StringLength(32)] string baseDocumentGuid,
    [FromQuery][StringLength(32)] string newRelatedDocumentGuid)
    {
        if (!Guid.TryParseExact(baseDocumentGuid, "N", out Guid baseDocumentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }
        if (!Guid.TryParseExact(newRelatedDocumentGuid, "N", out Guid newRelatedDocumentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        var baseDocumentDbInfo = await libraryDb.Documents
        .Where(doc => doc.Guid == baseDocumentGuid_Guid && doc.Owner.Guid == myGuid)
        .Select(doc => new
        {
            doc.Id,
            doc.RelatedVersions,
        })
        .FirstOrDefaultAsync();

        if (baseDocumentDbInfo is null)
        {
            ModelState.AddModelError("Authorization or baseDocumentGuid",
            "Only the owner of the specified documents can create relate then! " +
            "or there's no base document with the specified guid!");
            return BadRequest(ModelState);
        }

        Library_RelatedVersionsDbModel? relatedVersions = baseDocumentDbInfo.RelatedVersions;
        if (relatedVersions is null)
        {
            relatedVersions = new();
            await libraryDb.Documents.Where(doc => doc.Id == baseDocumentDbInfo.Id)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(doc => doc.RelatedVersions, relatedVersions)
            );
        }

        int numberOfUpdatedRows = await libraryDb.Documents
        .Where(doc => doc.Guid == newRelatedDocumentGuid_Guid && doc.Owner.Guid == myGuid)
        .ExecuteUpdateAsync(setter => setter
            .SetProperty(doc => doc.RelatedVersions, relatedVersions)
        );

        if (numberOfUpdatedRows == 0)
        {
            ModelState.AddModelError("Authorization or newRelatedDocumentGuid",
            "Only the owner of the specified documents can create relate then! " +
            "or there's no new related document with the specified guid!");
            return BadRequest(ModelState);
        }

        Library_VersionBrief_ViewModel[] versionBriefs = await libraryDb.RelatedVersions
        .Where(rv => rv.Guid == relatedVersions.Guid)
        .SelectMany(rv => rv.Documents)
        .Select(doc => new Library_VersionBrief_ViewModel()
        {
            DocumentGuid = doc.Guid,
            VersionName = doc.Version,
        })
        .ToArrayAsync();

        return Ok(versionBriefs);

    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVersionRelationship([FromQuery][StringLength(32)] string documentGuid)
    {
        if (!Guid.TryParseExact(documentGuid, "N", out Guid documentGuid_Guid))
        {
            ModelState.AddModelError("Parse Guid", "Couldn't parse the specified guid!");
            return BadRequest(ModelState);
        }

        Guid myGuid = await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstAsync();

        int numberOfUpdatedRows = await libraryDb.Documents
        .Where(doc => doc.Guid == documentGuid_Guid && doc.Owner.Guid == myGuid)
        .ExecuteUpdateAsync(setter => setter
            .SetProperty(doc => EF.Property<int?>(doc, "RelatedVersionsId"), (int?)null)
        );

        if (numberOfUpdatedRows == 0)
        {
            ModelState.AddModelError("Authorization or documentGuid",
            "Only the owner of the specified documents can edit it! " +
            "or there's no document with the specified guid!");
            return BadRequest(ModelState);
        }

        return Ok(new { success = true });
    }



}