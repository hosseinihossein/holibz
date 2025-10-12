using System.ComponentModel.DataAnnotations;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.Controllers;

[AutoValidateAntiforgeryToken]
public class IdentityController : Controller
{
    readonly UserManager<Identity_UserDbModel> userManager;

    public IdentityController(UserManager<Identity_UserDbModel> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<IActionResult> ConfirmEmail([FromQuery] string token,
    [FromQuery][StringLength(60)] string email, [FromServices] Identity_Process identityProcess)
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
                object o1 = "<h1>Your Email has already confirmed. Don't need to confirm anymore!</h1>";
                ViewBag.ResultState = "info";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", o1);
            }

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await identityProcess.UpdateUserSeed(user);

                object successMessage = "<h1>Your Email Successfully Confirmed.</h1>";
                ViewBag.ResultState = "success";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", successMessage);
            }

            object incorrectVal = "<h1>Your email validation link Is Incorrect or Expired!</h1>";
            ViewBag.ResultState = "danger";
            //ViewBag.InfoBtnName = "Resend";
            //ViewBag.InfoBtnHref = $"/Identity/ResendEmailValidation?email={email}";
            return View("Result", incorrectVal);
        }
        return BadRequest(ModelState);
    }

    public async Task<IActionResult> ConfirmNewEmail([FromQuery][StringLength(32)] string userGuid,
    [FromQuery] string token, [FromQuery][StringLength(60)] string newEmail,
    [FromServices] Identity_Process identityProcess)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user =
            await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
            {
                object userNotFoundMessage = "<h1>User Not found!!</h1>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            if (user.Email == newEmail)
            {
                object o1 = "<h1>Your Email has already been changed successfully.</h1>";
                ViewBag.ResultState = "info";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", o1);
            }

            IdentityResult result = await userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                await identityProcess.UpdateUserSeed(user);

                object successMessage = "<h2>Your Email Successfully Changed. You may need to login again.</h2>";
                ViewBag.ResultState = "success";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", successMessage);
            }

            object incorrectVal = "<h1>Your email validation link Is Incorrect or Expired!</h1>";
            ViewBag.ResultState = "danger";
            //ViewBag.InfoBtnName = "Resend";
            //ViewBag.InfoBtnHref = $"/Identity/ResendEmailValidation?newEmail={newEmail}";
            return View("Result", incorrectVal);
        }
        return BadRequest(ModelState);
    }

    public async Task<IActionResult> ResetPassword([FromQuery] string token,
    [FromQuery][StringLength(32)] string userGuid)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user =
            await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
            {
                object userNotFoundMessage = "<h1>User Not found!!</h1>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            Identity_ResetPasswordFormModel formModel = new()
            {
                UserGuid = user.UserGuid,
                Token = token,
            };
            return View(formModel);
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitResetPassword(Identity_ResetPasswordFormModel formModel,
    [FromServices] Identity_Process identityProcess)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user =
            await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == formModel.UserGuid);
            if (user is null)
            {
                object userNotFoundMessage = "<h1>User Not found!!</h1>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            var result =
            await userManager.ResetPasswordAsync(user, formModel.Token, formModel.NewPassword);
            if (result.Succeeded)
            {
                await identityProcess.UpdateUserSeed(user);

                object successMessage = "<h1>Your new password successfully set.</h1>";
                ViewBag.ResultState = "success";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", successMessage);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("SubmitResetPassword", error.Description);
            }
        }
        return BadRequest(ModelState);
    }



}