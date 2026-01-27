using System.IO.Compression;
using System.Text.Json;
using AspNetCoreApp.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Models;

public enum StatusEnum
{
    Not_Started,
    Started,
    Completed,
}
public class Backup_Status
{
    public string Generating_Zip_File { get; set; } = StatusEnum.Not_Started.ToString();

    public string Getting_Identity_User_Backup { get; set; } = StatusEnum.Not_Started.ToString();

    public string Getting_Library_Owner_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_Library_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_Shelf_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_Document_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_Element_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_Followship_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_UserFavoriteLibrary_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_UserFavoriteShelf_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_UserFavoriteDocument_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_LibraryShelf_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_ShelfDocument_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Library_DocumentTag_Backup { get; set; } = StatusEnum.Not_Started.ToString();

    public string Getting_Review_User_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Review_Review_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Review_Comment_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Review_UserLike_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Review_UserThumbsUp_Backup { get; set; } = StatusEnum.Not_Started.ToString();
    public string Getting_Review_UserThumbsDown_Backup { get; set; } = StatusEnum.Not_Started.ToString();

    public string Getting_Notification_Notification_Backup { get; set; } = StatusEnum.Not_Started.ToString();

    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public double File_Size { get; set; }
    public string? File_Name { get; set; }
    public bool Ready_To_Download { get; set; } = false;
}
public class Seed_Status
{
    public string Seeding_Identity_User_Db { get; set; } = StatusEnum.Not_Started.ToString();

    public string Seeding_Library_Owner_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_Library_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_Shelf_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_Document_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_Element_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_Followship_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_UserFavoriteLibrary_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_UserFavoriteShelf_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_UserFavoriteDocument_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_LibraryShelf_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_ShelfDocument_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Library_DocumentTag_Db { get; set; } = StatusEnum.Not_Started.ToString();

    public string Seeding_Review_User_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Review_Review_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Review_Comment_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Review_UserLike_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Review_UserThumbsUp_Db { get; set; } = StatusEnum.Not_Started.ToString();
    public string Seeding_Review_UserThumbsDown_Db { get; set; } = StatusEnum.Not_Started.ToString();

    public string Seeding_Notification_Notification_Db { get; set; } = StatusEnum.Not_Started.ToString();

}

public class Backup_Process
{
    public readonly DirectoryInfo Backup_Directory;
    public readonly DirectoryInfo Backup_Db_Directory;
    public readonly string Backup_Status_FilePath;
    public readonly string Seed_Status_FilePath;
    public readonly string BackupFileNameWithoutDate = "backup.zip";

    readonly DirectoryInfo Storage_Directory;
    readonly DirectoryInfo Backup_Identity_User_Directory;
    readonly DirectoryInfo Backup_Library_Owner_Directory;
    readonly DirectoryInfo Backup_Library_Library_Directory;
    readonly DirectoryInfo Backup_Library_Shelf_Directory;
    readonly DirectoryInfo Backup_Library_Document_Directory;
    readonly DirectoryInfo Backup_Library_Element_Directory;
    readonly DirectoryInfo Backup_Review_User_Directory;
    readonly DirectoryInfo Backup_Review_Review_Directory;
    readonly DirectoryInfo Backup_Review_Comment_Directory;
    readonly DirectoryInfo Backup_Notification_Notification_Directory;
    readonly DirectoryInfo Backup_Library_Followship_Directory;
    readonly DirectoryInfo Backup_Library_UserFavoriteLibrary_Directory;
    readonly DirectoryInfo Backup_Library_UserFavoriteShelf_Directory;
    readonly DirectoryInfo Backup_Library_UserFavoriteDocument_Directory;
    readonly DirectoryInfo Backup_Library_LibraryShelf_Directory;
    readonly DirectoryInfo Backup_Library_ShelfDocument_Directory;
    readonly DirectoryInfo Backup_Library_DocumentTag_Directory;
    readonly DirectoryInfo Backup_Review_UserLike_Directory;
    readonly DirectoryInfo Backup_Review_UserThumbsUp_Directory;
    readonly DirectoryInfo Backup_Review_UserThumbsDown_Directory;

    readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.General);

    readonly IServiceProvider serviceProvider;






    public Backup_Process(IWebHostEnvironment _env, IServiceProvider serviceProvider)
    {
        Storage_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage"));
        Backup_Directory = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Backup"));

        Backup_Db_Directory = Directory.CreateDirectory(Path.Combine(Storage_Directory.FullName, "DataBase"));


        Backup_Identity_User_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Identity", "User"));

        Backup_Library_Owner_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "Owner"));
        Backup_Library_Library_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "Library"));
        Backup_Library_Shelf_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "Shelf"));
        Backup_Library_Document_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "Document"));
        Backup_Library_Element_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "Element"));
        Backup_Library_Followship_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "Followship"));
        Backup_Library_UserFavoriteLibrary_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "UserFavoriteLibrary"));
        Backup_Library_UserFavoriteShelf_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "UserFavoriteShelf"));
        Backup_Library_UserFavoriteDocument_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "UserFavoriteDocument"));
        Backup_Library_LibraryShelf_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "LibraryShelf"));
        Backup_Library_ShelfDocument_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "ShelfDocument"));
        Backup_Library_DocumentTag_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Library", "DocumentTag"));

        Backup_Review_User_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Review", "User"));
        Backup_Review_Review_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Review", "Review"));
        Backup_Review_Comment_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Review", "Comment"));
        Backup_Review_UserLike_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Review", "UserLike"));
        Backup_Review_UserThumbsUp_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Review", "UserThumbsUp"));
        Backup_Review_UserThumbsDown_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Review", "UserThumbsDown"));

        Backup_Notification_Notification_Directory = Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName, "Notification"));

        Backup_Status_FilePath = Path.Combine(Backup_Directory.FullName, "Backup_Status.json");
        Seed_Status_FilePath = Path.Combine(Backup_Directory.FullName, "Seed_Status.json");

        jsonSerializerOptions.Converters.Add(new GuidJsonConverter());

        this.serviceProvider = serviceProvider;
    }
    public void Create_Directories()
    {
        Directory.CreateDirectory(Path.Combine(Storage_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Db_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Identity_User_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_Owner_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_Library_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_Shelf_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_Document_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_Element_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_Followship_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_UserFavoriteLibrary_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_UserFavoriteShelf_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_UserFavoriteDocument_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_LibraryShelf_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_ShelfDocument_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Library_DocumentTag_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Review_User_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Review_Review_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Review_Comment_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Review_UserLike_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Review_UserThumbsUp_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Review_UserThumbsDown_Directory.FullName));
        Directory.CreateDirectory(Path.Combine(Backup_Notification_Notification_Directory.FullName));
    }





    public async Task Generate_Backup_ZipFile()
    {
        //delete old directories
        if (Backup_Db_Directory.Exists)
        {
            Backup_Db_Directory.Delete(true);
        }
        if (Backup_Directory.Exists)
        {
            Backup_Directory.Delete(true);
        }

        //create new directories
        Create_Directories();

        //create new status and save it
        Backup_Status status = new();
        string statusInJson = JsonSerializer.Serialize(status);
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        //get all dbs backup
        await Get_All_Dbs_Backup();

        //define backup status
        statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        DateTime createdAt = status.Created_At;
        string backupFileName = createdAt.ToString("yyyy_MM_dd_HH_mm_ss") + "_" + BackupFileNameWithoutDate;
        status.File_Name = backupFileName;

        //write status
        status.Generating_Zip_File = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        //zip the Storage directory
        string backupFilePath = Path.Combine(Backup_Directory.FullName, backupFileName);
        ZipFile.CreateFromDirectory(Storage_Directory.FullName, backupFilePath);

        status.Generating_Zip_File = StatusEnum.Completed.ToString();
        FileInfo backupFileInfo = new FileInfo(backupFilePath);//fileInfo needed for fie length
        if (backupFileInfo.Exists)
        {
            status.File_Size = (double)backupFileInfo.Length / 1024;//size in KB
            status.Ready_To_Download = true;
        }
        statusInJson = JsonSerializer.Serialize(status);
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }





    public async Task Get_All_Dbs_Backup()
    {
        await Get_Identity_User_Backup();

        await Get_Library_Owner_Backup();
        await Get_Library_Library_Backup();
        await Get_Library_Shelf_Backup();
        await Get_Library_Document_Backup();
        await Get_Library_Element_Backup();
        await Get_Library_Followship_Backup();
        await Get_Library_UserFavoriteLibrary_Backup();
        await Get_Library_UserFavoriteShelf_Backup();
        await Get_Library_UserFavoriteDocument_Backup();
        await Get_Library_LibraryShelf_Backup();
        await Get_Library_ShelfDocument_Backup();
        await Get_Library_DocumentTag_Backup();

        await Get_Review_User_Backup();
        await Get_Review_Review_Backup();
        await Get_Review_Comment_Backup();
        await Get_Review_UserLike_Backup();
        await Get_Review_UserThumbsUp_Backup();
        await Get_Review_UserThumbsDown_Backup();

        await Get_Notification_Notification_Backup();
    }

    public async Task Get_Identity_User_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Identity_User_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            UserManager<Identity_UserDbModel> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Identity_UserDbModel>>();

            await foreach (Identity_UserDbModel user in userManager.Users.AsAsyncEnumerable())
            {
                string[] roles = [.. await userManager.GetRolesAsync(user)];
                Identity_User_SeedModel seedModel = Identity_User_SeedModel.Factory(user, roles);
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Identity_User_Directory.FullName, user.UserGuid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Identity_User_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Identity_User_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Identity_User_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            UserManager<Identity_UserDbModel> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Identity_UserDbModel>>();

            foreach (FileInfo fileInfo in Backup_Identity_User_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Identity_User_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Identity_User_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Identity_UserDbModel user = seedModel.GetDbModel();

                IdentityResult result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, seedModel.Roles);
                }
            }
        }

        //set new status
        status.Seeding_Identity_User_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_Owner_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_Owner_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            IAsyncEnumerable<Library_Owner_SeedModel> dbQuery = libraryDb.Owners
            .Select(o => new Library_Owner_SeedModel()
            {
                DefaultLibraryGuid = o.DefaultLibraryGuid,
                DefaultShelfGuid = o.DefaultShelfGuid,
                Guid = o.Guid,
                NormalizedUserName = o.NormalizedUserName,
            })
            .AsAsyncEnumerable();

            await foreach (Library_Owner_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_Owner_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }


        //set new status
        status.Getting_Library_Owner_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_Owner_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_Owner_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_Owner_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Library_Owner_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_Owner_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Library_OwnerDbModel owner = seedModel.GetDbModel();
                libraryDb.Owners.Add(owner);
            }
            await libraryDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Library_Owner_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_Library_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_Library_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            IAsyncEnumerable<Library_Library_SeedModel> dbQuery = libraryDb.Libraries
            .Select(lib => new Library_Library_SeedModel()
            {
                CreatedAt = lib.CreatedAt,
                Description = lib.Description,
                Guid = lib.Guid,
                HasImage = lib.HasImage,
                Owner_Guid = lib.Owner.Guid,
                Title = lib.Title,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Library_Library_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_Library_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Library_Library_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_Library_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_Library_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_Library_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Library_Library_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_Library_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Library_LibraryDbModel library = seedModel.GetDbModel();

                Library_OwnerDbModel? owner = await libraryDb.Owners
                .Where(o => o.Guid == seedModel.Owner_Guid)
                .Select(o => new Library_OwnerDbModel()
                {
                    Id = o.Id,
                })
                .FirstOrDefaultAsync();
                if (owner is null) continue;

                libraryDb.Owners.Attach(owner);
                library.Owner = owner;

                libraryDb.Libraries.Add(library);
            }
            await libraryDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Library_Library_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_Shelf_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_Shelf_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            IAsyncEnumerable<Library_Shelf_SeedModel> dbQuery = libraryDb.Shelves
            .Select(shlef => new Library_Shelf_SeedModel()
            {
                CreatedAt = shlef.CreatedAt,
                Description = shlef.Description,
                Guid = shlef.Guid,
                HasImage = shlef.HasImage,
                Owner_Guid = shlef.Owner.Guid,
                Title = shlef.Title,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Library_Shelf_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_Shelf_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Library_Shelf_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_Shelf_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_Shelf_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_Shelf_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Library_Shelf_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_Shelf_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Library_ShelfDbModel shelf = seedModel.GetDbModel();

                Library_OwnerDbModel? owner = await libraryDb.Owners
                .Where(o => o.Guid == seedModel.Owner_Guid)
                .Select(o => new Library_OwnerDbModel()
                {
                    Id = o.Id,
                })
                .FirstOrDefaultAsync();
                if (owner is null) continue;

                libraryDb.Owners.Attach(owner);
                shelf.Owner = owner;

                libraryDb.Shelves.Add(shelf);
            }
            await libraryDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Library_Shelf_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_Document_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_Document_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            IAsyncEnumerable<Library_Document_SeedModel> dbQuery = libraryDb.Documents
            .Select(doc => new Library_Document_SeedModel()
            {
                CreatedAt = doc.CreatedAt,
                Description = doc.Description,
                Guid = doc.Guid,
                HasImage = doc.HasImage,
                Owner_Guid = doc.Owner.Guid,
                Title = doc.Title,
                RelatedVersions_Guid = doc.RelatedVersions == null ? null : doc.RelatedVersions.Guid,
                Version = doc.Version,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Library_Document_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_Document_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Library_Document_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_Document_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_Document_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_Document_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Library_Document_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_Document_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Library_DocumentDbModel document = seedModel.GetDbModel();

                Library_OwnerDbModel? owner = await libraryDb.Owners
                .Where(o => o.Guid == seedModel.Owner_Guid)
                .Select(o => new Library_OwnerDbModel()
                {
                    Id = o.Id,
                })
                .FirstOrDefaultAsync();
                if (owner is null) continue;

                if (seedModel.RelatedVersions_Guid != null && seedModel.RelatedVersions_Guid.HasValue)
                {
                    Library_RelatedVersionsDbModel? relatedVersion = await libraryDb.RelatedVersions
                    .Where(rv => rv.Guid == seedModel.RelatedVersions_Guid)
                    .Select(rv => new Library_RelatedVersionsDbModel()
                    {
                        Id = rv.Id,
                    })
                    .FirstOrDefaultAsync();
                    if (relatedVersion is null)
                    {
                        //create
                        relatedVersion = new()
                        {
                            Guid = seedModel.RelatedVersions_Guid.Value,
                        };
                        libraryDb.RelatedVersions.Add(relatedVersion);
                    }
                    else
                    {
                        libraryDb.RelatedVersions.Attach(relatedVersion);
                    }
                    document.RelatedVersions = relatedVersion;
                }

                libraryDb.Owners.Attach(owner);
                document.Owner = owner;

                libraryDb.Documents.Add(document);
            }
            await libraryDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Library_Document_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_Element_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_Element_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            IAsyncEnumerable<Library_Element_SeedModel> dbQuery = libraryDb.Elements
            .Select(el => new Library_Element_SeedModel()
            {
                FileName = el.FileName,
                Guid = el.Guid,
                Order = el.Order,
                Owner_Guid = el.Owner.Guid,
                ParentDocument_Guid = el.ParentDocument.Guid,
                Title = el.Title,
                Type = el.Type,
                UpdatedAt = el.UpdatedAt,
                Value = el.Value,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Library_Element_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_Element_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Library_Element_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_Element_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_Element_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_Element_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Library_Element_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Library_Element_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Library_ElementDbModel element = seedModel.GetDbModel();

                //owner
                Library_OwnerDbModel? owner = await libraryDb.Owners
                .Where(o => o.Guid == seedModel.Owner_Guid)
                .Select(o => new Library_OwnerDbModel()
                {
                    Id = o.Id,
                })
                .FirstOrDefaultAsync();
                if (owner is null) continue;

                libraryDb.Owners.Attach(owner);
                element.Owner = owner;

                //parent document
                Library_DocumentDbModel? parentDoc = await libraryDb.Documents
                .Where(o => o.Guid == seedModel.Owner_Guid)
                .Select(o => new Library_DocumentDbModel()
                {
                    Id = o.Id,
                })
                .FirstOrDefaultAsync();
                if (parentDoc is null) continue;

                libraryDb.Documents.Attach(parentDoc);
                element.ParentDocument = parentDoc;

                libraryDb.Elements.Add(element);
            }
            await libraryDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Library_Element_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_Followship_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_Followship_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_FollowerFollowing_SeedModel> list = await libraryDb.FollowerFollowings
                .OrderBy(ff => ff.FollowerId)
                .ThenBy(ff => ff.FollowingId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(ff => new Library_FollowerFollowing_SeedModel()
                {
                    Follower_Guid = ff.Follower.Guid,
                    Following_Guid = ff.Following.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_Followship_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_Followship_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_Followship_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_Followship_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_Followship_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_FollowerFollowing_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_FollowerFollowing_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                //library followship
                foreach (Library_FollowerFollowing_SeedModel seedModel in list)
                {
                    //fetch
                    Library_OwnerDbModel? follower = await libraryDb.Owners
                    .Where(o => o.Guid == seedModel.Follower_Guid)
                    .Select(o => new Library_OwnerDbModel() { Id = o.Id })
                    .FirstOrDefaultAsync();
                    if (follower is null) continue;

                    Library_OwnerDbModel? following = await libraryDb.Owners
                    .Where(o => o.Guid == seedModel.Follower_Guid)
                    .Select(o => new Library_OwnerDbModel() { Id = o.Id })
                    .FirstOrDefaultAsync();
                    if (following is null) continue;

                    //attach
                    libraryDb.Owners.Attach(follower);
                    libraryDb.Owners.Attach(following);

                    //create join table
                    Library_FollowerFollowing_DbModel ff = new()
                    {
                        Follower = follower,
                        Following = following,
                    };

                    //add
                    libraryDb.FollowerFollowings.Add(ff);
                }
                await libraryDb.SaveChangesAsync();

                //review followship
                foreach (Library_FollowerFollowing_SeedModel seedModel in list)
                {
                    //fetch
                    Review_UserDbModel? follower = await reviewDb.Users
                    .Where(u => u.Guid == seedModel.Follower_Guid)
                    .Select(u => new Review_UserDbModel() { Id = u.Id })
                    .FirstOrDefaultAsync();
                    if (follower is null) continue;

                    Review_UserDbModel? following = await libraryDb.Owners
                    .Where(o => o.Guid == seedModel.Follower_Guid)
                    .Select(o => new Review_UserDbModel() { Id = o.Id })
                    .FirstOrDefaultAsync();
                    if (following is null) continue;

                    //attach
                    reviewDb.Users.Attach(follower);
                    reviewDb.Users.Attach(following);

                    //create join table
                    Review_FollowerFollowing_DbModel ff = new()
                    {
                        Follower = follower,
                        Following = following,
                    };

                    //add
                    reviewDb.FollowerFollowings.Add(ff);
                }
                await reviewDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_Followship_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_UserFavoriteLibrary_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_UserFavoriteLibrary_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_UserFavoriteLibrary_SeedModel> list = await libraryDb.UserFavoriteLibraries
                .OrderBy(ul => ul.UserId)
                .ThenBy(ul => ul.LibraryId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(ul => new Library_UserFavoriteLibrary_SeedModel()
                {
                    Library_Guid = ul.Library.Guid,
                    User_Guid = ul.User.Guid
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_UserFavoriteLibrary_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_UserFavoriteLibrary_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_UserFavoriteLibrary_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_UserFavoriteLibrary_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_UserFavoriteLibrary_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_UserFavoriteLibrary_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_UserFavoriteLibrary_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Library_UserFavoriteLibrary_SeedModel seedModel in list)
                {
                    //fetch
                    Library_OwnerDbModel? user = await libraryDb.Owners
                    .Where(o => o.Guid == seedModel.User_Guid)
                    .Select(o => new Library_OwnerDbModel() { Id = o.Id })
                    .FirstOrDefaultAsync();
                    if (user is null) continue;

                    Library_LibraryDbModel? library = await libraryDb.Libraries
                    .Where(lib => lib.Guid == seedModel.Library_Guid)
                    .Select(lib => new Library_LibraryDbModel() { Id = lib.Id })
                    .FirstOrDefaultAsync();
                    if (library is null) continue;

                    //attach
                    libraryDb.Owners.Attach(user);
                    libraryDb.Libraries.Attach(library);

                    //create join table
                    Library_UserFavoriteLibrary_DbModel ul = new()
                    {
                        User = user,
                        Library = library,
                    };

                    //add
                    libraryDb.UserFavoriteLibraries.Add(ul);
                }
                await libraryDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_UserFavoriteLibrary_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_UserFavoriteShelf_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_UserFavoriteShelf_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_UserFavoriteShelf_SeedModel> list = await libraryDb.UserFavoriteShelves
                .OrderBy(ush => ush.UserId)
                .ThenBy(ush => ush.ShelfId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(ush => new Library_UserFavoriteShelf_SeedModel()
                {
                    Shelf_Guid = ush.Shelf.Guid,
                    User_Guid = ush.User.Guid
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_UserFavoriteShelf_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_UserFavoriteShelf_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_UserFavoriteShelf_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_UserFavoriteShelf_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_UserFavoriteShelf_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_UserFavoriteShelf_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_UserFavoriteShelf_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Library_UserFavoriteShelf_SeedModel seedModel in list)
                {
                    //fetch
                    Library_OwnerDbModel? user = await libraryDb.Owners
                    .Where(o => o.Guid == seedModel.User_Guid)
                    .Select(o => new Library_OwnerDbModel() { Id = o.Id })
                    .FirstOrDefaultAsync();
                    if (user is null) continue;

                    Library_ShelfDbModel? shelf = await libraryDb.Shelves
                    .Where(shelf => shelf.Guid == seedModel.Shelf_Guid)
                    .Select(shelf => new Library_ShelfDbModel() { Id = shelf.Id })
                    .FirstOrDefaultAsync();
                    if (shelf is null) continue;

                    //attach
                    libraryDb.Owners.Attach(user);
                    libraryDb.Shelves.Attach(shelf);

                    //create join table
                    Library_UserFavoriteShelf_DbModel ush = new()
                    {
                        User = user,
                        Shelf = shelf,
                    };

                    //add
                    libraryDb.UserFavoriteShelves.Add(ush);
                }
                await libraryDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_UserFavoriteShelf_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_UserFavoriteDocument_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_UserFavoriteDocument_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_UserFavoriteDocument_SeedModel> list = await libraryDb.UserFavoriteDocuments
                .OrderBy(ud => ud.UserId)
                .ThenBy(ud => ud.DocumentId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(ud => new Library_UserFavoriteDocument_SeedModel()
                {
                    Document_Guid = ud.Document.Guid,
                    User_Guid = ud.User.Guid
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_UserFavoriteDocument_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_UserFavoriteDocument_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_UserFavoriteDocument_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_UserFavoriteDocument_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_UserFavoriteDocument_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_UserFavoriteDocument_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_UserFavoriteDocument_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Library_UserFavoriteDocument_SeedModel seedModel in list)
                {
                    //fetch
                    Library_OwnerDbModel? user = await libraryDb.Owners
                    .Where(o => o.Guid == seedModel.User_Guid)
                    .Select(o => new Library_OwnerDbModel() { Id = o.Id })
                    .FirstOrDefaultAsync();
                    if (user is null) continue;

                    Library_DocumentDbModel? document = await libraryDb.Documents
                    .Where(doc => doc.Guid == seedModel.Document_Guid)
                    .Select(doc => new Library_DocumentDbModel() { Id = doc.Id })
                    .FirstOrDefaultAsync();
                    if (document is null) continue;

                    //attach
                    libraryDb.Owners.Attach(user);
                    libraryDb.Documents.Attach(document);

                    //create join table
                    Library_UserFavoriteDocument_DbModel ud = new()
                    {
                        User = user,
                        Document = document,
                    };

                    //add
                    libraryDb.UserFavoriteDocuments.Add(ud);
                }
                await libraryDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_UserFavoriteDocument_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_LibraryShelf_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_LibraryShelf_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_LibraryShelf_SeedModel> list = await libraryDb.LibraryShelves
                .OrderBy(lsh => lsh.LibraryId)
                .ThenBy(lsh => lsh.ShelfId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(lsh => new Library_LibraryShelf_SeedModel()
                {
                    Library_Guid = lsh.Library.Guid,
                    Shelf_Guid = lsh.Shelf.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_LibraryShelf_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_LibraryShelf_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_LibraryShelf_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_LibraryShelf_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_LibraryShelf_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_LibraryShelf_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_LibraryShelf_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Library_LibraryShelf_SeedModel seedModel in list)
                {
                    //fetch
                    Library_LibraryDbModel? library = await libraryDb.Libraries
                    .Where(lib => lib.Guid == seedModel.Library_Guid)
                    .Select(lib => new Library_LibraryDbModel() { Id = lib.Id })
                    .FirstOrDefaultAsync();
                    if (library is null) continue;

                    Library_ShelfDbModel? shelf = await libraryDb.Shelves
                    .Where(doc => doc.Guid == seedModel.Shelf_Guid)
                    .Select(doc => new Library_ShelfDbModel() { Id = doc.Id })
                    .FirstOrDefaultAsync();
                    if (shelf is null) continue;

                    //attach
                    libraryDb.Libraries.Attach(library);
                    libraryDb.Shelves.Attach(shelf);

                    //create join table
                    Library_LibraryShelf_DbModel lsh = new()
                    {
                        Library = library,
                        Shelf = shelf,
                    };

                    //add
                    libraryDb.LibraryShelves.Add(lsh);
                }
                await libraryDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_LibraryShelf_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_ShelfDocument_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_ShelfDocument_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_ShelfDocument_SeedModel> list = await libraryDb.ShelfDocuments
                .OrderBy(lsh => lsh.ShelfId)
                .ThenBy(lsh => lsh.DocumentId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(lsh => new Library_ShelfDocument_SeedModel()
                {
                    Shelf_Guid = lsh.Shelf.Guid,
                    Document_Guid = lsh.Document.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_ShelfDocument_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_ShelfDocument_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_ShelfDocument_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_ShelfDocument_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_ShelfDocument_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_ShelfDocument_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_ShelfDocument_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Library_ShelfDocument_SeedModel seedModel in list)
                {
                    //fetch
                    Library_DocumentDbModel? document = await libraryDb.Documents
                    .Where(doc => doc.Guid == seedModel.Document_Guid)
                    .Select(doc => new Library_DocumentDbModel() { Id = doc.Id })
                    .FirstOrDefaultAsync();
                    if (document is null) continue;

                    Library_ShelfDbModel? shelf = await libraryDb.Shelves
                    .Where(doc => doc.Guid == seedModel.Shelf_Guid)
                    .Select(doc => new Library_ShelfDbModel() { Id = doc.Id })
                    .FirstOrDefaultAsync();
                    if (shelf is null) continue;

                    //attach
                    libraryDb.Documents.Attach(document);
                    libraryDb.Shelves.Attach(shelf);

                    //create join table
                    Library_ShelfDocument_DbModel shd = new()
                    {
                        Document = document,
                        Shelf = shelf,
                    };

                    //add
                    libraryDb.ShelfDocuments.Add(shd);
                }
                await libraryDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_ShelfDocument_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Library_DocumentTag_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Library_DocumentTag_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            while (true)
            {
                List<Library_DocumentTag_SeedModel> list = await libraryDb.DocumentTags
                .OrderBy(dt => dt.DocumentId)
                .ThenBy(dt => dt.TagId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(lsh => new Library_DocumentTag_SeedModel()
                {
                    Tag_Name = lsh.Tag.Name,
                    Document_Guid = lsh.Document.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Library_DocumentTag_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Library_DocumentTag_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Library_DocumentTag_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Library_DocumentTag_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Library_DbContext libraryDb = scope.ServiceProvider.GetRequiredService<Library_DbContext>();

            foreach (FileInfo fileInfo in Backup_Library_DocumentTag_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Library_DocumentTag_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Library_DocumentTag_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Library_DocumentTag_SeedModel seedModel in list)
                {
                    //fetch
                    Library_DocumentDbModel? document = await libraryDb.Documents
                    .Where(doc => doc.Guid == seedModel.Document_Guid)
                    .Select(doc => new Library_DocumentDbModel() { Id = doc.Id })
                    .FirstOrDefaultAsync();
                    if (document is null) continue;

                    Library_TagDbModel? tag = await libraryDb.Tags
                    .Where(tag => tag.Name == seedModel.Tag_Name)
                    .Select(tag => new Library_TagDbModel() { Id = tag.Id })
                    .FirstOrDefaultAsync();
                    if (tag is null) continue;

                    //attach
                    libraryDb.Documents.Attach(document);
                    libraryDb.Tags.Attach(tag);

                    //create join table
                    Library_DocumentTag_DbModel docTag = new()
                    {
                        Document = document,
                        Tag = tag,
                    };

                    //add
                    libraryDb.DocumentTags.Add(docTag);
                }
                await libraryDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Library_DocumentTag_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Review_User_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Review_User_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            IAsyncEnumerable<Review_User_SeedModel> dbQuery = reviewDb.Users
            .Select(u => new Review_User_SeedModel()
            {
                Guid = u.Guid,
                NormalizedUserName = u.NormalizedUserName,
            })
            .AsAsyncEnumerable();

            await foreach (Review_User_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Review_User_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Review_User_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Review_User_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Review_User_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Review_User_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Review_User_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Review_User_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Review_UserDbModel user = seedModel.GetDbModel();

                reviewDb.Users.Add(user);
            }
            await reviewDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Review_User_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Review_Review_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Review_Review_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            IAsyncEnumerable<Review_Review_SeedModel> dbQuery = reviewDb.Reviews
            .Select(r => new Review_Review_SeedModel()
            {
                SubjectGuid = r.SubjectGuid,
                Owner_Guid = r.Owner.Guid,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Review_Review_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Review_Review_Directory.FullName, seedModel.SubjectGuid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Review_Review_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Review_Review_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Review_Review_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Review_Review_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Review_Review_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Review_Review_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Review_ReviewDbModel review = seedModel.GetDbModel();

                //owner
                Review_UserDbModel? owner = await reviewDb.Users
                .Where(u => u.Guid == seedModel.Owner_Guid)
                .Select(u => new Review_UserDbModel()
                {
                    Id = u.Id,
                })
                .FirstOrDefaultAsync();
                if (owner is null) continue;

                reviewDb.Users.Attach(owner);
                review.Owner = owner;

                reviewDb.Reviews.Add(review);
            }
            await reviewDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Review_Review_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Review_Comment_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Review_Comment_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            IAsyncEnumerable<Review_Comment_SeedModel> dbQuery = reviewDb.Comments
            .Select(c => new Review_Comment_SeedModel()
            {
                CreatedAt = c.CreatedAt,
                Guid = c.Guid,
                ParentReview_SubjectGuid = c.ParentReview.SubjectGuid,
                ReplyTo_Guid = c.ReplyTo == null ? null : c.ReplyTo.Guid,
                Text = c.Text,
                Writer_Guid = c.Writer.Guid,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Review_Comment_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Review_Comment_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Review_Comment_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Review_Comment_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Review_Comment_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Review_Comment_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Review_Comment_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Review_Comment_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Review_CommentDbModel comment = seedModel.GetDbModel();

                //owner
                Review_UserDbModel? writer = await reviewDb.Users
                .Where(u => u.Guid == seedModel.Writer_Guid)
                .Select(u => new Review_UserDbModel()
                {
                    Id = u.Id,
                })
                .FirstOrDefaultAsync();
                if (writer is null) continue;

                reviewDb.Users.Attach(writer);
                comment.Writer = writer;

                //owner
                Review_ReviewDbModel? parentReview = await reviewDb.Reviews
                .Where(r => r.SubjectGuid == seedModel.ParentReview_SubjectGuid)
                .Select(r => new Review_ReviewDbModel()
                {
                    Id = r.Id,
                })
                .FirstOrDefaultAsync();
                if (parentReview is null) continue;

                reviewDb.Reviews.Attach(parentReview);
                comment.ParentReview = parentReview;

                //replyTo
                if (seedModel.ReplyTo_Guid is not null)
                {
                    Review_CommentDbModel? replyTo = await reviewDb.Comments
                    .Where(c => c.Guid == seedModel.ReplyTo_Guid)
                    .Select(r => new Review_CommentDbModel()
                    {
                        Id = r.Id,
                    })
                    .FirstOrDefaultAsync();

                    if (replyTo is not null)
                    {
                        reviewDb.Comments.Attach(replyTo);
                    }

                    comment.ReplyTo = replyTo;
                }

                reviewDb.Comments.Add(comment);
            }
            await reviewDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Review_Comment_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Review_UserLike_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Review_UserLike_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            while (true)
            {
                List<Review_UserLike_SeedModel> list = await reviewDb.UserLikes
                .OrderBy(ul => ul.UserId)
                .ThenBy(ul => ul.ReviewId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(ul => new Review_UserLike_SeedModel()
                {
                    Review_Guid = ul.Review.SubjectGuid,
                    User_Guid = ul.User.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Review_UserLike_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Review_UserLike_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Review_UserLike_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Review_UserLike_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Review_UserLike_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Review_UserLike_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Review_UserLike_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Review_UserLike_SeedModel seedModel in list)
                {
                    //fetch
                    Review_UserDbModel? user = await reviewDb.Users
                    .Where(u => u.Guid == seedModel.User_Guid)
                    .Select(u => new Review_UserDbModel() { Id = u.Id })
                    .FirstOrDefaultAsync();
                    if (user is null) continue;

                    Review_ReviewDbModel? review = await reviewDb.Reviews
                    .Where(r => r.SubjectGuid == seedModel.Review_Guid)
                    .Select(r => new Review_ReviewDbModel() { Id = r.Id })
                    .FirstOrDefaultAsync();
                    if (review is null) continue;

                    //attach
                    reviewDb.Users.Attach(user);
                    reviewDb.Reviews.Attach(review);

                    //create join table
                    Review_UserLike_DbModel userLike = new()
                    {
                        Review = review,
                        User = user,
                    };

                    //add
                    reviewDb.UserLikes.Add(userLike);
                }
                await reviewDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Review_UserLike_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Review_UserThumbsUp_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Review_UserThumbsUp_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            while (true)
            {
                List<Review_UserThumbsUp_SeedModel> list = await reviewDb.UserThumbsUp
                .OrderBy(uc => uc.UserId)
                .ThenBy(uc => uc.CommentId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(uc => new Review_UserThumbsUp_SeedModel()
                {
                    Comment_Guid = uc.Comment.Guid,
                    User_Guid = uc.User.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Review_UserThumbsUp_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Review_UserThumbsUp_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Review_UserThumbsUp_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Review_UserThumbsUp_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Review_UserThumbsUp_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Review_UserThumbsUp_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Review_UserThumbsUp_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Review_UserThumbsUp_SeedModel seedModel in list)
                {
                    //fetch
                    Review_UserDbModel? user = await reviewDb.Users
                    .Where(u => u.Guid == seedModel.User_Guid)
                    .Select(u => new Review_UserDbModel() { Id = u.Id })
                    .FirstOrDefaultAsync();
                    if (user is null) continue;

                    Review_CommentDbModel? comment = await reviewDb.Comments
                    .Where(c => c.Guid == seedModel.Comment_Guid)
                    .Select(c => new Review_CommentDbModel() { Id = c.Id })
                    .FirstOrDefaultAsync();
                    if (comment is null) continue;

                    //attach
                    reviewDb.Users.Attach(user);
                    reviewDb.Comments.Attach(comment);

                    //create join table
                    Review_UserThumbsUp_DbModel userThumbsUp = new()
                    {
                        Comment = comment,
                        User = user,
                    };

                    //add
                    reviewDb.UserThumbsUp.Add(userThumbsUp);
                }
                await reviewDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Review_UserThumbsUp_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Review_UserThumbsDown_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Review_UserThumbsDown_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        int bunchIndex = 0;
        int bunchSize = 10_000;
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            while (true)
            {
                List<Review_UserThumbsDown_SeedModel> list = await reviewDb.UserThumbsDown
                .OrderBy(uc => uc.UserId)
                .ThenBy(uc => uc.CommentId)
                .Skip(bunchIndex * bunchSize)
                .Take(bunchSize)
                .Select(uc => new Review_UserThumbsDown_SeedModel()
                {
                    Comment_Guid = uc.Comment.Guid,
                    User_Guid = uc.User.Guid,
                })
                .AsSplitQuery()
                .ToListAsync();

                if (list.Count == 0) break;

                string json = JsonSerializer.Serialize(list, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Review_UserThumbsDown_Directory.FullName, $"list_{bunchIndex}");
                await File.WriteAllTextAsync(filePath, json);

                if (list.Count < bunchSize) break;

                bunchIndex++;
            }
        }

        //set new status
        status.Getting_Review_UserThumbsDown_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Review_UserThumbsDown_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Review_UserThumbsDown_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Review_DbContext reviewDb = scope.ServiceProvider.GetRequiredService<Review_DbContext>();

            foreach (FileInfo fileInfo in Backup_Review_UserThumbsDown_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                List<Review_UserThumbsDown_SeedModel>? list;
                try
                {
                    list = JsonSerializer.Deserialize<List<Review_UserThumbsDown_SeedModel>>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (list is null || list.Count == 0) continue;

                foreach (Review_UserThumbsDown_SeedModel seedModel in list)
                {
                    //fetch
                    Review_UserDbModel? user = await reviewDb.Users
                    .Where(u => u.Guid == seedModel.User_Guid)
                    .Select(u => new Review_UserDbModel() { Id = u.Id })
                    .FirstOrDefaultAsync();
                    if (user is null) continue;

                    Review_CommentDbModel? comment = await reviewDb.Comments
                    .Where(c => c.Guid == seedModel.Comment_Guid)
                    .Select(c => new Review_CommentDbModel() { Id = c.Id })
                    .FirstOrDefaultAsync();
                    if (comment is null) continue;

                    //attach
                    reviewDb.Users.Attach(user);
                    reviewDb.Comments.Attach(comment);

                    //create join table
                    Review_UserThumbsDown_DbModel userThumbsDown = new()
                    {
                        Comment = comment,
                        User = user,
                    };

                    //add
                    reviewDb.UserThumbsDown.Add(userThumbsDown);
                }
                await reviewDb.SaveChangesAsync();
            }
        }

        //set new status
        status.Seeding_Review_UserThumbsDown_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }

    public async Task Get_Notification_Notification_Backup()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Backup_Status_FilePath);
        Backup_Status status = JsonSerializer.Deserialize<Backup_Status>(statusInJson) ?? new();
        //set new status
        status.Getting_Notification_Notification_Backup = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Notification_DbContext notifDb = scope.ServiceProvider.GetRequiredService<Notification_DbContext>();

            IAsyncEnumerable<Notification_Notification_SeedModel> dbQuery = notifDb.Notifications
            .Select(n => new Notification_Notification_SeedModel()
            {
                CreatedAt = n.CreatedAt,
                Guid = n.Guid,
                Description = n.Description,
                Link = n.Link,
                Owner_Guid = n.Owner.Guid,
                SubjectGuid = n.SubjectGuid,
                Title = n.Title,
            })
            .AsSplitQuery()
            .AsAsyncEnumerable();

            await foreach (Notification_Notification_SeedModel seedModel in dbQuery)
            {
                string json = JsonSerializer.Serialize(seedModel, jsonSerializerOptions);
                string filePath = Path.Combine(Backup_Notification_Notification_Directory.FullName, seedModel.Guid.ToString("N"));
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        //set new status
        status.Getting_Notification_Notification_Backup = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Backup_Status_FilePath, statusInJson);
    }
    public async Task Seed_Notification_Notification_Db()
    {
        //define backup status
        string statusInJson = await File.ReadAllTextAsync(Seed_Status_FilePath);
        Seed_Status status = JsonSerializer.Deserialize<Seed_Status>(statusInJson) ?? new();
        //set new status
        status.Seeding_Notification_Notification_Db = StatusEnum.Started.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            Notification_DbContext notifDb = scope.ServiceProvider.GetRequiredService<Notification_DbContext>();

            foreach (FileInfo fileInfo in Backup_Notification_Notification_Directory.EnumerateFiles())
            {
                string json = await File.ReadAllTextAsync(fileInfo.FullName);
                Notification_Notification_SeedModel? seedModel;
                try
                {
                    seedModel = JsonSerializer.Deserialize<Notification_Notification_SeedModel>(json, jsonSerializerOptions);
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine(e.Message);
                    continue;
                }
                if (seedModel is null) continue;

                Notification_NotificationDbModel notif = seedModel.GetDbModel();

                //owner
                Notification_UserDbModel? owner = await notifDb.Users
                .Where(u => u.Guid == seedModel.Owner_Guid)
                .Select(u => new Notification_UserDbModel()
                {
                    Id = u.Id,
                })
                .FirstOrDefaultAsync();
                if (owner is null)
                {
                    owner = new()
                    {
                        Guid = seedModel.Owner_Guid,
                    };
                    notifDb.Users.Add(owner);
                }
                else
                {
                    notifDb.Users.Attach(owner);
                }

                notif.Owner = owner;

                notifDb.Notifications.Add(notif);
            }
            await notifDb.SaveChangesAsync();
        }

        //set new status
        status.Seeding_Notification_Notification_Db = StatusEnum.Completed.ToString();
        statusInJson = JsonSerializer.Serialize(status);
        //write status
        await File.WriteAllTextAsync(Seed_Status_FilePath, statusInJson);
    }





    //***************** seed models *****************
    class Identity_User_SeedModel
    {
        public Guid UserGuid { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? ParsswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? Description { get; set; }
        public bool DisplayEmailPublicly { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasImage { get; set; }
        public string[] Roles { get; set; } = [];

        public static Identity_User_SeedModel Factory(Identity_UserDbModel dbModel, string[] roles)
        {
            return new Identity_User_SeedModel()
            {
                CreatedAt = dbModel.CreatedAt,
                Description = dbModel.Description,
                DisplayEmailPublicly = dbModel.DisplayEmailPublicly,
                Email = dbModel.Email,
                EmailConfirmed = dbModel.EmailConfirmed,
                HasImage = dbModel.HasImage,
                ParsswordHash = dbModel.PasswordHash,
                UserGuid = dbModel.UserGuid,
                UserName = dbModel.UserName,
                Roles = roles,
            };
        }

        public Identity_UserDbModel GetDbModel()
        {
            return new Identity_UserDbModel()
            {
                CreatedAt = this.CreatedAt,
                Description = this.Description,
                DisplayEmailPublicly = this.DisplayEmailPublicly,
                Email = this.Email,
                EmailConfirmed = this.EmailConfirmed,
                HasImage = this.HasImage,
                PasswordHash = this.ParsswordHash,
                UserGuid = this.UserGuid,
                UserName = this.UserName,
            };
        }
    }

    class Library_Owner_SeedModel
    {
        public Guid Guid { get; set; }
        public string NormalizedUserName { get; set; } = null!;
        public Guid DefaultLibraryGuid { get; set; }
        public Guid DefaultShelfGuid { get; set; }

        public Library_OwnerDbModel GetDbModel()
        {
            return new Library_OwnerDbModel()
            {
                DefaultLibraryGuid = this.DefaultLibraryGuid,
                DefaultShelfGuid = this.DefaultShelfGuid,
                Guid = this.Guid,
                NormalizedUserName = this.NormalizedUserName,
            };
        }
    }
    class Library_Library_SeedModel
    {
        public Guid Guid { get; set; }
        public Guid Owner_Guid { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool HasImage { get; set; } = false;

        public Library_LibraryDbModel GetDbModel()
        {
            return new Library_LibraryDbModel()
            {
                CreatedAt = this.CreatedAt,
                Description = this.Description,
                Guid = this.Guid,
                HasImage = this.HasImage,
                Title = this.Title,
            };
        }
    }
    class Library_Shelf_SeedModel
    {
        public Guid Guid { get; set; }
        public Guid Owner_Guid { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool HasImage { get; set; } = false;

        public Library_ShelfDbModel GetDbModel()
        {
            return new Library_ShelfDbModel()
            {
                CreatedAt = this.CreatedAt,
                Description = this.Description,
                Guid = this.Guid,
                HasImage = this.HasImage,
                Title = this.Title,
            };
        }
    }
    class Library_Document_SeedModel
    {
        public Guid Guid { get; set; }
        public Guid Owner_Guid { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Version { get; set; } = "Default";
        public Guid? RelatedVersions_Guid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool HasImage { get; set; } = false;

        public Library_DocumentDbModel GetDbModel()
        {
            return new Library_DocumentDbModel()
            {
                CreatedAt = this.CreatedAt,
                Description = this.Description,
                Guid = this.Guid,
                HasImage = this.HasImage,
                Title = this.Title,
                Version = this.Version,
            };
        }
    }
    class Library_Element_SeedModel
    {
        public Guid Guid { get; set; }
        public Guid Owner_Guid { get; set; }
        public string Type { get; set; } = null!;
        public string? Value { get; set; } = null;
        public string? Title { get; set; } = null;
        public string? FileName { get; set; } = null;
        public int Order { get; set; }
        public Guid ParentDocument_Guid { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Library_ElementDbModel GetDbModel()
        {
            return new Library_ElementDbModel()
            {
                FileName = this.FileName,
                Guid = this.Guid,
                Order = this.Order,
                Title = this.Title,
                Type = this.Type,
                UpdatedAt = this.UpdatedAt,
                Value = this.Value,
            };
        }
    }
    class Library_FollowerFollowing_SeedModel
    {
        public Guid Follower_Guid { get; set; }
        public Guid Following_Guid { get; set; }
    }
    class Library_UserFavoriteLibrary_SeedModel
    {
        public Guid User_Guid { get; set; }
        public Guid Library_Guid { get; set; }
    }
    class Library_UserFavoriteShelf_SeedModel
    {
        public Guid User_Guid { get; set; }
        public Guid Shelf_Guid { get; set; }
    }
    class Library_UserFavoriteDocument_SeedModel
    {
        public Guid User_Guid { get; set; }
        public Guid Document_Guid { get; set; }
    }
    class Library_LibraryShelf_SeedModel
    {
        public Guid Library_Guid { get; set; }
        public Guid Shelf_Guid { get; set; }
    }
    class Library_ShelfDocument_SeedModel
    {
        public Guid Shelf_Guid { get; set; }
        public Guid Document_Guid { get; set; }
    }
    class Library_DocumentTag_SeedModel
    {
        public Guid Document_Guid { get; set; }
        public string Tag_Name { get; set; } = null!;
    }

    class Review_User_SeedModel
    {
        public Guid Guid { get; set; }
        public string NormalizedUserName { get; set; } = null!;

        public Review_UserDbModel GetDbModel()
        {
            return new Review_UserDbModel()
            {
                Guid = this.Guid,
                NormalizedUserName = this.NormalizedUserName,
            };
        }
    }
    class Review_Review_SeedModel
    {
        public Guid SubjectGuid { get; set; }
        public Guid Owner_Guid { get; set; }

        public Review_ReviewDbModel GetDbModel()
        {
            return new Review_ReviewDbModel()
            {
                SubjectGuid = this.SubjectGuid,
            };
        }
    }
    class Review_Comment_SeedModel
    {
        public Guid Guid { get; set; }
        public Guid ParentReview_SubjectGuid { get; set; }
        public Guid Writer_Guid { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? ReplyTo_Guid { get; set; } = null;

        public Review_CommentDbModel GetDbModel()
        {
            return new Review_CommentDbModel()
            {
                CreatedAt = this.CreatedAt,
                Guid = this.Guid,
                Text = this.Text,
            };
        }
    }
    class Review_UserLike_SeedModel
    {
        public Guid User_Guid { get; set; }
        public Guid Review_Guid { get; set; }
    }
    class Review_UserThumbsUp_SeedModel
    {
        public Guid User_Guid { get; set; }
        public Guid Comment_Guid { get; set; }
    }
    class Review_UserThumbsDown_SeedModel
    {
        public Guid User_Guid { get; set; }
        public Guid Comment_Guid { get; set; }
    }

    class Notification_Notification_SeedModel
    {
        public Guid Guid { get; set; }
        public Guid? SubjectGuid { get; set; } = null;
        public Guid Owner_Guid { get; set; }
        public string Title { get; set; } = null!;
        public string[] Description { get; set; } = [];
        public string? Link { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Notification_NotificationDbModel GetDbModel()
        {
            return new Notification_NotificationDbModel()
            {
                CreatedAt = this.CreatedAt,
                Description = this.Description,
                Guid = this.Guid,
                Link = this.Link,
                SubjectGuid = this.SubjectGuid,
                Title = this.Title,
            };
        }
    }

}
