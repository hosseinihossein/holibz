using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApp.Controllers;

[AutoValidateAntiforgeryToken]
public class IdentityController : Controller
{
    readonly UserManager<Identity_UserDbModel> userManager;

    public IdentityController(UserManager<Identity_UserDbModel> userManager)
    {
        this.userManager = userManager;
    }


    [RequestSizeLimit(5 * 1024)]// 5 KB
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                object userNotFoundMessage = "<h1>User Not found!!</h1>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            if (user.EmailConfirmed)
            {
                object o1 = "Your Email has already confirmed. Don't need to confirm anymore!";
                ViewBag.ResultState = "info";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angular/login/";
                return View("Result", o1);
            }

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                object successMessage = "<h1>Your Email Successfully Confirmed.</h1>";
                ViewBag.ResultState = "success";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angular/login/";
                return View("Result", successMessage);
            }

            object incorrectVal = "<h1>Your email validation link Is Incorrect or Expired!</h1>";
            ViewBag.ResultState = "danger";
            ViewBag.InfoBtnName = "Resend";
            ViewBag.InfoBtnHref = $"/Identity/ResendEmailValidation?email={email}";
            return View("Result", incorrectVal);
        }
        return BadRequest(ModelState);
    }

    /*public IActionResult ResendEmailValidation(string? email)
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitResendEmailValidation([FromBody]string email)
    {
        
    }*/
}