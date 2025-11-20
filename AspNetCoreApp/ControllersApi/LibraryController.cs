using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
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
    readonly DirectoryInfo Storage_Libraries;
    readonly DirectoryInfo Storage_Shelves;
    readonly DirectoryInfo Storage_Documents;
    readonly DirectoryInfo Storage_Elements;
    readonly Library_Process libraryProcess;
    //readonly Identity_Process identityProcess;




    public LibraryController(Library_DbContext _libraryDb, UserManager<Identity_UserDbModel> _userManager,
    Library_Process _libraryProcess, Identity_Process _identityProcess)
    {
        libraryDb = _libraryDb;
        userManager = _userManager;
        Storage_Libraries = _libraryProcess.Storage_Libraries;
        Storage_Shelves = _libraryProcess.Storage_Shelves;
        Storage_Documents = _libraryProcess.Storage_Documents;
        Storage_Elements = _libraryProcess.Storage_Elements;
        libraryProcess = _libraryProcess;
        //identityProcess = _identityProcess;
    }





    [HttpGet]
    public async Task<IActionResult> List([FromQuery][StringLength(32)] string userGuid)
    {
        Library_LibraryCardModel[] libraryCardModels = await libraryDb.Libraries
        .Include(lib => lib.Shelves)
        .Where(lib => lib.OwnerGuid == userGuid)
        .Select(lib => new Library_LibraryCardModel()
        {
            Guid = lib.Guid,
            Title = lib.Title,
            Description = lib.Description,
            ShelvesTitles = lib.Shelves.Select(shelf => shelf.Title).ToArray(),
            CreatedAt = lib.CreatedAt,
            OwnerGuid = lib.OwnerGuid,
            IntegrityVersion = lib.IntegrityVersion,
            HasImage = lib.HasImage,
        })
        .ToArrayAsync();

        string[] nonParentShelvesTitles = await libraryDb.Shelves
        .Include(shelf => shelf.Libraries)
        .Where(shelf => shelf.OwnerGuid == userGuid && shelf.Libraries.Count == 0)
        .Select(shelf => shelf.Title)
        .ToArrayAsync();

        Library_LibraryCardModel? defaultLibrary =
        libraryCardModels.FirstOrDefault(lib => lib.Guid == "DefaultLibrary");

        if (defaultLibrary is not null)
        {
            defaultLibrary.ShelvesTitles = [.. defaultLibrary.ShelvesTitles, .. nonParentShelvesTitles];
        }

        return Ok(libraryCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> LibraryModel([FromQuery][StringLength(32)] string libraryGuid,
    [FromQuery][StringLength(32)] string? ownerGuid)
    {
        Library_LibraryCardModel? libraryCardModel;

        if (libraryGuid == "DefaultLibrary")
        {
            if (ownerGuid is null)
            {
                ModelState.AddModelError("ownerGuid", "ownerGuid cannot be null for the Default Library!");
                return BadRequest(ModelState);
            }

            libraryCardModel = await libraryDb.Libraries
            .Include(lib => lib.Shelves)
            .Where(lib => lib.Guid == libraryGuid && lib.OwnerGuid == ownerGuid)
            .Select(lib => new Library_LibraryCardModel()
            {
                Guid = lib.Guid,
                Title = lib.Title,
                Description = lib.Description,
                ShelvesTitles = lib.Shelves.Select(shelf => shelf.Title).ToArray(),
                OwnerGuid = lib.OwnerGuid,
                CreatedAt = lib.CreatedAt,
                IntegrityVersion = lib.IntegrityVersion,
                HasImage = lib.HasImage,
            })
            .FirstOrDefaultAsync();

            if (libraryCardModel is null)
            {
                var result = await libraryProcess.CreateDefaultShelf(libraryDb, ownerGuid);//default shelf automatically creates default library
                if (!result.Success)
                {
                    return NotFound("Couldn't find and create default shelf and library");
                }

                libraryCardModel = await libraryDb.Libraries
                .Include(lib => lib.Shelves)
                .Where(lib => lib.Guid == libraryGuid && lib.OwnerGuid == ownerGuid)
                .Select(lib => new Library_LibraryCardModel()
                {
                    Guid = lib.Guid,
                    Title = lib.Title,
                    Description = lib.Description,
                    ShelvesTitles = lib.Shelves.Select(shelf => shelf.Title).ToArray(),
                    OwnerGuid = lib.OwnerGuid,
                    CreatedAt = lib.CreatedAt,
                    IntegrityVersion = lib.IntegrityVersion,
                    HasImage = lib.HasImage,
                })
                .FirstOrDefaultAsync();

                if (libraryCardModel is null)
                {
                    return NotFound("Couldn't find and create default shelf and library");
                }
            }

            string[] nonParentShelvesTitles = await libraryDb.Shelves
            .Include(shelf => shelf.Libraries)
            .Where(shelf => shelf.OwnerGuid == ownerGuid && shelf.Libraries.Count == 0)
            .Select(shelf => shelf.Title)
            .ToArrayAsync();

            libraryCardModel.ShelvesTitles = [.. libraryCardModel.ShelvesTitles, .. nonParentShelvesTitles];

        }
        else
        {
            libraryCardModel = await libraryDb.Libraries
            .Include(lib => lib.Shelves)
            .Where(lib => lib.Guid == libraryGuid)
            .Select(lib => new Library_LibraryCardModel()
            {
                Guid = lib.Guid,
                Title = lib.Title,
                Description = lib.Description,
                ShelvesTitles = lib.Shelves.Select(shelf => shelf.Title).ToArray(),
                OwnerGuid = lib.OwnerGuid,
                CreatedAt = lib.CreatedAt,
                IntegrityVersion = lib.IntegrityVersion,
                HasImage = lib.HasImage,
            })
            .FirstOrDefaultAsync();
        }

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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLibrary([FromQuery][StringLength(32)] string libraryGuid)
    {
        if (libraryGuid == "DefaultLibrary")
        {
            ModelState.AddModelError("DefaultLibrary", "Default library cannot be deleted!");
            return BadRequest(ModelState);
        }

        Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
        .Include(lib => lib.Shelves)
        .ThenInclude(shelf => shelf.Libraries)
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
        if (userGuid != libraryDbModel.OwnerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the library!");
            return BadRequest(ModelState);
        }

        //default library
        var defaultLibrary = await libraryDb.Libraries
            .FirstOrDefaultAsync(lib => lib.OwnerGuid == userGuid && lib.Guid == "DefaultLibrary");
        if (defaultLibrary is null)
        {
            // creating Default library
            var createDefaultLibraryResult = await libraryProcess.CreateDefaultLibrary(libraryDb, userGuid);

            if (createDefaultLibraryResult.Success &&
            createDefaultLibraryResult.ResultObject is not null)
            {
                defaultLibrary = (Library_LibraryDbModel)createDefaultLibraryResult.ResultObject;
            }
            else
            {
                //log
                Console.WriteLine($"\n***** Cloudnt find and create default library for the user with guid '{userGuid}'!");
                ModelState.AddModelError(createDefaultLibraryResult.ErrorTitle ?? "defaultLibrary",
                createDefaultLibraryResult.ErrorDescription ?? $"Cloudnt find and create default library for the user with guid '{userGuid}'!");
                return BadRequest(ModelState);
            }
        }

        libraryDb.Libraries.Remove(libraryDbModel);

        foreach (var shelf in libraryDbModel.Shelves)
        {
            if (shelf.Libraries.Count == 0)
            {
                shelf.Libraries = [defaultLibrary];
            }
        }

        await libraryDb.SaveChangesAsync();

        DirectoryInfo libraryDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Libraries.FullName, libraryDbModel.Guid));
        if (libraryDirectoryInfo.Exists)
        {
            libraryDirectoryInfo.Delete(true);
        }

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> ShelfList([FromQuery][StringLength(32)] string libraryGuid,
    [FromQuery][StringLength(32)] string? ownerGuid)
    {
        Library_ShelfCardModel[] shelfCardModels;

        if (libraryGuid == "DefaultLibrary")
        {
            if (ownerGuid is null)
            {
                ModelState.AddModelError("ownerGuid", "ownerGuid cannot be null for the Default Library!");
                return BadRequest(ModelState);
            }

            shelfCardModels = await libraryDb.Libraries
            .Include(lib => lib.Shelves)
            .ThenInclude(shelf => shelf.Documents)
            .ThenInclude(doc => doc.Elements)
            .Where(lib => lib.Guid == libraryGuid && lib.OwnerGuid == ownerGuid)
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
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Title = shelf.Title,
                OwnerGuid = shelf.OwnerGuid,
                TotalNumberOfShelfDocuments = shelf.Documents.Count,
                HasImage = shelf.HasImage,
                IntegrityVersion = shelf.IntegrityVersion,
            })
            .AsSplitQuery()
            .ToArrayAsync();

            Library_ShelfCardModel[] nonParentShelfCardModels = await libraryDb.Shelves
            .Include(shelf => shelf.Libraries)
            .Include(shelf => shelf.Documents)
            .ThenInclude(doc => doc.Elements)
            .Where(shelf => shelf.OwnerGuid == ownerGuid && shelf.Libraries.Count == 0)
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
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Title = shelf.Title,
                OwnerGuid = shelf.OwnerGuid,
                TotalNumberOfShelfDocuments = shelf.Documents.Count,
                HasImage = shelf.HasImage,
                IntegrityVersion = shelf.IntegrityVersion,
            })
            .AsSplitQuery()
            .ToArrayAsync();

            shelfCardModels = [.. shelfCardModels, .. nonParentShelfCardModels];

            var defaultShelfCardModel = shelfCardModels.FirstOrDefault(shelf => shelf.Guid == "DefaultShelf");
            /*if (defaultShelfCardModel is null)
            {
                Console.WriteLine($"\n***** couldn't find default shelf for ownerGuid {ownerGuid}");
            }
            else*/
            if (defaultShelfCardModel is not null && defaultShelfCardModel.DocumentCardModels.Length < 10)
            {
                int numberOfNeededDocs = 10 - defaultShelfCardModel.DocumentCardModels.Length;

                Library_DocumentCardModel[] nonParentDocumentCardModels = await libraryDb.Documents
                .Include(doc => doc.Shelves)
                .Include(doc => doc.Elements)
                .Where(doc => doc.OwnerGuid == ownerGuid && doc.Shelves.Count == 0)
                .Take(numberOfNeededDocs)
                .Select(doc => new Library_DocumentCardModel()
                {
                    Description = doc.Description,
                    Guid = doc.Guid,
                    Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                })
                .AsSplitQuery()
                .ToArrayAsync();

                defaultShelfCardModel.DocumentCardModels = [
                    .. defaultShelfCardModel.DocumentCardModels,
                    .. nonParentDocumentCardModels//.Take(numberOfNeededDocs)
                ];

                int totalNonParentDocumentCardModels = await libraryDb.Documents
                .Include(doc => doc.Shelves)
                .Where(doc => doc.OwnerGuid == ownerGuid && doc.Shelves.Count == 0)
                .CountAsync();

                defaultShelfCardModel.TotalNumberOfShelfDocuments += totalNonParentDocumentCardModels;
            }

        }
        else//libraryGuid != "DefaultLibrary"
        {
            shelfCardModels = await libraryDb.Libraries
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
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Title = shelf.Title,
                OwnerGuid = shelf.OwnerGuid,
                TotalNumberOfShelfDocuments = shelf.Documents.Count,
                HasImage = shelf.HasImage,
                IntegrityVersion = shelf.IntegrityVersion,
            })
            .AsSplitQuery()
            .ToArrayAsync();
        }

        return Ok(shelfCardModels);
    }

    [HttpGet]
    public async Task<IActionResult> UserShelfList([FromQuery][StringLength(32)] string ownerGuid)
    {
        var userShelfModels = await libraryDb.Shelves
        .Include(shelf => shelf.Libraries)
        .Where(shelf => shelf.OwnerGuid == ownerGuid)
        .Select(shelf => new
        {
            shelf.Guid,
            Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
            {
                Guid = shelfLib.Guid,
                Title = shelfLib.Title,
            }).ToArray(),
            shelf.Title,
        })
        .ToArrayAsync();

        return Ok(userShelfModels);
    }

    [HttpGet]
    public async Task<IActionResult> ShelfModel([FromQuery][StringLength(32)] string shelfGuid,
    [FromQuery][StringLength(32)] string ownerGuid)
    {
        Library_ShelfCardModel? shelfCardModel;
        if (shelfGuid == "DefaultShelf")
        {
            if (ownerGuid is null)
            {
                ModelState.AddModelError("ownerGuid", "ownerGuid cannot be null for the Default Shelf!");
                return BadRequest(ModelState);
            }

            shelfCardModel = await libraryDb.Shelves
            .Include(shelf => shelf.Libraries)
            .Include(shelf => shelf.Documents)
            .ThenInclude(doc => doc.Elements)
            .Where(shelf => shelf.Guid == shelfGuid && shelf.OwnerGuid == ownerGuid)
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
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Title = shelf.Title,
                OwnerGuid = shelf.OwnerGuid,
                TotalNumberOfShelfDocuments = shelf.Documents.Count,
                HasImage = shelf.HasImage,
                IntegrityVersion = shelf.IntegrityVersion,
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync();

            if (shelfCardModel is null)
            {
                var result = await libraryProcess.CreateDefaultShelf(libraryDb, ownerGuid);
                if (!result.Success)
                {
                    return NotFound("Couldn't find and create default shelf and library");
                }

                shelfCardModel = await libraryDb.Shelves
                .Include(shelf => shelf.Libraries)
                .Include(shelf => shelf.Documents)
                .ThenInclude(doc => doc.Elements)
                .Where(shelf => shelf.Guid == shelfGuid && shelf.OwnerGuid == ownerGuid)
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
                        OwnerGuid = doc.OwnerGuid,
                        Title = doc.Title,
                        HasImage = doc.HasImage,
                        IntegrityVersion = doc.IntegrityVersion,
                    }).ToArray(),
                    Guid = shelf.Guid,
                    Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
                    {
                        Guid = shelfLib.Guid,
                        Title = shelfLib.Title,
                    }).ToArray(),
                    Title = shelf.Title,
                    OwnerGuid = shelf.OwnerGuid,
                    TotalNumberOfShelfDocuments = shelf.Documents.Count,
                    HasImage = shelf.HasImage,
                    IntegrityVersion = shelf.IntegrityVersion,
                })
                .AsSplitQuery()
                .FirstOrDefaultAsync();

                if (shelfCardModel is null)
                {
                    return NotFound("Couldn't find and create default shelf and library");
                }
            }

            if (shelfCardModel.DocumentCardModels.Length < 10)
            {
                int numberOfNeededDocs = 10 - shelfCardModel.DocumentCardModels.Length;

                Library_DocumentCardModel[] nonParentDocumentCardModels = await libraryDb.Documents
                .Include(doc => doc.Shelves)
                .Include(doc => doc.Elements)
                .Where(doc => doc.OwnerGuid == ownerGuid && doc.Shelves.Count == 0)
                .Take(numberOfNeededDocs)
                .Select(doc => new Library_DocumentCardModel()
                {
                    Description = doc.Description,
                    Guid = doc.Guid,
                    Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                })
                .AsSplitQuery()
                .ToArrayAsync();

                shelfCardModel.DocumentCardModels = [
                    .. shelfCardModel.DocumentCardModels,
                    .. nonParentDocumentCardModels//.Take(numberOfNeededDocs)
                ];

                int totalNonParentDocumentCardModels = await libraryDb.Documents
                .Include(doc => doc.Shelves)
                .Where(doc => doc.OwnerGuid == ownerGuid && doc.Shelves.Count == 0)
                .CountAsync();

                shelfCardModel.TotalNumberOfShelfDocuments += totalNonParentDocumentCardModels;
            }

        }
        else
        {
            shelfCardModel = await libraryDb.Shelves
            .Include(shelf => shelf.Libraries)
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
                    OwnerGuid = doc.OwnerGuid,
                    Title = doc.Title,
                    HasImage = doc.HasImage,
                    IntegrityVersion = doc.IntegrityVersion,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
                {
                    Guid = shelfLib.Guid,
                    Title = shelfLib.Title,
                }).ToArray(),
                Title = shelf.Title,
                OwnerGuid = shelf.OwnerGuid,
                TotalNumberOfShelfDocuments = shelf.Documents.Count,
                HasImage = shelf.HasImage,
                IntegrityVersion = shelf.IntegrityVersion,
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync();
        }

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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShelf([FromQuery][StringLength(32)] string shelfGuid)
    {
        if (shelfGuid == "DefaultShelf")
        {
            ModelState.AddModelError("DefaultShelf", "Default shelf cannot be deleted!");
            return BadRequest(ModelState);
        }

        Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
        .Include(shelf => shelf.Documents)
        .ThenInclude(doc => doc.Shelves)
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
        if (userGuid != shelfDbModel.OwnerGuid)
        {
            ModelState.AddModelError("Authorization", "Only the owner can delete the shelf!");
            return BadRequest(ModelState);
        }

        //default shelf
        var defaultShelf = await libraryDb.Shelves
            .FirstOrDefaultAsync(shelf => shelf.OwnerGuid == userGuid && shelf.Guid == "DefaultShelf");
        if (defaultShelf is null)
        {
            // creating Default Shelf
            var createDefaultShelfResult = await libraryProcess.CreateDefaultShelf(libraryDb, userGuid);

            if (createDefaultShelfResult.Success &&
            createDefaultShelfResult.ResultObject is not null)
            {
                defaultShelf = (Library_ShelfDbModel)createDefaultShelfResult.ResultObject;
            }
            else
            {
                //log
                Console.WriteLine($"\n***** Cloudnt find and create default shelf for the user with guid '{userGuid}'!");
                ModelState.AddModelError(createDefaultShelfResult.ErrorTitle ?? "defaultShelf",
                createDefaultShelfResult.ErrorDescription ?? $"Cloudnt find and create default shelf for the user with guid '{userGuid}'!");
                return BadRequest(ModelState);
            }
        }

        libraryDb.Shelves.Remove(shelfDbModel);

        foreach (var doc in shelfDbModel.Documents)
        {
            if (doc.Shelves.Count == 0)
            {
                doc.Shelves = [defaultShelf];
            }
        }

        await libraryDb.SaveChangesAsync();

        DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
        if (shelfDirectoryInfo.Exists)
        {
            shelfDirectoryInfo.Delete(true);
        }

        return Ok(new { success = true });
    }





    [HttpGet]
    public async Task<IActionResult> DocumentCardList([FromQuery][StringLength(32)] string shelfGuid,
    [FromQuery][StringLength(32)] string? ownerGuid)
    {
        Library_DocumentCardModel[] documentCardModels = [];

        if (shelfGuid == "DefaultShelf")
        {
            if (ownerGuid is null)
            {
                ModelState.AddModelError("ownerGuid", "ownerGuid cannot be null for the Default Shelf!");
                return BadRequest(ModelState);
            }

            documentCardModels = await libraryDb.Shelves
            .Include(shelf => shelf.Documents)
            .ThenInclude(doc => doc.Elements)
            .Where(shelf => shelf.Guid == shelfGuid && shelf.OwnerGuid == ownerGuid)
            .SelectMany(shelf => shelf.Documents)
            .Select(doc => new Library_DocumentCardModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.OwnerGuid,
                Title = doc.Title,
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
            })
            .ToArrayAsync();

            Library_DocumentCardModel[] nonParentDocuments = await libraryDb.Documents
            .Include(doc => doc.Shelves)
            .Include(doc => doc.Elements)
            .Where(doc => doc.OwnerGuid == ownerGuid && doc.Shelves.Count == 0)
            .Select(doc => new Library_DocumentCardModel()
            {
                Description = doc.Description,
                Guid = doc.Guid,
                Headers = doc.Elements.Where(el => el.Type == "h1" || el.Type == "h2").Select(el => el.Value!).ToArray(),
                OwnerGuid = doc.OwnerGuid,
                Title = doc.Title,
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
            })
            .AsSplitQuery()
            .ToArrayAsync();

            documentCardModels = [.. documentCardModels, .. nonParentDocuments];
        }
        else
        {
            documentCardModels = await libraryDb.Shelves
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
                HasImage = doc.HasImage,
                IntegrityVersion = doc.IntegrityVersion,
            })
            .ToArrayAsync();
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
        .Include(doc => doc.Tags)
        .Include(doc => doc.Elements)
        .Include(doc => doc.RelatedVersions)
            .ThenInclude(rv => rv!.Documents)
        .Include(doc => doc.Shelves)
            .ThenInclude(shelf => shelf.Documents)
        .Include(doc => doc.Shelves)
            .ThenInclude(shelf => shelf.Libraries)
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
                UserGuid = doc.OwnerGuid,
                UserName = "_",
            },
            Elements = doc.Elements.Select(el => new Library_ElementModel()
            {
                Guid = el.Guid,
                Order = el.Order,
                OwnerGuid = el.OwnerGuid,
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
            Shelves = doc.Shelves.Select(shelf => new Library_ShelfBrief()
            {
                Documents = shelf.Documents.Select(shelfDoc => new Library_DocumentBrief()
                {
                    Guid = shelfDoc.Guid,
                    Title = shelfDoc.Title,
                }).ToArray(),
                Guid = shelf.Guid,
                Libraries = shelf.Libraries.Select(shelfLib => new Library_LibraryBrief()
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument([FromQuery][StringLength(32)] string documentGuid)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
        .Include(doc => doc.Elements)
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

        //remove from Db
        libraryDb.Documents.Remove(documentDbModel);
        await libraryDb.SaveChangesAsync();

        //delete directory path from Storage_Document
        string directoryPath = Path.Combine(Storage_Documents.FullName, documentDbModel.Guid);
        try
        {
            System.IO.Directory.Delete(directoryPath, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"\n***** {e.Message}");
        }

        //delete directory path of elements from storage
        foreach (string elementGuid in documentDbModel.Elements.Select(el => el.Guid))
        {
            string elemenDirPath = Path.Combine(Storage_Elements.FullName, elementGuid);
            try
            {
                System.IO.Directory.Delete(elemenDirPath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n***** {e.Message}");
            }
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditElements([FromBody] Library_EditElementFormModel[] formModels)
    {
        if (ModelState.IsValid)
        {
            IEnumerable<string> elementGuids = formModels.Select(m => m.Guid);

            /*List<Library_ElementDbModel>*/
            var elementDbModels = await libraryDb.Elements
            .Include(el => el.Document)
            .Where(el => elementGuids.Contains(el.Guid))
            .ToListAsync();

            IEnumerable<string> parentDocumentGuids = elementDbModels.Select(el => el.Document.Guid).Distinct();
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
                if (elementDbModel.OwnerGuid != ownerGuid)
                {
                    ModelState.AddModelError("Owner", "You are Not the owner of all of the edited elements!");
                    return BadRequest(ModelState);
                }
            }

            //List<Library_ElementDbModel> deletedElements = [];
            bool needToReorder = false;
            foreach (var elementDbModel in elementDbModels)
            {
                var formModel = formModels.FirstOrDefault(fm => fm.Guid == elementDbModel.Guid);

                if (formModel is not null)
                {
                    if (formModel.Delete ?? false)
                    {
                        //deletedElements.Add(elementDbModel);
                        libraryDb.Elements.Remove(elementDbModel);
                        needToReorder = true;

                        if (!string.IsNullOrWhiteSpace(elementDbModel.FileName))
                        {
                            string filePath = Path.Combine(Storage_Elements.FullName,
                            elementDbModel.Guid, elementDbModel.FileName);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }
                    else
                    {
                        elementDbModel.Order = formModel.Order ?? elementDbModel.Order;
                        elementDbModel.Title = formModel.Title ?? elementDbModel.Title;
                        elementDbModel.Value = formModel.Value ?? elementDbModel.Value;
                    }
                }
            }

            await libraryDb.SaveChangesAsync();

            //elementDbModels.RemoveAll(el => deletedElements.Contains(el));
            if (needToReorder)
            {
                List<Library_ElementDbModel> elementsToReorder = (await libraryDb.Documents
                .Include(doc => doc.Elements)
                .Where(doc => doc.Guid == parentDocumentGuids.Single())
                .Select(doc => doc.Elements)
                .FirstOrDefaultAsync())!
                .OrderBy(el => el.Order)
                .ToList();

                for (int i = 0; i < elementsToReorder.Count; i++)
                {
                    elementsToReorder[i].Order = i;
                }

                await libraryDb.SaveChangesAsync();

                elementDbModels = elementsToReorder;
            }

            //seed
            /*foreach (var elementDbModel in elementDbModels)
            {
                _ = libraryProcess.Update_ElementSeed(elementDbModel);
            }*/

            //create response
            Library_ElementModel[] elementModelArray = elementDbModels
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
    public async Task<IActionResult> TotalNumberOfDocuments([FromQuery][StringLength(32)] string ownerGuid)
    {
        int totalNumberOfUserDocuments = await libraryDb.Documents
        .Where(doc => doc.OwnerGuid == ownerGuid)
        .CountAsync();

        return Ok(new { totalNumberOfUserDocuments });
    }

    [HttpGet]
    public async Task<IActionResult> TotalNumberOfShelves([FromQuery][StringLength(32)] string ownerGuid)
    {
        int totalNumberOfUserShelves = await libraryDb.Shelves
        .Where(shelf => shelf.OwnerGuid == ownerGuid)
        .CountAsync();

        return Ok(new { totalNumberOfUserShelves });
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewDocument(Library_NewDocumentFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
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





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(512 * 1024)]//512 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentIntroduction(
        [FromForm] Library_EditIntroductionFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
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
            if (userGuid != documentDbModel.OwnerGuid)
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLibraryIntroduction(
        [FromForm] Library_EditIntroductionFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            if (formModel.Guid == "DefaultLibrary")
            {
                ModelState.AddModelError("DefaultLibrary", "Default library cannot be edited.");
                return BadRequest(ModelState);
            }

            Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
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
            if (userGuid != libraryDbModel.OwnerGuid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the library!");
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

                libraryDbModel.IntegrityVersion += 1;
            }

            await libraryDb.SaveChangesAsync();

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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShelfIntroduction(
        [FromForm] Library_EditIntroductionFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            if (formModel.Guid == "DefaultShelf")
            {
                ModelState.AddModelError("DefaultShelf", "Default shelf cannot be edited.");
                return BadRequest(ModelState);
            }

            Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
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
            if (userGuid != shelfDbModel.OwnerGuid)
            {
                ModelState.AddModelError("Authorization", "Only the owner can edit the shelf!");
                return BadRequest(ModelState);
            }

            shelfDbModel.Title = formModel.Title;
            shelfDbModel.Description = string.IsNullOrWhiteSpace(formModel.Description) ? null : formModel.Description;
            //await libraryDb.SaveChangesAsync();

            if (formModel.Image is not null)
            {
                DirectoryInfo shelfDirectoryInfo = Directory.CreateDirectory(Path.Combine(Storage_Shelves.FullName, shelfDbModel.Guid));
                string shelfImagePath = Path.Combine(shelfDirectoryInfo.FullName, "image");
                using (FileStream fs = System.IO.File.Create(shelfImagePath))
                {
                    await formModel.Image.CopyToAsync(fs);
                }

                shelfDbModel.IntegrityVersion += 1;
            }

            await libraryDb.SaveChangesAsync();

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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocumentIntroductionImage([FromQuery][StringLength(32)]
    string documentGuid)
    {
        Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
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
        if (userGuid != documentDbModel.OwnerGuid)
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

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLibraryIntroductionImage([FromQuery][StringLength(32)]
    string libraryGuid)
    {
        Library_LibraryDbModel? libraryDbModel = await libraryDb.Libraries
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
        if (userGuid != libraryDbModel.OwnerGuid)
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

        return Ok(new { success = true });
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShelfIntroductionImage([FromQuery][StringLength(32)]
    string shelfGuid)
    {
        Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
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
        if (userGuid != shelfDbModel.OwnerGuid)
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

        return Ok(new { success = true });
    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentParentShelves([FromBody]
    Library_DocumentParentShelvesFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            Library_DocumentDbModel? documentDbModel = await libraryDb.Documents
            .Include(doc => doc.Shelves)
            .FirstOrDefaultAsync(doc =>
                doc.Guid == formModel.DocumentGuid &&
                doc.OwnerGuid == userGuid
            );
            if (documentDbModel is null)
            {
                ModelState.AddModelError("document", "Couldn't find the specified document for the user!");
                return BadRequest(ModelState);
            }

            List<Library_ShelfDbModel> parentShelfDbModels = await libraryDb.Shelves
            .Where(shelf => shelf.OwnerGuid == userGuid && formModel.ShelfGuids.Contains(shelf.Guid))
            .ToListAsync();

            documentDbModel.Shelves = parentShelfDbModels;

            await libraryDb.SaveChangesAsync();

            return Ok(new { success = true });
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShelfParentLibraries([FromBody]
    Library_ShelfParentLibrariesFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            if (formModel.ShelfGuid == "DefaultShelf")
            {
                ModelState.AddModelError("DefaultShelf", "Default shelf cannot be edited.");
                return BadRequest(ModelState);
            }

            string userGuid = (await userManager.Users
            .Where(u => u.NormalizedUserName == userManager.NormalizeName(User.Identity!.Name))
            .Select(u => u.UserGuid)
            .FirstOrDefaultAsync())!;

            Library_ShelfDbModel? shelfDbModel = await libraryDb.Shelves
            .Include(shelf => shelf.Libraries)
            .FirstOrDefaultAsync(doc =>
                doc.Guid == formModel.ShelfGuid &&
                doc.OwnerGuid == userGuid
            );
            if (shelfDbModel is null)
            {
                ModelState.AddModelError("shelf", "Couldn't find the specified shelf for the user!");
                return BadRequest(ModelState);
            }

            List<Library_LibraryDbModel> libraryDbModels = await libraryDb.Libraries
            .Where(lib => lib.OwnerGuid == userGuid && formModel.LibraryGuids.Contains(lib.Guid))
            .ToListAsync();

            shelfDbModel.Libraries = libraryDbModels;

            await libraryDb.SaveChangesAsync();

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

}