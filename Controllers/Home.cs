using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using holibz.Models;

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
    public IActionResult SubmitContactUs(Home_ContactUsModel contactUsModel)
    {
        if (ModelState.IsValid)
        {
            object o = $"<p>Name: {contactUsModel.Name}</p><p>Email: {contactUsModel.Email}</p><p>Message: {contactUsModel.Message}</p>";
            ViewBag.ResultState = "info";
            return View("Result", o);
        }
        return View("ContactUs");
    }
}