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
    [FromServices] Library_DbContext libraryDb, [FromServices] Library_Process libraryProcess)
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
                // creating Default library and shelf
                await libraryProcess.CreateDefaultShelf(libraryDb, user.UserGuid);

                //await identityProcess.UpdateUserSeed(user, userManager);

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
    [FromServices] Library_Process libraryProcess)
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
                // creating Default library and shelf
                await libraryProcess.CreateDefaultShelf(libraryDb, user.UserGuid);

                //await identityProcess.UpdateUserSeed(user, userManager);

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
    /*
        private async Task CreateDefaultLibraryAndShelf(Library_process libraryProcess, Library_DbContext libraryDb,
        string ownerGuid)
        {
            // creating Default library
            Library_NewLibrayFormModel libraryFormModel = new()
            {
                Title = "Default Library",
                Decription = "Containing all shelves that doesn't belong to anyother libraries."
            };
            var createDefaultLibraryResult = await libraryProcess.CreateNewLibrary(libraryDb, ownerGuid, libraryFormModel);

            Library_LibraryDbModel? defaultLibrary;
            if (createDefaultLibraryResult.Success &&
            createDefaultLibraryResult.ResultObject is not null)
            {
                defaultLibrary = (Library_LibraryDbModel)createDefaultLibraryResult.ResultObject;
            }
            else
            {
                defaultLibrary = await libraryDb.Libraries.FirstOrDefaultAsync(lib =>
                lib.OwnerGuid == ownerGuid && lib.Title == "Default Library");
            }
            if (defaultLibrary is null)
            {
                //log
                Console.WriteLine("\n***** /Identity/CreateDefaultLibraryAndShelf, defaultLibrary is null! Couldn't create Default library and shelf");
            }
            else
            {
                // creating Default shelf in Default library
                Library_NewShelfFormModel shelfFormModel = new()
                {
                    Title = "Default Shelf",
                    Decription = "Containing all documents that doesn't belong to anyother shelves.",
                    LibraryGuid = defaultLibrary.Guid,
                };
                var createDefaultShelfResult = await libraryProcess.CreateNewShelf(libraryDb, ownerGuid, shelfFormModel);
                if (!createDefaultShelfResult.Success)
                {
                    //log
                    Console.WriteLine("\n***** /Identity/CreateDefaultLibraryAndShelf, Couldn't create Default shelf!");
                }
            }
        }
    */



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
                //await identityProcess.UpdateUserSeed(user, userManager);

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