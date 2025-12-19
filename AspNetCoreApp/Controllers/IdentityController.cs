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
    [FromQuery][StringLength(60)] string email, [FromServices] Identity_Process identityProcess,
    [FromServices] Library_DbContext libraryDb, [FromServices] Library_Process libraryProcess,
    Review_Process reviewProcess, Review_DbContext reviewDb)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                object userNotFoundMessage = "<h2>User Not found!!</h2>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            if (user.EmailConfirmed)
            {
                object o1 = "<h2>Your Email has already confirmed. Don't need to confirm anymore!</h2>";
                ViewBag.ResultState = "info";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", o1);
            }

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // creating Library_OwnerDbModel and its default library and shelf
                await libraryProcess.CreateNewOwner(libraryDb, user.UserGuid);
                // creating Review_UserDbModel
                await reviewProcess.CreateNewUser(reviewDb, user.UserGuid);

                //seed
                _ = identityProcess.Update_UserSeed(user, userManager);

                object successMessage = "<h2>Your Email Successfully Confirmed.</h2>";
                ViewBag.ResultState = "success";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", successMessage);
            }

            object incorrectVal = "<h2>Your email validation link Is Incorrect or Expired!</h2>";
            ViewBag.ResultState = "danger";
            //ViewBag.InfoBtnName = "Resend";
            //ViewBag.InfoBtnHref = $"/Identity/ResendEmailValidation?email={email}";
            return View("Result", incorrectVal);
        }
        return BadRequest(ModelState);
    }

    public async Task<IActionResult> ConfirmNewEmail([FromQuery][StringLength(32)] string userGuid,
    [FromQuery] string token, [FromQuery][StringLength(60)] string newEmail,
    [FromServices] Identity_Process identityProcess, [FromServices] Library_DbContext libraryDb,
    [FromServices] Library_Process libraryProcess, Review_Process reviewProcess, Review_DbContext reviewDb)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user =
            await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
            {
                object userNotFoundMessage = "<h2>User Not found!!</h2>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            if (user.Email == newEmail)
            {
                object o1 = "<h2>Your Email has already been changed successfully.</h2>";
                ViewBag.ResultState = "info";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", o1);
            }

            IdentityResult result = await userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                // creating Library_OwnerDbModel and its default library and shelf
                await libraryProcess.CreateNewOwner(libraryDb, user.UserGuid);
                // creating Review_UserDbModel
                await reviewProcess.CreateNewUser(reviewDb, user.UserGuid);

                //seed
                _ = identityProcess.Update_UserSeed(user, userManager);

                object successMessage = "<h2>Your Email Successfully Changed. You need to login again to see changes.</h2>";
                ViewBag.ResultState = "success";
                ViewBag.InfoBtnName = "Login";
                ViewBag.InfoBtnHref = "/angularapp/browser/login/";
                return View("Result", successMessage);
            }

            object incorrectVal = "<h2>Your email validation link Is Incorrect or Expired!</h2>";
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
                object userNotFoundMessage = "<h2>User Not found!!</h2>";
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
                object userNotFoundMessage = "<h2>User Not found!!</h2>";
                ViewBag.ResultState = "danger";
                return View("Result", userNotFoundMessage);
            }

            var result =
            await userManager.ResetPasswordAsync(user, formModel.Token, formModel.NewPassword);
            if (result.Succeeded)
            {
                //seed
                _ = identityProcess.Update_UserSeed(user, userManager);

                object successMessage = "<h2>Your new password successfully set.</h2>";
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