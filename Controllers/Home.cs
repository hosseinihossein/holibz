using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using holibz.Models;
using Microsoft.Extensions.Primitives;

namespace holibz.Controllers;

public class HomeController : Controller
{
    readonly IWebHostEnvironment env;
    readonly IEmailSender emailSender;
    readonly IConfiguration configManager;
    readonly WebComponents_DbContext webComponentsDb;
    readonly Identity_DbContext identityDb;

    public HomeController(IWebHostEnvironment _env, IEmailSender emailSender, IConfiguration configManager,
    WebComponents_DbContext webComponentsDb, Identity_DbContext identityDb)
    {
        env = _env;
        this.emailSender = emailSender;
        this.configManager = configManager;
        this.webComponentsDb = webComponentsDb;
        this.identityDb = identityDb;
    }

    public async Task<IActionResult> Index()
    {
        List<WebComponents_ItemDbModel> itemDbModels =
        await webComponentsDb.Items.OrderByDescending(item => item.Date).Take(9).ToListAsync();
        List<WebComponents_ItemModel> itemModels = new(9);
        int indexCounter = 3;
        foreach (WebComponents_ItemDbModel itemDbModel in itemDbModels)
        {
            Identity_UserModel? developer =
            await identityDb.Users.FirstOrDefaultAsync(u => u.UserGuid == itemDbModel.DeveloperGuid);
            WebComponents_ItemModel itemModel = new()
            {
                Guid = itemDbModel.Guid,
                Developer = developer,
                Title = itemDbModel.Title,
            };
            itemModels.Insert(indexCounter, itemModel);
            indexCounter++;
            if (indexCounter >= 9) indexCounter = 0;
        }
        Home_IndexModel indexModel = new() { RecentlyAddedComponents = itemModels };
        return View(indexModel);
    }
    public IActionResult ContactUs()
    {
        return View();
    }
    public async Task<IActionResult> SubmitContactUs(Home_ContactUsModel contactUsModel)
    {
        if (ModelState.IsValid)
        {
            var formData = new { secret = "0x4AAAAAAAkeZ_VQzHOlwqGq3-wl_DJ_HEw", response = contactUsModel.CfTurnstileResponse };
            string url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, formData);
            string responseContentString = await response.Content.ReadAsStringAsync();
            dynamic? responseContent = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContentString);
            if (!response.IsSuccessStatusCode || (responseContent?.success ?? false) == false)
            {
                ModelState.AddModelError("Turnstile", "Cloudn't pass the CAPTCHA!");
                return View(nameof(ContactUs));
            }

            //***** Sending Email *****
            string emailMessage = $"<p>Name: {contactUsModel.Name}</p><p>Email: {contactUsModel.Email}</p><p>Message: {contactUsModel.Message}</p>";
            string contactUsAdminEmail = configManager["ContactUsAdminEmail"]!;
            await emailSender.SendEmailAsync("ContactUs Admin", contactUsAdminEmail,
            "HoLibz ContactUs", emailMessage);

            object o = $"<p>Your message sent successfully!</p><p>Name: {contactUsModel.Name}</p><p>Email: {contactUsModel.Email}</p><p>Message: {contactUsModel.Message}</p>";
            ViewBag.ResultState = "success";
            return View("Result", o);
        }
        return View("ContactUs");
    }
}