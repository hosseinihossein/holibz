using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using UploadLargeFormFile.Filters;

namespace AspNetCoreApp.ControllersApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class IdentityController : ControllerBase
{
    readonly SignInManager<Identity_UserDbModel> signInManager;
    readonly UserManager<Identity_UserDbModel> userManager;
    readonly DirectoryInfo userImageDirectoryInfo;





    public IdentityController(SignInManager<Identity_UserDbModel> signInManager,
    UserManager<Identity_UserDbModel> userManager, IWebHostEnvironment env)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;

        userImageDirectoryInfo =
        Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "Storage", "Identity", "UserImage"));
    }





    [HttpGet]
    [GenerateAntiforgeryTokenCookie]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetCsrf()
    {
        return Ok();
    }





    [HttpPost]
    public async Task<IActionResult> Login([FromBody] Identity_LoginFormModel loginModel,
    [FromServices] IConfiguration configuration, [FromServices] TurnstileService turnstileService,
    IAntiforgery antiforgery)
    {
        foreach (var header in Request.Headers)
        {
            Console.WriteLine($"\n***** {header.Key} = {header.Value}");
        }
        if (ModelState.IsValid)
        {
            var remoteip = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                HttpContext.Connection.RemoteIpAddress?.ToString();

            TurnstileResponse? turnstileResponse =
            await turnstileService.ValidateTokenAsync(loginModel.CfTurnstileResponse, remoteip);

            if (turnstileResponse is null || turnstileResponse.Success is false)
            {
                ModelState.AddModelError("TurnstileError",
                turnstileResponse is null ? "response null!" : string.Join(", ", turnstileResponse.ErrorCodes));
            }
            else
            {
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    await signInManager.SignOutAsync();
                }

                Identity_UserDbModel? user;
                if (loginModel.UsernameOrEmail.Contains('@'))
                {
                    user = await userManager.FindByEmailAsync(loginModel.UsernameOrEmail);
                }
                else
                {
                    user = await userManager.FindByNameAsync(loginModel.UsernameOrEmail);
                }

                if (user is not null)
                {
                    if (!user.ActivityAllowed)
                    {
                        ModelState.AddModelError("Inactive", "Your Account is inactive! Contact to admin.");
                    }
                    else if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError("EmailValidation", "Email Not confirmed! Please click the validation link in your email first.");
                    }
                    else
                    {
                        Microsoft.AspNetCore.Identity.SignInResult result =
                        await signInManager.CheckPasswordSignInAsync(user, loginModel.Password, true);

                        if (result.Succeeded)
                        {
                            string token = await userManager.GenerateUserTokenAsync(user, "customTokenProvider", "login");
                            var jwtSettings = configuration.GetSection("JwtSettings");

                            // Send a new request token as a JavaScript-readable cookie
                            /*var tokens = antiforgery.GetAndStoreTokens(HttpContext);

                            HttpContext.Response.Cookies.Append(
                                "XSRF-TOKEN",
                                tokens.RequestToken!,
                                new CookieOptions() { HttpOnly = false, Secure = true });*/

                            return Ok(new
                            {
                                token,
                                expiresInHours = jwtSettings["DurationInHours"] ?? "10",
                                user = new
                                {
                                    guid = user.UserGuid,
                                    username = user.UserName,
                                    description = user.Description,
                                    imageAddress = await GetUserImageAddress(user.UserGuid),
                                    email = user.Email,
                                },
                            });
                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Invalid Credentials");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Username", "Invalid Username or Email");
                }
            }
        }

        return BadRequest(ModelState);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserModel([FromQuery][StringLength(32)] string userGuid)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel? user =
            await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
            {
                ModelState.AddModelError("user", "the specified user Not found!");
                return BadRequest(ModelState);
            }
            return Ok(new
            {
                guid = user.UserGuid,
                username = user.UserName,
                description = user.Description,
                imageAddress = await GetUserImageAddress(user.UserGuid),
                email = user.Email,
            });
        }
        return BadRequest(ModelState);
    }





    [HttpPost("signup")]
    public async Task<IActionResult> CreateNewAccount([FromBody] Identity_SignupFormModel signupModel,
    [FromServices] IEmailSender emailSender, [FromServices] TurnstileService turnstileService,
    [FromServices] Identity_Process identityProcess)
    {
        if (ModelState.IsValid)
        {
            var remoteip = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                HttpContext.Connection.RemoteIpAddress?.ToString();

            TurnstileResponse? turnstileResponse =
            await turnstileService.ValidateTokenAsync(signupModel.CfTurnstileResponse, remoteip);

            if (turnstileResponse is null || turnstileResponse.Success is false)
            {
                ModelState.AddModelError("TurnstileError",
                turnstileResponse is null ? "response null!" : string.Join(", ", turnstileResponse.ErrorCodes));
            }
            else
            {
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    await signInManager.SignOutAsync();
                }

                Identity_UserDbModel user = new Identity_UserDbModel()
                {
                    UserName = signupModel.Username,
                    Email = signupModel.Email,
                    EmailConfirmed = false,
                    UserGuid = Guid.NewGuid().ToString().Replace("-", "")
                };
                IdentityResult result = await userManager.CreateAsync(user, signupModel.Password);
                if (result.Succeeded)
                {
                    await identityProcess.UpdateUserSeed(user);

                    _ = SendEmailValidationLink(user, emailSender);// commented out for development 

                    return Ok(new { success = true });
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Signup", error.Description);
                }
            }
        }
        return BadRequest(ModelState);
    }

    [HttpGet("CheckUsername")]
    public async Task<IActionResult> UsernameExist([FromQuery][StringLength(60)] string username)
    {
        /*Identity_UserDbModel? user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            return Ok(new { isTaken = false });
        }
        return Ok(new { isTaken = false });*/

        bool userExist = await userManager.Users.AnyAsync(u => u.UserName == username);
        return Ok(new { isTaken = userExist });
    }





    [HttpPost]
    public async Task<IActionResult> ResendEmailValidation([FromBody] Identity_EmailValidationFormModel emailModel,
    [FromServices] IEmailSender emailSender, [FromServices] TurnstileService turnstileService)
    {
        if (ModelState.IsValid)
        {
            var remoteip = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                HttpContext.Connection.RemoteIpAddress?.ToString();

            TurnstileResponse? turnstileResponse =
            await turnstileService.ValidateTokenAsync(emailModel.CfTurnstileResponse, remoteip);

            if (turnstileResponse is null || turnstileResponse.Success is false)
            {
                ModelState.AddModelError("TurnstileError",
                turnstileResponse is null ? "response null!" : string.Join(", ", turnstileResponse.ErrorCodes));
            }
            else
            {
                Identity_UserDbModel? user = await userManager.FindByEmailAsync(emailModel.Email);
                if (user is not null)
                {
                    await SendEmailValidationLink(user, emailSender);

                    return Ok(new { success = true });
                }
                ModelState.AddModelError("Email", "user Not found!");
            }
        }
        return BadRequest(ModelState);
    }
    private async Task SendEmailValidationLink(Identity_UserDbModel user, IEmailSender emailSender)
    {
        //***** Generate Email Validation Token *****
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        //***** Sending Email *****
        string emailMessage = $"<h4>Hi dear {user.UserName}</h4>" +
        "<p>Please click " +
        $"<a href='https://localhost:5443/Identity/ConfirmEmail?token={token}&email={user.Email}' " +
        "target='_blank'>'Here'</a>" +
        " to confirm your email validation.</p>";

        await emailSender.SendEmailAsync(user.UserName!, user.Email!,
        "Email Validation", emailMessage);
    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitUsername(
        [FromBody] UsernameModel model,
        //[FromQuery] string username,
        [FromServices] Identity_Process identityProcess,
        [FromServices] IAntiforgery antiForgery)
    {
        if (!await antiForgery.IsRequestValidAsync(HttpContext))
        {
            ModelState.AddModelError("antiforgerytoken", "anti forgery token is incorrect!");
            return BadRequest(ModelState);
        }
        if (ModelState.IsValid)
        {
            //Console.WriteLine("\n***** ModelState is valid");
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"\n***** {header.Key} = {header.Value}");
            }
            /*foreach (var claim in User.Claims)
            {
                Console.WriteLine($"\n***** claim = {claim}");
            }*/
            /*if (User.Identity is not null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    Console.WriteLine("\n***** User IsAuthenticated");
                }
                if (User.Identity.Name is not null)
                {
                    Console.WriteLine($"\n***** User.Identity.Name = {User.Identity.Name}");
                }
            }
            else
            {
                Console.WriteLine("\n***** User.Identity is null");
                ModelState.AddModelError("Identity", "User.Identity is null");
                return BadRequest(ModelState);
            }*/
            if (User.Identity?.Name is null)
            {
                Console.WriteLine("\n***** User.Identity.Name is null");
                ModelState.AddModelError("Identity", "User.Identity.Name is null");
                return BadRequest(ModelState);
            }

            Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
            var result = await userManager.SetUserNameAsync(user, model.Username);
            if (result.Succeeded)
            {
                string token = await userManager.GenerateUserTokenAsync(user, "customTokenProvider", "login");
                // user seed
                await identityProcess.UpdateUserSeed(user);
                return Ok(new { success = true, token });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return BadRequest(ModelState);
    }
    public class UsernameModel
    {
        [StringLength(60, MinimumLength = 8)]
        public string Username { get; set; } = string.Empty;
    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangeEmail([FromBody] Identity_EmailValidationFormModel formModel,
    [FromServices] IEmailSender emailSender, [FromServices] TurnstileService turnstileService)
    {
        if (ModelState.IsValid)
        {
            var remoteip = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                HttpContext.Connection.RemoteIpAddress?.ToString();

            TurnstileResponse? turnstileResponse =
            await turnstileService.ValidateTokenAsync(formModel.CfTurnstileResponse, remoteip);

            if (turnstileResponse is null || turnstileResponse.Success is false)
            {
                ModelState.AddModelError("TurnstileError",
                turnstileResponse is null ? "response null!" : string.Join(", ", turnstileResponse.ErrorCodes));
            }
            else
            {
                Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
                if (user.Email == formModel.Email)
                {
                    ModelState.AddModelError("Email", "The current and new email addresses are the same!");
                }
                else
                {
                    await SendNewEmailValidationLink(user, formModel.Email, emailSender);
                    return Ok(new { success = true });
                }
            }
        }
        return BadRequest(ModelState);
    }

    private async Task SendNewEmailValidationLink(Identity_UserDbModel user, string newEmail,
    IEmailSender emailSender)
    {
        //***** Generate Email Validation Token *****
        string token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        //***** Sending Email *****
        string emailMessage = $"<h4>Hi dear {user.UserName}</h4>" +
        "<p>Please click " +
        $"<a href='https://localhost:5443/Identity/ConfirmNewEmail?userGuid={user.UserGuid}&token={token}&newEmail={newEmail}' " +
        "target='_blank'>'Here'</a>" +
        " to confirm your new email validation.</p>";

        await emailSender.SendEmailAsync(user.UserName!, newEmail!,
        "New Email Validation", emailMessage);
    }





    private async Task<string?> GetUserImageAddress(string userGuid)
    {
        Identity_UserDbModel? user = await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
        if (user is null)
        {
            return null;
        }

        string userImagePath =
        Path.Combine(userImageDirectoryInfo.FullName, user.UserGuid);
        if (System.IO.File.Exists(userImagePath))
        {
            return $"api/Identity/UserImage?userGuid={user.UserGuid}&v={user.Version}";
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> UserImage([FromQuery][StringLength(32)] string userGuid)
    {
        Identity_UserDbModel? user = await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
        if (user is null)
        {
            return NotFound("User Not Found!");
        }

        string userImagePath =
        Path.Combine(userImageDirectoryInfo.FullName, user.UserGuid);
        if (System.IO.File.Exists(userImagePath))
        {
            return PhysicalFile(userImagePath, "application/octet-stream", "userImage", true);
        }

        return NotFound("User Image Not Found!");
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequestSizeLimit(128 * 1024)]//128 KB
    public async Task<IActionResult> SubmitUserImage(IFormFile? userImageFile = null)
    {
        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        string userImagePath = Path.Combine(userImageDirectoryInfo.FullName, user.UserGuid);
        if (userImageFile is null)
        {
            if (System.IO.File.Exists(userImagePath))
            {
                System.IO.File.Delete(userImagePath);
                user.Version++;
                await userManager.UpdateAsync(user);
            }
        }
        else
        {
            using (FileStream fs = System.IO.File.Create(userImagePath))
            {
                await userImageFile.CopyToAsync(fs);
            }
            user.Version++;
            await userManager.UpdateAsync(user);
        }

        return Ok(new { success = true });
    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SubmitDescription([FromBody][StringLength(500)] string description,
    [FromServices] Identity_Process identityProcess)
    {
        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        user.Description = description;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // user seed
            await identityProcess.UpdateUserSeed(user);
            return Ok(new { success = true });
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return BadRequest(ModelState);
    }





    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangePassword([FromBody] Identity_ChangePasswordFormModel formModel,
    [FromServices] Identity_Process identityProcess)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;

            var result = await userManager.ChangePasswordAsync(user, formModel.CurrentPassword, formModel.NewPassword);
            if (result.Succeeded)
            {
                await identityProcess.UpdateUserSeed(user);
                return Ok(new { success = true });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword([FromBody] Identity_EmailValidationFormModel formModel,
    [FromServices] IEmailSender emailSender, [FromServices] TurnstileService turnstileService)
    {
        if (ModelState.IsValid)
        {
            var remoteip = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                HttpContext.Connection.RemoteIpAddress?.ToString();

            TurnstileResponse? turnstileResponse =
            await turnstileService.ValidateTokenAsync(formModel.CfTurnstileResponse, remoteip);

            if (turnstileResponse is null || turnstileResponse.Success is false)
            {
                ModelState.AddModelError("TurnstileError",
                turnstileResponse is null ? "response null!" : string.Join(", ", turnstileResponse.ErrorCodes));
            }
            else
            {
                Identity_UserDbModel? user = await userManager.FindByEmailAsync(formModel.Email);
                if (user is not null)
                {
                    await SendResetPasswordLink(user, emailSender);

                    return Ok(new { success = true });
                }
                ModelState.AddModelError("Email", "user Not found!");
            }
        }
        return BadRequest(ModelState);
    }
    private async Task SendResetPasswordLink(Identity_UserDbModel user, IEmailSender emailSender)
    {
        //***** Generate Email Validation Token *****
        string token = await userManager.GeneratePasswordResetTokenAsync(user);

        //***** Sending Email *****
        string emailMessage = $"<h4>Hi dear {user.UserName}</h4>" +
        "<p>Please click " +
        $"<a href='https://localhost:5443/Identity/ResetPassword?token={token}&userGuid={user.UserGuid}' " +
        "target='_blank'>'Here'</a>" +
        " to proceed password reset.</p>";

        await emailSender.SendEmailAsync(user.UserName!, user.Email!,
        "Reset Password", emailMessage);
    }








}