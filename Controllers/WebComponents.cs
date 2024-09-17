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

namespace holibz.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class WebComponentsController : Controller
    {
        readonly WebComponents_DbContext webComponentsDb;
        readonly UserManager<Identity_UserModel> userManager;
        readonly IWebHostEnvironment env;
        readonly char ds = Path.DirectorySeparatorChar;
        readonly ElasticsearchClient esClient;
        public WebComponentsController(WebComponents_DbContext _webComponentsDb, UserManager<Identity_UserModel> _userManager,
        IWebHostEnvironment _env, ElasticsearchClient esClient)
        {
            webComponentsDb = _webComponentsDb;
            userManager = _userManager;
            env = _env;
            this.esClient = esClient;
        }
        public async Task<IActionResult> Index(string? SelectedTags, string? DeveloperUserName,
        string? SearchPhrase, int page = 1)
        {
            List<Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<WebComponents_ElasticsearchModel>>> actionList =
            new();
            string? developerGuid = null;
            if (!string.IsNullOrWhiteSpace(DeveloperUserName))
            {
                developerGuid = (await userManager.FindByNameAsync(DeveloperUserName))?.UserGuid;
                if (developerGuid is not null)
                {
                    var action = new Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<WebComponents_ElasticsearchModel>>(must =>
                    {
                        must.Term(t => t.Field(Field.FromString("developerGuid")!)
                            .Value(developerGuid)
                        );
                    });

                    actionList.Add(action);
                }
                else
                {
                    WebComponents_IndexModel indexModel01 = new()
                    {
                        Items = [],
                        TagsJson = JsonSerializer.Serialize((await webComponentsDb.Tags.ToListAsync()).Select(t => t.Name).ToArray()),
                        SelectedTags = SelectedTags ?? string.Empty,
                        DeveloperUserName = DeveloperUserName ?? string.Empty,
                        SearchPhrase = SearchPhrase ?? string.Empty,
                    };

                    //int totalFoundItems = searchResponse.Hits.Count;

                    return View(indexModel01);
                }
            }

            string? selectedTags = null;
            if (!string.IsNullOrWhiteSpace(SelectedTags))
            {
                string[] tags = SelectedTags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (tags.Length > 0)
                {
                    selectedTags = tags.Aggregate((a, b) => a + " " + b);

                    var action = new Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<WebComponents_ElasticsearchModel>>(must =>
                    {
                        must.Match(m => m.Field(Field.FromString("tags")!)
                            .Query(selectedTags)
                            .Operator(Elastic.Clients.Elasticsearch.QueryDsl.Operator.And)
                        );
                    });

                    actionList.Add(action);
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchPhrase))
            {
                var action = new Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<WebComponents_ElasticsearchModel>>(must =>
                {
                    must.MultiMatch(mm =>
                    {
                        mm.Fields(Fields.FromStrings(["title", "description", "tags"]))
                        .Query(SearchPhrase)
                        .Operator(Elastic.Clients.Elasticsearch.QueryDsl.Operator.Or);
                        //.Fuzziness(new Fuzziness(1));
                    });
                });

                actionList.Add(action);
            }
            //***** use search server *****
            var searchResponse = await esClient.SearchAsync<WebComponents_ElasticsearchModel>(s =>
            {
                s.Index("webcomponents_index")
                .From((page - 1) * 12)
                .Size(12)
                //.Sort(i => i.Doc(d => d.Order(SortOrder.Desc)).Field(Field.FromString("date")!))
                .Query(q =>
                q.Bool(b =>
                {
                    b.Must(actionList.ToArray());
                }))
                .Source(new SourceConfig(false));
            });

            if (searchResponse is null || !searchResponse.IsValidResponse)
            {
                Console.WriteLine("********************** searchResponse *********************");
                Console.WriteLine("actionList.Count: " + actionList.Count);
                Console.WriteLine(searchResponse);
                Console.WriteLine("searchResponse?.Hits.Count: " + searchResponse?.Hits.Count);
                Console.WriteLine("**********************************************************");
                return NotFound();
            }

            List<WebComponents_ItemDbModel> itemDbModels = new();
            List<WebComponents_ItemModel> itemModels = new();
            foreach (var hit in searchResponse.Hits)
            {
                WebComponents_ItemDbModel? itemDbModel =
                await webComponentsDb.Items.FirstOrDefaultAsync(item => item.Guid == hit.Id);
                if (itemDbModel is not null)
                {
                    //itemDbModels.Add(itemDbModel);
                    Identity_UserModel? developer =
                    await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == itemDbModel.DeveloperGuid);
                    WebComponents_ItemModel itemModel = new()
                    {
                        Guid = itemDbModel.Guid,
                        Developer = developer,
                        Title = itemDbModel.Title,
                        Description = itemDbModel.Description,
                        Tags = [.. itemDbModel.TagDbModels.Select(t => t.Name)],
                        SearchScore = hit.Score.ToString() ?? string.Empty,
                    };
                    itemModels.Add(itemModel);
                }
            }

            WebComponents_IndexModel indexModel = new()
            {
                Items = itemModels,
                TagsJson = JsonSerializer.Serialize((await webComponentsDb.Tags.ToListAsync()).Select(t => t.Name).ToArray()),
                SelectedTags = SelectedTags ?? string.Empty,
                DeveloperUserName = DeveloperUserName ?? string.Empty,
                SearchPhrase = SearchPhrase ?? string.Empty,
            };

            //int totalFoundItems = searchResponse.Hits.Count;
            ViewBag.CurrentPage = page;
            ViewBag.LastPage = (int)Math.Ceiling(searchResponse.Total / 12.0);

            return View(indexModel);
        }

        //********************************* Item *********************************
        [Authorize]
        public async Task<IActionResult> NewItem()
        {
            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"User \'{User.Identity.Name}\' Not Found!";
                return View("Result", o);
            }
            if (!user.EmailConfirmed)
            {
                ViewBag.ResultState = "danger";
                object o = "Please Validate your Email first!";
                return View("Result", o);
            }

            WebComponents_NewItemFormModel newItemFormModel = new()
            {
                TagsJson = JsonSerializer.Serialize(await webComponentsDb.Tags.Select(tag => tag.Name).ToArrayAsync())
            };
            return View(newItemFormModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitNewItem(WebComponents_NewItemFormModel formModel)
        {
            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"User \'{User.Identity.Name}\' Not Found!";
                return View("Result", o);
            }
            if (!user.EmailConfirmed)
            {
                ViewBag.ResultState = "danger";
                object o = "Please Validate your Email first!";
                return View("Result", o);
            }

            if (ModelState.IsValid)
            {
                //***** create item dir *****
                string guid = Guid.NewGuid().ToString().Replace("-", "");
                string itemDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{guid}";
                Directory.CreateDirectory(itemDirectory);

                //************* zip file *************
                using (FileStream fs = System.IO.File.Create(itemDirectory + ds + guid + ".zip"))
                {
                    await formModel.ZipFile!.CopyToAsync(fs);
                }

                //***** extract zip file *****
                ZipFile.ExtractToDirectory(itemDirectory + ds + guid + ".zip", itemDirectory);

                //************* save on database *************
                List<WebComponents_TagDbModel> tagDbModels = new();
                string[] tags = formModel.SelectedTags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (string tag in tags)
                {
                    WebComponents_TagDbModel? tagDbModel = await webComponentsDb.Tags.FirstOrDefaultAsync(t => t.Name == tag);
                    if (tagDbModel != null)
                    {
                        tagDbModels.Add(tagDbModel);
                    }
                }
                //***** add item to webComponentsDb *****
                WebComponents_ItemDbModel itemDbModel = new()
                {
                    Guid = guid,
                    DeveloperGuid = user.UserGuid,
                    Title = formModel.Title,
                    Description = formModel.Description,
                    TagDbModels = tagDbModels
                };
                await webComponentsDb.Items.AddAsync(itemDbModel);
                await webComponentsDb.SaveChangesAsync();

                //***** add developer project's number *****
                //user.ProjectsNumber++;
                //await userManager.UpdateAsync(user);

                //***** add item to search server *****
                WebComponents_ElasticsearchModel elasticsearchModel = new()
                {
                    Guid = itemDbModel.Guid,
                    DeveloperGuid = itemDbModel.DeveloperGuid,
                    Title = itemDbModel.Title,
                    Description = itemDbModel.Description,
                    Tags = tags.Aggregate((a, b) => a + " " + b),
                    Date = itemDbModel.Date
                };
                var indexResponse = await esClient.IndexAsync(elasticsearchModel,
                (IndexName)"webcomponents_index", (Id)elasticsearchModel.Guid);

                if (!indexResponse.IsValidResponse)
                {
                    Console.WriteLine("********************** indexResponse *********************");
                    Console.WriteLine(indexResponse);
                    Console.WriteLine("**********************************************************");
                }

                //*********** respond to user ***********
                //return View("Result", "the web item created successfully");
                return RedirectToAction(nameof(ItemDetail), new { itemGuid = itemDbModel.Guid });
            }

            WebComponents_NewItemFormModel newItemFormModel = new()
            {
                TagsJson = JsonSerializer.Serialize(await webComponentsDb.Tags.Select(tag => tag.Name).ToArrayAsync())
            };
            return View(nameof(NewItem), newItemFormModel);
        }

        [Authorize]
        public async Task<IActionResult> DeleteItem(string itemGuid)
        {
            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Couldn't find user with name \'{User.Identity.Name ?? "unkown"}\'";
                return View("Result", o);
            }

            WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items
            .FirstOrDefaultAsync(item => item.Guid == itemGuid);
            if (itemDbModel is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Couldn't find item with guid \'{itemGuid}\'";
                return View("Result", o);
            }

            if (!await userManager.IsInRoleAsync(user, "WebComponents_Admins") && itemDbModel.DeveloperGuid != user.UserGuid)
            {
                ViewBag.ResultState = "danger";
                object o = "Only developers can delete their own projects!";
                return View("Result", o);
            }

            //***** WebComponentsDb *****
            webComponentsDb.Items.Remove(itemDbModel);
            await webComponentsDb.SaveChangesAsync();

            //***** Item directory *****
            string itemDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{itemGuid}";
            Directory.Delete(itemDirectory, true);

            //user.ProjectsNumber--;
            //await userManager.UpdateAsync(user);

            //***** remove from Elasticsearch *****
            var deleteResponse = await esClient.DeleteAsync((IndexName)"webcomponents_index", (Id)itemGuid);
            if (!deleteResponse.IsValidResponse)
            {
                Console.WriteLine("********************** deleteResponse *********************");
                Console.WriteLine(deleteResponse);
                Console.WriteLine("***********************************************************");
            }

            ViewBag.ResultState = "success";
            object o1 = $"Item with guid \'{itemGuid}\' has been removed successfully.";
            return View("Result", o1);
        }

        public async Task<IActionResult> ItemDetail(string itemGuid)
        {
            string itemDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{itemGuid}";
            if (!System.IO.Directory.Exists(itemDirectory))
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find item '{itemGuid}'!";
                return View("Result", o);
            }

            //***** item *****
            WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items
            .Include(item => item.TagDbModels)
            .FirstOrDefaultAsync(item => item.Guid == itemGuid);
            if (itemDbModel is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find ItemModel width name '{itemGuid}' on Inventory Datbase!";
                return View("Result", o);
            }

            //***** developer *****
            Identity_UserModel? developer = await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == itemDbModel.DeveloperGuid);

            //***** tags *****
            List<string> tags = itemDbModel.TagDbModels.Select(t => t.Name).ToList();

            WebComponents_ItemModel itemModel = new()
            {
                Guid = itemDbModel.Guid,
                Developer = developer,
                Title = itemDbModel.Title,
                Description = itemDbModel.Description,
                Tags = tags
            };

            return View(itemModel);
        }

        //********** edit in item details **********
        [Authorize]
        public async Task<IActionResult> EditItemTags(string itemGuid)
        {
            WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items
                .Include(item => item.TagDbModels)
                .FirstOrDefaultAsync(item => item.Guid == itemGuid);
            if (itemDbModel is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find item with guid \'{itemGuid}\'";
                return View("Result", o);
            }

            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = "Can't recognize user identity";
                return View("Result", o);
            }

            if (!await userManager.IsInRoleAsync(user, "WebComponents_Admins") && itemDbModel.DeveloperGuid != user.UserGuid)
            {
                ViewBag.ResultState = "danger";
                object o = "Only developers can edit their item tags!";
                return View("Result", o);
            }

            List<string> currentTagList = itemDbModel.TagDbModels.Select(t => t.Name).ToList();
            List<string> allTagList = (await webComponentsDb.Tags.ToListAsync()).Select(t => t.Name).ToList();
            WebComponents_EditItemTagsModel editItemTagsModel = new()
            {
                CurrentTags = currentTagList,
                AllTags = allTagList,
                ItemGuid = itemDbModel.Guid
            };

            return View(editItemTagsModel);
        }

        [Authorize]
        public async Task<IActionResult> SubmitEditItemTags(string itemGuid, string selectedTags)
        {
            WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items
                .Include(item => item.TagDbModels)
                .FirstOrDefaultAsync(item => item.Guid == itemGuid);
            if (itemDbModel is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find item with guid \'{itemGuid}\'";
                return View("Result", o);
            }

            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = "Can't recognize user identity";
                return View("Result", o);
            }

            if (!await userManager.IsInRoleAsync(user, "WebComponents_Admins") && itemDbModel.DeveloperGuid != user.UserGuid)
            {
                ViewBag.ResultState = "danger";
                object o = "Only developers can edit their item tags!";
                return View("Result", o);
            }

            string[] tags = selectedTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            List<WebComponents_TagDbModel> newTagDbModels = new();
            foreach (string tag in tags)
            {
                WebComponents_TagDbModel? tagDbModel = await webComponentsDb.Tags
                .FirstOrDefaultAsync(t => t.Name == tag);
                if (tagDbModel is not null)
                {
                    newTagDbModels.Add(tagDbModel);
                }
            }
            itemDbModel.TagDbModels = newTagDbModels;
            await webComponentsDb.SaveChangesAsync();

            //***** update Elasticsearch *****
            //***** GetAsync *****
            var getResponse = await esClient.GetAsync<WebComponents_ElasticsearchModel>(
                (Id)itemGuid, idx => idx.Index("webcomponents_index")
            );
            if (getResponse.IsValidResponse)
            {
                WebComponents_ElasticsearchModel? updateItem = getResponse.Source;
                if (updateItem is not null)
                {
                    updateItem.Tags = tags.Aggregate((a, b) => a + " " + b);

                    //***** UpdateAsync *****
                    var updateResponse =
                    await esClient.UpdateAsync<WebComponents_ElasticsearchModel, WebComponents_ElasticsearchModel>(
                        (IndexName)"webcomponents_index", (Id)itemGuid, u => u.Doc(updateItem)
                    );
                }
            }

            //***** return *****
            return RedirectToAction(nameof(ItemDetail), new { itemGuid });
        }

        [Authorize]
        public async Task<IActionResult> SubmitTitle(string itemGuid, string title)
        {
            WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items
                .Include(item => item.TagDbModels)
                .FirstOrDefaultAsync(item => item.Guid == itemGuid);
            if (itemDbModel is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find item with guid \'{itemGuid}\'";
                return View("Result", o);
            }

            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = "Can't recognize user identity";
                return View("Result", o);
            }

            if (!await userManager.IsInRoleAsync(user, "WebComponents_Admins") && itemDbModel.DeveloperGuid != user.UserGuid)
            {
                ViewBag.ResultState = "danger";
                object o = "Only developers can edit their item title!";
                return View("Result", o);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ViewBag.ResultState = "danger";
                object o = "Title can not be empty or whitespace!";
                return View("Result", o);
            }

            //***** webcomponentsDb *****
            itemDbModel.Title = title;
            await webComponentsDb.SaveChangesAsync();

            //***** update Elasticsearch *****
            //***** GetAsync *****
            var getResponse = await esClient.GetAsync<WebComponents_ElasticsearchModel>(
                (Id)itemGuid, idx => idx.Index("webcomponents_index")
            );
            if (getResponse.IsValidResponse)
            {
                WebComponents_ElasticsearchModel? updateItem = getResponse.Source;
                if (updateItem is not null)
                {
                    updateItem.Title = title;

                    //***** UpdateAsync *****
                    var updateResponse =
                    await esClient.UpdateAsync<WebComponents_ElasticsearchModel, WebComponents_ElasticsearchModel>(
                        (IndexName)"webcomponents_index", (Id)itemGuid, u => u.Doc(updateItem)
                    );
                }
            }

            return RedirectToAction(nameof(ItemDetail), new { itemGuid });
        }

        [Authorize]
        public async Task<IActionResult> SubmitDescription(string itemGuid, string description)
        {
            WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items
                .Include(item => item.TagDbModels)
                .FirstOrDefaultAsync(item => item.Guid == itemGuid);
            if (itemDbModel is null)
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find item with guid \'{itemGuid}\'";
                return View("Result", o);
            }

            Identity_UserModel? user = await userManager.FindByNameAsync(User.Identity!.Name!);
            if (user is null)
            {
                ViewBag.ResultState = "danger";
                object o = "Can't recognize user identity";
                return View("Result", o);
            }

            if (!await userManager.IsInRoleAsync(user, "WebComponents_Admins") && itemDbModel.DeveloperGuid != user.UserGuid)
            {
                ViewBag.ResultState = "danger";
                object o = "Only developers can edit their item description!";
                return View("Result", o);
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                ViewBag.ResultState = "danger";
                object o = "Description can not be empty or whitespace!";
                return View("Result", o);
            }

            //***** webcomponentsDb *****
            itemDbModel.Description = description;
            await webComponentsDb.SaveChangesAsync();

            //***** update Elasticsearch *****
            //***** GetAsync *****
            var getResponse = await esClient.GetAsync<WebComponents_ElasticsearchModel>(
                (Id)itemGuid, idx => idx.Index("webcomponents_index")
            );
            if (getResponse.IsValidResponse)
            {
                WebComponents_ElasticsearchModel? updateItem = getResponse.Source;
                if (updateItem is not null)
                {
                    updateItem.Description = description;

                    //***** UpdateAsync *****
                    var updateResponse =
                    await esClient.UpdateAsync<WebComponents_ElasticsearchModel, WebComponents_ElasticsearchModel>(
                        (IndexName)"webcomponents_index", (Id)itemGuid, u => u.Doc(updateItem)
                    );
                }
            }

            return RedirectToAction(nameof(ItemDetail), new { itemGuid });
        }

        //********** source **********
        [Route("/WebComponents/ItemSource/{itemGuid}/{fileName}")]
        public async Task<IActionResult> GetItemSource(string itemGuid, string fileName)
        {
            string filePath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{itemGuid}{ds}{fileName}";
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            if (new string(fileName.TakeLast(5).ToArray()) == ".html")
            {
                string content = await System.IO.File.ReadAllTextAsync(filePath);
                return View("HtmlContent", content);
            }

            return new PhysicalFileResult(filePath, "application/file");
        }

        [Route("/WebComponents/ItemSource/{itemGuid}/statics/{fileName}")]
        public IActionResult GetItemStatics(string itemGuid, string fileName)
        {
            string filePath =
            $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{itemGuid}{ds}statics{ds}{fileName}";
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            return new PhysicalFileResult(filePath, "application/file");
        }

        public async Task<IActionResult> CodeText(string itemGuid)
        {
            string itemPath =
            env.ContentRootPath + ds + "SpecificStorage" + ds + "WebComponents" + ds + "Library" + ds + itemGuid;
            if (!System.IO.Directory.Exists(itemPath))
            {
                ViewBag.ResultState = "danger";
                object o = $"Can't find item {itemGuid} !";
                return View("Result", o);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(itemPath);
            FileInfo[] htmlFileInfos = directoryInfo.GetFiles("*.html");
            string content = "";
            foreach (FileInfo htmlFileInfo in htmlFileInfos)
            {
                content += $"\n******************** {htmlFileInfo.Name} ********************\n";
                content += await System.IO.File.ReadAllTextAsync(htmlFileInfo.FullName);
            }

            return View("CodeText", content);
        }

        //******************************* Backup *********************************
        [Authorize(Roles = "WebComponents_Admins")]
        public IActionResult Backup()
        {
            string backupDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Backup";
            string backupZipFilePath = $"{backupDirectory}{ds}backup.zip";
            if (System.IO.File.Exists(backupZipFilePath))
            {
                ViewBag.BackupDate = System.IO.File.GetCreationTime(backupZipFilePath);
            }

            ViewBag.ControllerName = "WebComponents";
            return View();
        }

        [Authorize(Roles = "WebComponents_Admins")]
        public async Task<IActionResult> RenewBackup()
        {
            string mainDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Backup";
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

            foreach (WebComponents_ItemDbModel item in await webComponentsDb.Items.Include(item => item.TagDbModels).ToListAsync())
            {
                WebComponents_BackupItemModel model = new()
                {
                    Guid = item.Guid,
                    DeveloperGuid = item.DeveloperGuid,
                    Title = item.Title,
                    Description = item.Description,
                    Tags = item.TagDbModels.Select(t => t.Name).ToList(),
                    Date = item.Date
                };
                string json = JsonSerializer.Serialize(model);

                string itemDirectoryPath = $"{dataDirectoryPath}{ds}{item.Guid}";
                Directory.CreateDirectory(itemDirectoryPath);

                string jsonDataFilePath = $"{itemDirectoryPath}{ds}data.json";
                await System.IO.File.WriteAllTextAsync(jsonDataFilePath, json);

                string itemSourceZipFilePath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{item.Guid}{ds}{item.Guid}.zip";
                if (System.IO.File.Exists(itemSourceZipFilePath))
                {
                    string itemDestinationZipFilePath = $"{itemDirectoryPath}{ds}{item.Guid}.zip";
                    using (FileStream fsSource = System.IO.File.Open(itemSourceZipFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (FileStream fsDestination = System.IO.File.Create(itemDestinationZipFilePath))
                        {
                            await fsSource.CopyToAsync(fsDestination);
                        }
                    }
                }
            }

            ZipFile.CreateFromDirectory(dataDirectoryPath, backupZipFilePath);

            return RedirectToAction(nameof(Backup));
        }

        [Authorize(Roles = "WebComponents_Admins")]
        public IActionResult DownloadBackup()
        {
            string backupDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Backup";
            if (Directory.Exists(backupDirectory))
            {
                string backupZipFilePath = $"{backupDirectory}{ds}backup.zip";
                if (System.IO.File.Exists(backupZipFilePath))
                {
                    return PhysicalFile(backupZipFilePath, "File/zip");
                }
            }
            object o = "backup file Not found!";
            ViewBag.ResultState = "danger";
            return View("Result", o);
            //return null;
        }

        [Authorize(Roles = "WebComponents_Admins")]
        public IActionResult DeleteBackup()
        {
            string backupDirectory = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Backup";
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
        [Authorize(Roles = "WebComponents_Admins")]
        public async Task<IActionResult> UploadBackup(IFormFile backupZipFile)
        {
            if (ModelState.IsValid)
            {
                string mainDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Backup";
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

        [Authorize(Roles = "WebComponents_Admins")]
        public async Task<IActionResult> RecoverLastBackup()
        {
            string mainDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Backup";
            string dataDirectoryPath = $"{mainDirectoryPath}{ds}Data";
            DirectoryInfo dataDirectoryInfo = new DirectoryInfo(dataDirectoryPath);
            foreach (DirectoryInfo itemDirInfo in dataDirectoryInfo.EnumerateDirectories())
            {
                string jsonPath = $"{itemDirInfo.FullName}{ds}data.json";
                string json = await System.IO.File.ReadAllTextAsync(jsonPath, Encoding.UTF8);
                WebComponents_BackupItemModel? model = JsonSerializer.Deserialize<WebComponents_BackupItemModel>(json);
                if (model is null)
                {
                    continue;
                }
                WebComponents_ItemDbModel? itemDbModel = await webComponentsDb.Items.FirstOrDefaultAsync(item => item.Guid == model.Guid);
                if (itemDbModel != null)
                {
                    continue;
                }
                List<WebComponents_TagDbModel> tagDbModels = new();
                foreach (string tag in model.Tags)
                {
                    WebComponents_TagDbModel? tagDbModel = await webComponentsDb.Tags.FirstOrDefaultAsync(t => t.Name == tag);
                    if (tagDbModel is not null)
                    {
                        tagDbModels.Add(tagDbModel);
                    }
                }
                itemDbModel = new()
                {
                    Guid = model.Guid,
                    DeveloperGuid = model.DeveloperGuid,
                    Title = model.Title,
                    Description = model.Description,
                    TagDbModels = tagDbModels,
                    Date = model.Date,

                };
                await webComponentsDb.Items.AddAsync(itemDbModel);
                string itemDestinationZipFilePath = $"{itemDirInfo.FullName}{ds}{itemDbModel.Guid}.zip";
                if (System.IO.File.Exists(itemDestinationZipFilePath))
                {
                    string itemSourceDirectoryPath = $"{env.ContentRootPath}{ds}SpecificStorage{ds}WebComponents{ds}Library{ds}{itemDbModel.Guid}";
                    string itemSourceZipFilePath = $"{itemSourceDirectoryPath}{ds}{itemDbModel.Guid}.zip";
                    using (FileStream fsSource = System.IO.File.Open(itemDestinationZipFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (FileStream fsDestination = System.IO.File.Create(itemSourceZipFilePath))
                        {
                            await fsSource.CopyToAsync(fsDestination);
                        }
                    }
                    ZipFile.ExtractToDirectory(itemSourceZipFilePath, itemSourceDirectoryPath);
                }
            }

            ViewBag.ResultState = "success";
            object o = $"Identity recovery completed successfully.";
            return View("Result", o);
        }
        //************************************************************************
    }
}