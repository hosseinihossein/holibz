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

    public HomeController(IWebHostEnvironment _env)
    {
        env = _env;
    }

    public /*async Task<IActionResult>*/ IActionResult Index()
    {
        return View();
    }
    public IActionResult ContactUs()
    {
        return View();
    }
    public async Task<IActionResult> SubmitContactUs(Home_ContactUsModel contactUsModel)
    {
        if (ModelState.IsValid)
        {
            var formDictionary = new Dictionary<string, StringValues>
            {
                { "secret", "0x4AAAAAAAkeZ_VQzHOlwqGq3-wl_DJ_HEw" },
                { "response", contactUsModel.CfTurnstileResponse },
                //{ "remoteip", ip }
            };
            var formData = new FormCollection(formDictionary);
            string url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, formData);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("Turnstile", "Cloudn't pass the CAPTCHA!");
                return View("Login");
            }

            object o = $"<p>Name: {contactUsModel.Name}</p><p>Email: {contactUsModel.Email}</p><p>Message: {contactUsModel.Message}</p>";
            ViewBag.ResultState = "info";
            return View("Result", o);
        }
        return View("ContactUs");
    }
}