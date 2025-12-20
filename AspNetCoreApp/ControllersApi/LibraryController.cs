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
        Library_LibraryCardModel[] libraryCardModels = await libraryDb.Owners
        .Include(owner => owner.Libraries)
        .ThenInclude(lib => lib.Shelves)
        .Where(owner => owner.Guid == ownerGuid)
        .SelectMany(owner => owner.Libraries)
        .Select(lib => new Library_LibraryCardModel()
        {
            Guid = lib.Guid,
            Title = lib.Title,
            Description = lib.Description,
            ShelvesTitles = lib.Shelves.Take(10).Select(shelf => shelf.Title).ToArray(),
            CreatedAt = lib.CreatedAt,
            OwnerGuid = lib.Owner.Guid,
            IntegrityVersion = lib.IntegrityVersion,
            HasImage = lib.HasImage,
            IsDefault = lib.Guid == lib.Owner.DefaultLibraryGuid,
        })
        .ToArrayAsync();

        return Ok(libraryCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> LibraryModel([FromQuery][StringLength(32)] string libraryGuid)
    {
        Library_LibraryCardModel? libraryCardModel = await libraryDb.Libraries
        .Include(lib => lib.Shelves)
        .Include(lib => lib.Owner)
        .Where(lib => lib.Guid == libraryGuid)
        .Select(lib => new Library_LibraryCardModel()
        {
            Guid = lib.Guid,
            Title = lib.Title,
            Description = lib.Description,
            ShelvesTitles = lib.Shelves.Take(10).Select(shelf => shelf.Title).ToArray(),
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
        Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
        .Include(lib => lib.Owner)
        .ThenInclude(owner => owner.Libraries)
        .Include(lib => lib.Shelves)
        .ThenInclude(shelf => shelf.ParentLibraries)
        .AsSplitQuery()
        .FirstOrDefaultAsync(lib => lib.Guid == libraryGuid);

        if (libraryDbModel is null)
        {
            ModelState.AddModelError("Guid", $"Couldn't find any library with guid '{libraryGuid}'!");
            return BadRequest(ModelState);
        }

        string userGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (userGuid != libraryDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the library!");
            return BadRequest(ModelState);
        }

        if (libraryDbModel.Guid == libraryDbModel.Owner.DefaultLibraryGuid)
        {
            ModelState.AddModelError("Default Library", $"Cannot Delete default library");
            return BadRequest(ModelState);
        }

        //default library
        var defaultLibrary = libraryDbModel.Owner.Libraries
        .FirstOrDefault(lib => lib.Guid == libraryDbModel.Owner.DefaultLibraryGuid)!;

        //remove the library
        libraryDb.Libraries.Remove(libraryDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        libraryProcess.Delete_LibraryDirectory(libraryDbModel.Guid);

        //set the delault library as the parent of its non-parent shelves
        foreach (var shelf in libraryDbModel.Shelves)
        {
            if (shelf.ParentLibraries.Count == 0)
            {
                shelf.ParentLibraries = [defaultLibrary];
            }
        }
        await libraryDb.SaveChangesAsync();

        //seed
        string[] shelvesGuids = libraryDbModel.Shelves.Select(sh => sh.Guid).ToArray();
        await libraryProcess.Update_ShelvesSeeds(shelvesGuids, libraryDb);

        DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
        if (libraryDirectoryInfo.Exists)
        {
            libraryDirectoryInfo.Delete(true);
        }

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> ShelfList([FromQuery][StringLength(32)] string libraryGuid)
    {
        Library_ShelfCardModel[] shelfCardModels = await libraryDb.Libraries
        .Include(lib => lib.Owner)
        .Include(lib => lib.Shelves)
        .ThenInclude(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Elements)
        .Where(lib => lib.Guid == libraryGuid)
        .SelectMany(lib => lib.Shelves)
        .Select(shelf => new Library_ShelfCardModel()
        {
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            DocumentCardModels = shelf.Documents
            .Take(10)
            .Select(doc => new Library_DocumentCardModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.Owner.Guid,
                Title = doc.Title,
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
            }).ToArray(),
            Guid = shelf.Guid,
            Libraries = shelf.ParentLibraries.Select(shelfLib => new Library_LibraryBrief()
            {
                Guid = shelfLib.Guid,
                Title = shelfLib.Title,
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
    public async Task<IActionResult> UserShelfList([FromQuery][StringLength(32)] string ownerGuid)
    {
        var userShelfModels = await libraryDb.Owners
        .Include(owner => owner.Shelves)
        .ThenInclude(shelf => shelf.ParentLibraries)
        .Include(owner => owner.Shelves)
        .ThenInclude(shelf => shelf.Documents)
        .Where(owner => owner.Guid == ownerGuid)
        .SelectMany(owner => owner.Shelves)
        .Select(shelf => new
        {
            shelf.Guid,
            shelf.Title,
            Libraries = shelf.ParentLibraries.Select(shelfLib => new Library_LibraryBrief()
            {
                Guid = shelfLib.Guid,
                Title = shelfLib.Title,
            }).ToArray(),
            Documents = shelf.Documents.Select(doc => new Library_DocumentBrief()
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
        Library_ShelfCardModel? shelfCardModel = await libraryDb.Shelves
        .Include(shelf => shelf.Owner)
        .Include(shelf => shelf.ParentLibraries)
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Elements)
        .Where(shelf => shelf.Guid == shelfGuid)
        .Select(shelf => new Library_ShelfCardModel()
        {
            CreatedAt = shelf.CreatedAt,
            Description = shelf.Description,
            DocumentCardModels = shelf.Documents
            .Take(10)
            .Select(doc => new Library_DocumentCardModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.Owner.Guid,
                Title = doc.Title,
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
            }).ToArray(),
            Guid = shelf.Guid,
            Libraries = shelf.ParentLibraries.Select(shelfLib => new Library_LibraryBrief()
            {
                Guid = shelfLib.Guid,
                Title = shelfLib.Title,
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
        Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
        .Include(shelf => shelf.Owner)
        .ThenInclude(owner => owner.Shelves)
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.ParentShelves)
        .FirstOrDefaultAsync(shelf => shelf.Guid == shelfGuid);

        if (shelfDbModel is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified shelf!");
            return BadRequest(ModelState);
        }

        string userGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (userGuid != shelfDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the shelf!");
            return BadRequest(ModelState);
        }

        if (shelfDbModel.Guid == shelfDbModel.Owner.DefaultShelfGuid)
        {
            ModelState.AddModelError("Default Shelf", "Cannot delete the default shelf!");
            return BadRequest(ModelState);
        }

        //default shelf
        var defaultShelf = shelfDbModel.Owner.Shelves
        .FirstOrDefault(shelf => shelf.Guid == shelfDbModel.Owner.DefaultShelfGuid)!;

        //remove the shelf
        libraryDb.Shelves.Remove(shelfDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        libraryProcess.Delete_ShelfDirectory(shelfDbModel.Guid);

        //set the default shelf as the parent of its non-parent documents
        foreach (var doc in shelfDbModel.Documents)
        {
            if (doc.ParentShelves.Count == 0)
            {
                doc.ParentShelves = [defaultShelf];
            }
        }
        await libraryDb.SaveChangesAsync();

        //seed
        string[] documentsGuids = shelfDbModel.Documents.Select(doc => doc.Guid).ToArray();
        await libraryProcess.Update_DocumentsSeeds(documentsGuids, libraryDb);

        DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
        if (shelfDirectoryInfo.Exists)
        {
            shelfDirectoryInfo.Delete(true);
        }

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> DocumentCardList([FromQuery][StringLength(32)] string shelfGuid)
    {
        Library_DocumentCardModel[] documentCardModels = await libraryDb.Shelves
        .Include(shelf => shelf.Owner)
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Elements)
        .Where(shelf => shelf.Guid == shelfGuid)
        .SelectMany(shelf => shelf.Documents)
        .Select(doc => new Library_DocumentCardModel()
        {
            Description = doc.Description,
            Guid = doc.Guid,
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
            OwnerGuid = doc.Owner.Guid,
            Title = doc.Title,
            HasImage = doc.HasImage,
            IntegrityVersion = doc.IntegrityVersion,
        })
        .AsSplitQuery()
        .ToArrayAsync();

        return Ok(documentCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> DocumentCardModel([FromQuery][StringLength(32)] string documentGuid)
    {
        Library_DocumentCardModel? documentCardModel = await libraryDb.Documents
        .Include(doc => doc.Owner)
        .Include(doc => doc.Elements)
        .Where(doc => doc.Guid == documentGuid)
        .Select(doc => new Library_DocumentCardModel()
        {
            Guid = doc.Guid,
            Description = doc.Description,
            Title = doc.Title,
            OwnerGuid = doc.Owner.Guid,
            Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
            HasImage = doc.HasImage,
            IntegrityVersion = doc.IntegrityVersion,
        })
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
        Library_DocumentPageModel? documentPageModel = await libraryDb.Documents
        .Include(doc => doc.Owner)
        .Include(doc => doc.Tags)
        .Include(doc => doc.Elements)
        .Include(doc => doc.RelatedVersions)
            .ThenInclude(rv => rv!.Documents)
        .Include(doc => doc.ParentShelves)
            .ThenInclude(shelf => shelf.Documents)
        .Include(doc => doc.ParentShelves)
            .ThenInclude(shelf => shelf.ParentLibraries)
        .Where(doc => doc.Guid == documentGuid)
        .Select(doc => new Library_DocumentPageModel()
        {
            CreatedAt = doc.CreatedAt,
            Description = doc.Description,
            Guid = doc.Guid,
            HasImage = doc.HasImage,
            Tags = doc.Tags.Select(tag => tag.Name).ToArray(),
            Title = doc.Title,
            Version = doc.Version,
            Owner = new Library_OwnerBrief()
            {
                UserGuid = doc.Owner.Guid,
                UserName = "_",
            },
            Elements = doc.Elements.Select(el => new Library_ElementModel()
            {
                Guid = el.Guid,
                Order = el.Order,
                OwnerGuid = el.Owner.Guid,
                Title = el.Title,
                Type = el.Type,
                UpdatedAt = el.UpdatedAt,
                Value = el.Value ??
                    $"/api/Library/ElementFile?elementGuid={el.Guid}&elementFileName={el.FileName}",
            }).ToArray(),
            RelatedVersions = doc.RelatedVersions == null ?
            new Library_VersionBrief[0] :
            doc.RelatedVersions!.Documents.Select(rvDoc => new Library_VersionBrief()
            {
                DocumentGuid = rvDoc.Guid,
                VersionName = rvDoc.Version,
            }).ToArray(),
            Shelves = doc.ParentShelves.Select(shelf => new Library_ShelfBrief()
            {
                Documents = shelf.Documents.Select(shelfDoc => new Library_DocumentBrief()
                {
                    Guid = shelfDoc.Guid,
                    Title = shelfDoc.Title,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.ParentLibraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Title = shelf.Title,
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


        documentPageModel.Owner.UserName = (await userManager.Users
        .Where(u => u.UserGuid == documentPageModel.Owner.UserGuid)
        .Select(u => u.UserName)
        .FirstOrDefaultAsync())!;

        return Ok(documentPageModel);
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument([FromQuery][StringLength(32)] string documentGuid,
    [FromServices] Review_Process reviewProcess, [FromServices] Review_DbContext reviewDb)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .Include(doc => doc.Owner)
        .Include(doc => doc.Elements)
        .AsSplitQuery()
        .FirstOrDefaultAsync(doc => doc.Guid == documentGuid);

        if (documentDbModel is null)
        {
            return NotFound($"There's no document with guid '{documentGuid}'!");
        }

        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        if (user.UserGuid != documentDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the author of the document is allowed to delete it!");
            return BadRequest(ModelState);
        }

        //remove from Db
        libraryDb.Documents.Remove(documentDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        libraryProcess.Delete_DocumentDirectory(documentDbModel.Guid);
        //delete directory path of elements from storage
        foreach (string elementGuid in documentDbModel.Elements.Select(el => el.Guid))
        {
            libraryProcess.Delete_ElementDirectory(elementGuid);
        }

        //delete review
        Review_ReviewDbModel? reviewDbModel = await reviewDb.Reviews
        .FirstOrDefaultAsync(r => r.SubjectGuid == documentDbModel.Guid);
        if (reviewDbModel is not null)
        {
            //first delete directories
            await reviewProcess.DeleteReviewAndCommentsDirectories(reviewDb, documentDbModel.Guid);
            //then remove from db
            reviewDb.Reviews.Remove(reviewDbModel);
            await reviewDb.SaveChangesAsync();
        }

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
    public async Task<IActionResult> EditElements([FromBody] Library_EditElementFormModel[] formModels)
    {
        if (ModelState.IsValid)
        {
            IEnumerable<string> elementGuids = formModels.Select(m => m.Guid);

            /*List<Library_ElementDbModel>*/
            var elementDbModels = await libraryDb.Elements
            .Include(el => el.Owner)
            .Include(el => el.ParentDocument)
            .Where(el => elementGuids.Contains(el.Guid))
            .AsSplitQuery()
            .ToListAsync();

            //make sure all edited elements belong to the same document
            IEnumerable<string> parentDocumentGuids = elementDbModels.Select(el => el.ParentDocument.Guid).Distinct();
            if (parentDocumentGuids.Count() > 1)
            {
                ModelState.AddModelError("ParentDocument", "The edited elements don't belong to the same parent document!");
                return BadRequest(ModelState);
            }

            string ownerGuid = (await userManager.Users
            .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(user => user.UserGuid)
            .FirstOrDefaultAsync())!;

            foreach (var elementDbModel in elementDbModels)
            {
                if (elementDbModel.Owner.Guid != ownerGuid)
                {
                    ModelState.AddModelError("Owner", "You are Not the owner of all of the edited elements!");
                    return BadRequest(ModelState);
                }
            }

            bool needToReorder = false;
            foreach (var elementDbModel in elementDbModels)
            {
                var formModel = formModels.First(fm => fm.Guid == elementDbModel.Guid);
                if (formModel.Delete ?? false)
                {
                    libraryDb.Elements.Remove(elementDbModel);
                    needToReorder = true;
                }
                else
                {
                    elementDbModel.Order = formModel.Order ?? elementDbModel.Order;
                    elementDbModel.Title = formModel.Title ?? elementDbModel.Title;
                    elementDbModel.Value = formModel.Value ?? elementDbModel.Value;
                }
            }

            await libraryDb.SaveChangesAsync();

            if (needToReorder)
            {
                var result = await libraryProcess.ReorderElements(libraryDb, parentDocumentGuids.Single());
                if (result.Success && result.ResultObject is not null)
                {
                    elementDbModels = (List<Library_ElementDbModel>)result.ResultObject;
                }
            }

            //seed
            foreach (var elementDbModel in elementDbModels)
            {
                var formModel = formModels.First(fm => fm.Guid == elementDbModel.Guid);
                if (formModel.Delete ?? false)
                {
                    libraryProcess.Delete_ElementDirectory(elementDbModel.Guid);
                }
                else
                {
                    await libraryProcess.Update_ElementSeed(elementDbModel.Guid, libraryDb);
                }
            }

            //create response
            Library_ElementModel[] elementModelArray = elementDbModels
            .Select(elementDbModel => new Library_ElementModel()
            {
                Guid = elementDbModel.Guid,
                Order = elementDbModel.Order,
                OwnerGuid = elementDbModel.Owner.Guid,
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
    public async Task<IActionResult> TotalNumberOfDocuments([FromQuery][StringLength(32)] string ownerGuid)
    {
        int totalNumberOfUserDocuments = await libraryDb.Owners
        .Include(owner => owner.Documents)
        .Where(owner => owner.Guid == ownerGuid)
        .Select(owner => owner.Documents.Count)
        .FirstOrDefaultAsync();

        return Ok(new { totalNumberOfUserDocuments });
    }

    [HttpGet]
    public async Task<IActionResult> TotalNumberOfShelves([FromQuery][StringLength(32)] string ownerGuid)
    {
        int totalNumberOfUserShelves = await libraryDb.Owners
        .Include(owner => owner.Shelves)
        .Where(owner => owner.Guid == ownerGuid)
        .Select(owner => owner.Shelves.Count)
        .FirstOrDefaultAsync();

        return Ok(new { totalNumberOfUserShelves });
    }





    [HttpPost]
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewLibrary(Library_NewLibraryFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
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
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewShelf(Library_NewShelfFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
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
    public async Task<IActionResult> CreateNewDocument(Library_NewDocumentFormModel formModel,
    [FromServices] Review_Process reviewProcess, [FromServices] Review_DbContext reviewDb)
    {
        if (ModelState.IsValid)
        {
            string myGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            var result = await libraryProcess.CreateNewDocument(libraryDb, myGuid, formModel);
            if (result.Success && result.ResultObject is not null)
            {
                Library_DocumentDbModel documentDbModel = (Library_DocumentDbModel)result.ResultObject;

                /*Review_ReviewDbModel reviewDbModel = new()
                {
                    SubjectGuid = documentDbModel.Guid,
                    SubjectOwnerGuid = myGuid,
                };
                await reviewDb.Reviews.AddAsync(reviewDbModel);
                await reviewDb.SaveChangesAsync();*/
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
    public async Task<IActionResult> CreateNewElement(Library_NewElementFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
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
                    OwnerGuid = elementDbModel.Owner.Guid,
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





    [HttpPost]
    [Authorize]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentIntroduction(
        [FromForm] Library_EditIntroductionFormModel formModel)
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

            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (userGuid != documentDbModel.Owner.Guid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the document!");
                return BadRequest(ModelState);
            }

            documentDbModel.Title = formModel.Title;
            documentDbModel.Description = formModel.Description;

            if (formModel.Image is not null)
            {
                DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, documentDbModel.Guid));
                string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(documentImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }

                documentDbModel.HasImage = true;
                documentDbModel.IntegrityVersion += 1;
            }

            await libraryDb.SaveChangesAsync();

            //seed
            await libraryProcess.Update_DocumentSeed(documentDbModel.Guid, libraryDb);

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
        [FromForm] Library_EditIntroductionFormModel formModel)
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

            string userGuid = (await userManager.Users
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
                DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
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
            await libraryProcess.Update_LibrarySeed(libraryDbModel.Guid, libraryDb);

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
        [FromForm] Library_EditIntroductionFormModel formModel)
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

            string userGuid = (await userManager.Users
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
                DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
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
            await libraryProcess.Update_ShelfSeed(shelfDbModel.Guid, libraryDb);

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
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .Include(doc => doc.Owner)
        .FirstOrDefaultAsync(doc => doc.Guid == documentGuid);
        if (documentDbModel is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified document!");
            return BadRequest(ModelState);
        }

        string userGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (userGuid != documentDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the image of the document's introduction!");
            return BadRequest(ModelState);
        }

        DirectoryInfo documentDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Documents.FullName, documentDbModel.Guid));
        string documentImagePath = Path.Combine(documentDirectoryInfo.FullName, "image");
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

        documentDbModel.HasImage = false;
        documentDbModel.IntegrityVersion = 0;
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_DocumentSeed(documentDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLibraryIntroductionImage([FromQuery][StringLength(32)]
    string libraryGuid)
    {
        Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
        .Include(lib => lib.Owner)
        .FirstOrDefaultAsync(lib => lib.Guid == libraryGuid);
        if (libraryDbModel is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified library!");
            return BadRequest(ModelState);
        }

        string userGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (userGuid != libraryDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the image of the library's introduction!");
            return BadRequest(ModelState);
        }

        DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
        string libraryImagePath = Path.Combine(libraryDirectoryInfo.FullName, "image");
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

        libraryDbModel.HasImage = false;
        libraryDbModel.IntegrityVersion = 0;
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_LibrarySeed(libraryDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShelfIntroductionImage([FromQuery][StringLength(32)]
    string shelfGuid)
    {
        Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
        .Include(shelf => shelf.Owner)
            .FirstOrDefaultAsync(doc => doc.Guid == shelfGuid);
        if (shelfDbModel is null)
        {
            ModelState.AddModelError("Guid", "Couldn't find the specified shelf!");
            return BadRequest(ModelState);
        }

        string userGuid = (await userManager.Users
        .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
        .Select(u => u.UserGuid)
        .FirstOrDefaultAsync())!;
        if (userGuid != shelfDbModel.Owner.Guid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the image of the shelf's introduction!");
            return BadRequest(ModelState);
        }

        DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
        string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
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

        shelfDbModel.HasImage = false;
        shelfDbModel.IntegrityVersion = 0;
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_ShelfSeed(shelfDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentParentShelves([FromBody]
    Library_DocumentParentShelvesFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
            .Include(doc => doc.Owner)
            .ThenInclude(owner => owner.Shelves)
            .ThenInclude(shelf => shelf.ParentLibraries)
            .Include(doc => doc.Owner)
            .ThenInclude(owner => owner.Shelves)
            .ThenInclude(shelf => shelf.Documents)
            .Include(doc => doc.ParentShelves)
            .AsSplitQuery()
            .FirstOrDefaultAsync(doc =>
                doc.Guid == formModel.DocumentGuid
            );
            if (documentDbModel is null)
            {
                ModelState.AddModelError("document", "Couldn't find the specified document for the user!");
                return BadRequest(ModelState);
            }

            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (userGuid != documentDbModel.Owner.Guid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the parent shelves of the document!");
                return BadRequest(ModelState);
            }

            List<Library_ShelfDbModel> parentShelfDbModels = documentDbModel.Owner.Shelves
            .Where(shelf => formModel.ShelfGuids.Contains(shelf.Guid))
            .ToList();

            documentDbModel.ParentShelves = parentShelfDbModels;
            await libraryDb.SaveChangesAsync();

            //seed
            await libraryProcess.Update_DocumentSeed(documentDbModel.Guid, libraryDb);

            var parentShelves = parentShelfDbModels.Select(shelf => new
            {
                shelf.Guid,
                shelf.Title,
                Libraries = shelf.ParentLibraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Documents = shelf.Documents.Select(doc => new Library_DocumentBrief()
                {
                    Guid = doc.Guid,
                    Title = doc.Title,
                }).ToArray(),
            }).ToArray();

            return Ok(new { success = true, parentShelves });
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShelfParentLibraries([FromBody]
    Library_ShelfParentLibrariesFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
            .Include(shelf => shelf.Owner)
            .ThenInclude(owner => owner.Libraries)
            .Include(shelf => shelf.ParentLibraries)
            .AsSplitQuery()
            .FirstOrDefaultAsync(doc => doc.Guid == formModel.ShelfGuid);
            if (shelfDbModel is null)
            {
                ModelState.AddModelError("shelf", "Couldn't find the specified shelf for the user!");
                return BadRequest(ModelState);
            }

            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;
            if (userGuid != shelfDbModel.Owner.Guid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the parent libraries of the shelf!");
                return BadRequest(ModelState);
            }

            if (shelfDbModel.Guid == shelfDbModel.Owner.DefaultShelfGuid)
            {
                ModelState.AddModelError("Default Shelf", "Cannot edit the default shelf!");
                return BadRequest(ModelState);
            }

            List<Library_LibraryDbModel> parentLibraryDbModels = shelfDbModel.Owner.Libraries
            .Where(lib => formModel.LibraryGuids.Contains(lib.Guid))
            .ToList();

            shelfDbModel.ParentLibraries = parentLibraryDbModels;

            await libraryDb.SaveChangesAsync();

            //seed
            await libraryProcess.Update_ShelfSeed(shelfDbModel.Guid, libraryDb);

            return Ok(new { success = true });
        }

        return BadRequest(ModelState);
    }





    [HttpGet]
    public async Task<IActionResult> GetOwnerModel([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerModel? ownerModel = await userManager.Users
        .Where(u => u.UserGuid == ownerGuid)
        .Select(user => new Library_OwnerModel()
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
    public async Task<IActionResult> GetFollowers([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .Include(owner => owner.Followers)
        .FirstOrDefaultAsync(owner => owner.Guid == ownerGuid);

        if (ownerDbModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        List<string> followersGuids = ownerDbModel.Followers.Select(f => f.Guid).ToList();

        Library_OwnerModel[] followers_OwnerModel = await userManager.Users
        .Where(user => followersGuids.Contains(user.UserGuid))
        .Select(user => new Library_OwnerModel()
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
    public async Task<IActionResult> GetFollowings([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .Include(owner => owner.Followings)
        .FirstOrDefaultAsync(owner => owner.Guid == ownerGuid);

        if (ownerDbModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        List<string> followingsGuids = ownerDbModel.Followings.Select(f => f.Guid).ToList();

        Library_OwnerModel[] followings_OwnerModel = await userManager.Users
        .Where(user => followingsGuids.Contains(user.UserGuid))
        .Select(user => new Library_OwnerModel()
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
    public async Task<IActionResult> Follow([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerDbModel? followingDbModel = await libraryDb.Owners
        .FirstOrDefaultAsync(owner => owner.Guid == ownerGuid);

        if (followingDbModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        string followerGuid = (await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstOrDefaultAsync())!;

        Library_OwnerDbModel followerDbModel = (await libraryDb.Owners
        .Include(owner => owner.Followings)
        .FirstOrDefaultAsync(owner => owner.Guid == followerGuid))!;

        followerDbModel.Followings.Add(followingDbModel);
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_OwnerSeed(followerDbModel.Guid, libraryDb);
        await libraryProcess.Update_OwnerSeed(followingDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnFollow([FromQuery][StringLength(32)] string ownerGuid)
    {
        string followerGuid = (await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstOrDefaultAsync())!;

        Library_OwnerDbModel followerDbModel = (await libraryDb.Owners
        .Include(owner => owner.Followings)
        .FirstOrDefaultAsync(owner => owner.Guid == followerGuid))!;

        Library_OwnerDbModel? followingDbModel = followerDbModel.Followings
        .FirstOrDefault(owner => owner.Guid == ownerGuid);

        if (followingDbModel is not null)
        {
            followerDbModel.Followings.Remove(followingDbModel);
            await libraryDb.SaveChangesAsync();

            //seed
            await libraryProcess.Update_OwnerSeed(followerDbModel.Guid, libraryDb);
            await libraryProcess.Update_OwnerSeed(followingDbModel.Guid, libraryDb);
        }

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> GetFavoriteLibraries([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .Include(owner => owner.FavoriteLibraries)
        .ThenInclude(lib => lib.Owner)
        //.AsSplitQuery()
        .FirstOrDefaultAsync(owner => owner.Guid == ownerGuid);

        if (ownerDbModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        Library_FavoriteModel[] favoriteLibraries = ownerDbModel.FavoriteLibraries
        .Select(lib => new Library_FavoriteModel()
        {
            Guid = lib.Guid,
            HasImage = lib.HasImage,
            IntegrityVersion = lib.IntegrityVersion,
            Title = lib.Title,
            Owner = new Library_OwnerModel()
            {
                Guid = lib.Owner.Guid,
                Username = "_",
            }
        })
        .ToArray();

        List<string> favoriteLibrariesOwnersGuids = favoriteLibraries
        .Select(fl => fl.Owner.Guid).ToList();

        List<Identity_UserDbModel> favoriteLibrariesOwners = await userManager.Users
        .Where(user => favoriteLibrariesOwnersGuids.Contains(user.UserGuid))
        .ToListAsync();

        foreach (var favLib in favoriteLibraries)
        {
            var userDbModel = favoriteLibrariesOwners
            .FirstOrDefault(u => u.UserGuid == favLib.Owner.Guid);
            if (userDbModel is not null)
            {
                favLib.Owner.Username = userDbModel.UserName!;
                favLib.Owner.HasImage = userDbModel.HasImage!;
                favLib.Owner.IntegrityVersion = userDbModel.IntegrityVersion!;
            }
        }

        return Ok(favoriteLibraries);
    }
    [HttpGet]
    public async Task<IActionResult> GetFavoriteShelves([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .Include(owner => owner.FavoriteShelves)
        .ThenInclude(shelf => shelf.Owner)
        //.AsSplitQuery()
        .FirstOrDefaultAsync(owner => owner.Guid == ownerGuid);

        if (ownerDbModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        Library_FavoriteModel[] favoriteShelves = ownerDbModel.FavoriteShelves
        .Select(lib => new Library_FavoriteModel()
        {
            Guid = lib.Guid,
            HasImage = lib.HasImage,
            IntegrityVersion = lib.IntegrityVersion,
            Title = lib.Title,
            Owner = new Library_OwnerModel()
            {
                Guid = lib.Owner.Guid,
                Username = "_",
            }
        })
        .ToArray();

        List<string> favoriteShelvesOwnersGuids = favoriteShelves
        .Select(fl => fl.Owner.Guid).ToList();

        List<Identity_UserDbModel> favoriteShelvesOwners = await userManager.Users
        .Where(user => favoriteShelvesOwnersGuids.Contains(user.UserGuid))
        .ToListAsync();

        foreach (var fl in favoriteShelves)
        {
            var userDbModel = favoriteShelvesOwners
            .FirstOrDefault(u => u.UserGuid == fl.Owner.Guid);
            if (userDbModel is not null)
            {
                fl.Owner.Username = userDbModel.UserName!;
                fl.Owner.HasImage = userDbModel.HasImage!;
                fl.Owner.IntegrityVersion = userDbModel.IntegrityVersion!;
            }
        }

        return Ok(favoriteShelves);
    }
    [HttpGet]
    public async Task<IActionResult> GetFavoriteDocuments([FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_OwnerDbModel? ownerDbModel = await libraryDb.Owners
        .Include(owner => owner.FavoriteDocuments)
        .ThenInclude(doc => doc.Owner)
        //.AsSplitQuery()
        .FirstOrDefaultAsync(owner => owner.Guid == ownerGuid);

        if (ownerDbModel is null)
        {
            ModelState.AddModelError("user", "the specified owner Not found!");
            return BadRequest(ModelState);
        }

        Library_FavoriteModel[] favoriteDocuments = ownerDbModel.FavoriteDocuments
        .Select(lib => new Library_FavoriteModel()
        {
            Guid = lib.Guid,
            HasImage = lib.HasImage,
            IntegrityVersion = lib.IntegrityVersion,
            Title = lib.Title,
            Owner = new Library_OwnerModel()
            {
                Guid = lib.Owner.Guid,
                Username = "_",
            }
        })
        .ToArray();

        List<string> favoriteDocumentsOwnersGuids = favoriteDocuments
        .Select(fl => fl.Owner.Guid).ToList();

        List<Identity_UserDbModel> favoriteDocumentsOwners = await userManager.Users
        .Where(user => favoriteDocumentsOwnersGuids.Contains(user.UserGuid))
        .ToListAsync();

        foreach (var fl in favoriteDocuments)
        {
            var userDbModel = favoriteDocumentsOwners
            .FirstOrDefault(u => u.UserGuid == fl.Owner.Guid);
            if (userDbModel is not null)
            {
                fl.Owner.Username = userDbModel.UserName!;
                fl.Owner.HasImage = userDbModel.HasImage!;
                fl.Owner.IntegrityVersion = userDbModel.IntegrityVersion!;
            }
        }

        return Ok(favoriteDocuments);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersInFavorOfLibrary([FromQuery][StringLength(32)] string libraryGuid)
    {
        Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
        .Include(lib => lib.InFavorOf)
        .FirstOrDefaultAsync(lib => lib.Guid == libraryGuid);
        if (libraryDbModel is null)
        {
            ModelState.AddModelError("library", "the specified library Not found!");
            return BadRequest(ModelState);
        }

        List<string> usersInFavorOf_Guids = libraryDbModel.InFavorOf
        .Select(user => user.Guid).ToList();

        List<Identity_UserDbModel> usersInFavorOf_DbModels = await userManager.Users
        .Where(user => usersInFavorOf_Guids.Contains(user.UserGuid))
        .ToListAsync();

        Library_OwnerModel[] usersInFavorOf = usersInFavorOf_DbModels
        .Select(user => new Library_OwnerModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArray();

        return Ok(usersInFavorOf);
    }
    [HttpGet]
    public async Task<IActionResult> GetUsersInFavorOfShelf([FromQuery][StringLength(32)] string shelfGuid)
    {
        Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
        .Include(shelf => shelf.InFavorOf)
        .FirstOrDefaultAsync(lib => lib.Guid == shelfGuid);
        if (shelfDbModel is null)
        {
            ModelState.AddModelError("shelf", "the specified shelf Not found!");
            return BadRequest(ModelState);
        }

        List<string> usersInFavorOf_Guids = shelfDbModel.InFavorOf
        .Select(user => user.Guid).ToList();

        List<Identity_UserDbModel> usersInFavorOf_DbModels = await userManager.Users
        .Where(user => usersInFavorOf_Guids.Contains(user.UserGuid))
        .ToListAsync();

        Library_OwnerModel[] usersInFavorOf = usersInFavorOf_DbModels
        .Select(user => new Library_OwnerModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArray();

        return Ok(usersInFavorOf);
    }
    [HttpGet]
    public async Task<IActionResult> GetUsersInFavorOfDocument([FromQuery][StringLength(32)] string documentGuid)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .Include(doc => doc.InFavorOf)
        .FirstOrDefaultAsync(lib => lib.Guid == documentGuid);
        if (documentDbModel is null)
        {
            ModelState.AddModelError("document", "the specified document Not found!");
            return BadRequest(ModelState);
        }

        List<string> usersInFavorOf_Guids = documentDbModel.InFavorOf
        .Select(user => user.Guid).ToList();

        List<Identity_UserDbModel> usersInFavorOf_DbModels = await userManager.Users
        .Where(user => usersInFavorOf_Guids.Contains(user.UserGuid))
        .ToListAsync();

        Library_OwnerModel[] usersInFavorOf = usersInFavorOf_DbModels
        .Select(user => new Library_OwnerModel()
        {
            Guid = user.UserGuid,
            HasImage = user.HasImage,
            IntegrityVersion = user.IntegrityVersion,
            Username = user.UserName!,
        })
        .ToArray();

        return Ok(usersInFavorOf);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteLibrary([FromQuery][StringLength(32)] string libraryGuid)
    {
        Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
        .FirstOrDefaultAsync(lib => lib.Guid == libraryGuid);
        if (libraryDbModel is null)
        {
            ModelState.AddModelError("library", "the specified library Not found!");
            return BadRequest(ModelState);
        }

        string myGuid = (await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstOrDefaultAsync())!;

        Library_OwnerDbModel myDbModel = (await libraryDb.Owners
        .Include(owner => owner.FavoriteLibraries)
        .FirstOrDefaultAsync(owner => owner.Guid == myGuid))!;

        if (!myDbModel.FavoriteLibraries.Remove(libraryDbModel))
        {
            myDbModel.FavoriteLibraries.Add(libraryDbModel);
        }
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_LibrarySeed(libraryDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteShelf([FromQuery][StringLength(32)] string shelfGuid)
    {
        Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
        .FirstOrDefaultAsync(shelf => shelf.Guid == shelfGuid);
        if (shelfDbModel is null)
        {
            ModelState.AddModelError("shelf", "the specified shelf Not found!");
            return BadRequest(ModelState);
        }

        string myGuid = (await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstOrDefaultAsync())!;

        Library_OwnerDbModel myDbModel = (await libraryDb.Owners
        .Include(owner => owner.FavoriteShelves)
        .FirstOrDefaultAsync(owner => owner.Guid == myGuid))!;

        if (!myDbModel.FavoriteShelves.Remove(shelfDbModel))
        {
            myDbModel.FavoriteShelves.Add(shelfDbModel);
        }
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_ShelfSeed(shelfDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavoriteDocument([FromQuery][StringLength(32)] string documentGuid)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .FirstOrDefaultAsync(doc => doc.Guid == documentGuid);
        if (documentDbModel is null)
        {
            ModelState.AddModelError("document", "the specified document Not found!");
            return BadRequest(ModelState);
        }

        string myGuid = (await userManager.Users
        .Where(user => user.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name!))
        .Select(user => user.UserGuid)
        .FirstOrDefaultAsync())!;

        Library_OwnerDbModel myDbModel = (await libraryDb.Owners
        .Include(owner => owner.FavoriteDocuments)
        .FirstOrDefaultAsync(owner => owner.Guid == myGuid))!;

        if (!myDbModel.FavoriteDocuments.Remove(documentDbModel))
        {
            myDbModel.FavoriteDocuments.Add(documentDbModel);
        }
        await libraryDb.SaveChangesAsync();

        //seed
        await libraryProcess.Update_DocumentSeed(documentDbModel.Guid, libraryDb);

        return Ok(new { success = true });
    }




}