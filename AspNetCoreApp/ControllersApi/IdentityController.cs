using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
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
    //readonly DirectoryInfo usersImagesDirectoryInfo;
    readonly Identity_Process identityProcess;
    readonly DirectoryInfo Storage_Users;





    public IdentityController(SignInManager<Identity_UserDbModel> signInManager,
    UserManager<Identity_UserDbModel> userManager, Identity_Process identityProcess)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.identityProcess = identityProcess;
        Storage_Users = identityProcess.Storage_Users;
        //usersImagesDirectoryInfo =
        //Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "Storage", "Identity", "UsersImages"));
    }





    [HttpGet]
    [GenerateAntiforgeryTokenCookie]
    [Authorize]
    public IActionResult GetCsrf()
    {
        return Ok();
    }





    [HttpPost]
    public async Task<IActionResult> Login([FromBody] Identity_LoginFormModel loginModel,
    [FromServices] IConfiguration configuration, [FromServices] TurnstileService turnstileService)
    {
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
                    if (!user.EmailConfirmed)
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

                            return Ok(new
                            {
                                token,
                                expiresInHours = jwtSettings["DurationInHours"] ?? "10",
                                user = new Identity_UserProfileModel()
                                {
                                    Guid = user.UserGuid,
                                    Username = user.UserName!,
                                    Description = user.Description,
                                    Email = user.Email!,
                                    DisplayEmailPublicly = user.DisplayEmailPublicly,
                                    Roles = (await userManager.GetRolesAsync(user)).ToArray(),
                                    HasImage = user.HasImage,
                                    IntegrityVersion = user.IntegrityVersion,
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
            if (user.DisplayEmailPublicly)
            {
                return Ok(new Identity_UserProfileModel()
                {
                    Guid = user.UserGuid,
                    Username = user.UserName!,
                    Description = user.Description,
                    Email = user.Email!,
                    HasImage = user.HasImage,
                    IntegrityVersion = user.IntegrityVersion,
                });
            }
            else
            {
                return Ok(new Identity_UserProfileModel()
                {
                    Guid = user.UserGuid,
                    Username = user.UserName!,
                    Description = user.Description,
                    HasImage = user.HasImage,
                    IntegrityVersion = user.IntegrityVersion,
                });
            }
        }
        return BadRequest(ModelState);
    }





    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] Identity_SignupFormModel signupModel,
    [FromServices] IEmailSender emailSender, [FromServices] TurnstileService turnstileService)
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
                };
                IdentityResult result = await userManager.CreateAsync(user, signupModel.Password);
                if (result.Succeeded)
                {
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

    [HttpGet]
    public async Task<IActionResult> CheckUsername([FromQuery][StringLength(60)] string username)
    {
        bool userExist = await userManager.Users.AnyAsync(u => u.NormalizedUserName == userManager.NormalizeName(username));
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
        " to confirm your email.</p>";

        await emailSender.SendEmailAsync(user.UserName!, user.Email!,
        "Email Validation", emailMessage);
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitUsername(
        //[FromBody] string username,//BadRequest status 400, username is required, The JSON value could not be converted to System.String. it didn't work even by newtonsoft json.
        [FromBody] UsernameModel model)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
            var result = await userManager.SetUserNameAsync(user, model.Username);
            if (result.Succeeded)
            {
                string token = await userManager.GenerateUserTokenAsync(user, "customTokenProvider", "login");
                // user seed
                //await identityProcess.UpdateUserSeed(user, userManager);
                return Ok(new { success = true, token });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("Username", error.Description);
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
    [Authorize]
    //[ValidateAntiForgeryToken]//there's no need to the antiforgery when turnstile is set
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
        Console.WriteLine($"\n***** in SendNewEmailValidationLink");
        //***** Generate Email Validation Token *****
        string token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        Console.WriteLine($"\n***** token generated");

        //***** Sending Email *****
        string emailMessage = $"<h4>Hi dear {user.UserName}</h4>" +
        "<p>Please click " +
        $"<a href='https://localhost:5443/Identity/ConfirmNewEmail?userGuid={user.UserGuid}&token={token}&newEmail={newEmail}' " +
        "target='_blank'>'Here'</a>" +
        " to confirm your new email.</p>";

        await emailSender.SendEmailAsync(user.UserName!, newEmail!,
        "New Email Validation", emailMessage);

        Console.WriteLine($"\n***** email sent");
    }




    /*
        private async Task<string?> GetUserImageAddress(string userGuid)
        {
            Identity_UserDbModel? user = await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
            {
                return null;
            }

            string userImagePath =
            Path.Combine(Storage_Users.FullName, user.UserGuid, "image");
            if (System.IO.File.Exists(userImagePath))
            {
                return $"/api/Identity/UserImage?userGuid={user.UserGuid}&v={user.IntegrityVersion}";
            }

            return null;
        }
        private string? GetUserImageAddressWithVersion(string userGuid, int version)
        {
            string userImagePath =
            Path.Combine(Storage_Users.FullName, userGuid, "image");
            if (System.IO.File.Exists(userImagePath))
            {
                return $"/api/Identity/UserImage?userGuid={userGuid}&v={version}";
            }

            return null;
        }
    */
    [HttpGet]
    public IActionResult UserImage([FromQuery][StringLength(32)] string userGuid)
    {
        string userImagePath =
        Path.Combine(Storage_Users.FullName, userGuid, "image");
        if (System.IO.File.Exists(userImagePath))
        {
            return PhysicalFile(userImagePath, "application/octet-stream", "userImage", true);
        }

        return NotFound("User Image Not Found!");
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(128 * 1024)]//128 KB
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitUserImage(UserImageFileModel model)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
            string userImagePath =
            Path.Combine(Storage_Users.FullName, user.UserGuid, "image");
            using (FileStream fs = System.IO.File.Create(userImagePath))
            {
                await model.UserImageFile.CopyToAsync(fs);
            }
            user.HasImage = true;
            user.IntegrityVersion++;
            await userManager.UpdateAsync(user);

            return Ok(new { success = true, user.HasImage, user.IntegrityVersion });
        }
        return BadRequest(ModelState);
    }
    public class UserImageFileModel
    {
        public IFormFile UserImageFile { get; set; } = null!;
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserImage()
    {
        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        string userImagePath =
        Path.Combine(Storage_Users.FullName, user.UserGuid, "image");

        if (System.IO.File.Exists(userImagePath))
        {
            System.IO.File.Delete(userImagePath);
            user.HasImage = false;
            user.IntegrityVersion = 0;
            await userManager.UpdateAsync(user);
        }

        return Ok(new { success = true });
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDescription([FromBody] DescriptionModel model)
    {
        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        user.Description = model.Description;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // user seed
            //await identityProcess.UpdateUserSeed(user, userManager);
            return Ok(new { success = true });
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return BadRequest(ModelState);
    }
    public class DescriptionModel
    {
        [StringLength(500)]
        public string? Description { get; set; } = string.Empty;
    }





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromBody] Identity_ChangePasswordFormModel formModel)
    {
        if (ModelState.IsValid)
        {
            Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;

            var result = await userManager.ChangePasswordAsync(user, formModel.CurrentPassword, formModel.NewPassword);
            if (result.Succeeded)
            {
                string token = await userManager.GenerateUserTokenAsync(user, "customTokenProvider", "login");
                //user seed
                //await identityProcess.UpdateUserSeed(user, userManager);
                return Ok(new { success = true, token });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("ChangePassword", error.Description);
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





    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDisplayEmailPublicly([FromBody] DisplayEmailPubliclyModel model)
    {
        Identity_UserDbModel user = (await userManager.FindByNameAsync(User.Identity!.Name!))!;
        user.DisplayEmailPublicly = model.DisplayEmailPublicly;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // user seed
            //await identityProcess.UpdateUserSeed(user, userManager);
            return Ok(new { success = true });
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return BadRequest(ModelState);
    }
    public class DisplayEmailPubliclyModel
    {
        public bool DisplayEmailPublicly { get; set; }
    }





    //********************************* admin ********************************
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Identity_Admins")]
    public async Task<IActionResult> UsersList([FromQuery] UsersListFilterModel? filter)
    {
        filter ??= new();

        DateTime? createdTo = null;
        if (filter.CreatedTo is not null)
        {
            try
            {
                createdTo = JsonSerializer.Deserialize<DateTime>(filter.CreatedTo);
            }
            catch
            {
                createdTo = null;
            }
        }
        DateTime? createdFrom = null;
        if (filter.CreatedFrom is not null)
        {
            try
            {
                createdFrom = JsonSerializer.Deserialize<DateTime>(filter.CreatedFrom);
            }
            catch
            {
                createdFrom = null;
            }
        }

        //Console.WriteLine($"\n***** filter json: {JsonSerializer.Serialize(filter)}");

        List<UsersListModel> allFilteredUsers = [];
        if (filter.SortDirection == "asc")
        {
            if (filter.SortProperty == "CreatedAt")
            {
                allFilteredUsers = await userManager.Users
                .Where(user =>
                    (filter.UserName == null || user.NormalizedUserName!.Contains(userManager.NormalizeName(filter.UserName))) &&
                    (filter.Email == null || user.NormalizedEmail!.Contains(userManager.NormalizeEmail(filter.Email))) &&
                    (createdFrom == null || user.CreatedAt >= createdFrom) &&
                    (createdTo == null || user.CreatedAt <= createdTo) &&
                    (filter.DisplayEmailPublicly == null || filter.DisplayEmailPublicly == user.DisplayEmailPublicly) &&
                    (filter.EmailConfirmed == null || filter.EmailConfirmed == user.EmailConfirmed)
                )
                .OrderBy(user => user.CreatedAt)
                .Skip(filter.Page!.Value * filter.PageSize!.Value)
                .Take(filter.PageSize!.Value)
                .Select(user => new UsersListModel()
                {
                    CreatedAt = user.CreatedAt,
                    UserName = user.UserName!,
                    UserGuid = user.UserGuid,
                    Email = user.Email!,
                    EmailConfirmed = user.EmailConfirmed,
                    DisplayEmailPublicly = user.DisplayEmailPublicly,
                    //ImageAddress = await GetUserImageAddress(user.UserGuid),
                    IntegrityVersion = user.IntegrityVersion,
                    HasImage = user.HasImage,
                })
                .ToListAsync();
            }
            //return Ok(new { usersList = allFilteredUsers.ToArray(), totalResultsLength = allFilteredUsersLength });
        }
        else
        {
            if (filter.SortProperty == "CreatedAt")
            {
                allFilteredUsers = await userManager.Users
                .Where(user =>
                    (filter.UserName == null || user.NormalizedUserName!.Contains(userManager.NormalizeName(filter.UserName))) &&
                    (filter.Email == null || user.NormalizedEmail!.Contains(userManager.NormalizeEmail(filter.Email))) &&
                    (createdFrom == null || user.CreatedAt >= createdFrom) &&
                    (createdTo == null || user.CreatedAt <= createdTo) &&
                    (filter.DisplayEmailPublicly == null || filter.DisplayEmailPublicly == user.DisplayEmailPublicly) &&
                    (filter.EmailConfirmed == null || filter.EmailConfirmed == user.EmailConfirmed)
                )
                .OrderByDescending(user => user.CreatedAt)
                .Skip(filter.Page!.Value * filter.PageSize!.Value)
                .Take(filter.PageSize!.Value)
                .Select(user => new UsersListModel()
                {
                    CreatedAt = user.CreatedAt,
                    UserName = user.UserName!,
                    UserGuid = user.UserGuid,
                    Email = user.Email!,
                    EmailConfirmed = user.EmailConfirmed,
                    DisplayEmailPublicly = user.DisplayEmailPublicly,
                    //ImageAddress = $"/api/Identity/UserImage?userGuid=${user.UserGuid}&v=${user.Version}",
                    IntegrityVersion = user.IntegrityVersion,
                    HasImage = user.HasImage,
                })
                .ToListAsync();
            }
            else//descending order by CreatedAt as fallback
            {
                allFilteredUsers = await userManager.Users
                .Where(user =>
                    (filter.UserName == null || user.NormalizedUserName!.Contains(userManager.NormalizeName(filter.UserName))) &&
                    (filter.Email == null || user.NormalizedEmail!.Contains(userManager.NormalizeEmail(filter.Email))) &&
                    (createdFrom == null || user.CreatedAt >= createdFrom) &&
                    (createdTo == null || user.CreatedAt <= createdTo) &&
                    (filter.DisplayEmailPublicly == null || filter.DisplayEmailPublicly == user.DisplayEmailPublicly) &&
                    (filter.EmailConfirmed == null || filter.EmailConfirmed == user.EmailConfirmed)
                )
                .OrderByDescending(user => user.CreatedAt)
                .Skip(filter.Page!.Value * filter.PageSize!.Value)
                .Take(filter.PageSize!.Value)
                .Select(user => new UsersListModel()
                {
                    CreatedAt = user.CreatedAt,
                    UserName = user.UserName!,
                    UserGuid = user.UserGuid,
                    Email = user.Email!,
                    EmailConfirmed = user.EmailConfirmed,
                    DisplayEmailPublicly = user.DisplayEmailPublicly,
                    //ImageAddress = $"/api/Identity/UserImage?userGuid=${user.UserGuid}&v=${user.Version}",
                    IntegrityVersion = user.IntegrityVersion,
                    HasImage = user.HasImage,
                })
                .ToListAsync();
            }
        }

        int allFilteredUsersLength = allFilteredUsers.Count;
        if (allFilteredUsers.Count == filter.PageSize)
        {
            allFilteredUsersLength = await userManager.Users
            .Where(user =>
                (filter.UserName == null || user.NormalizedUserName!.Contains(userManager.NormalizeName(filter.UserName))) &&
                (filter.Email == null || user.NormalizedEmail!.Contains(userManager.NormalizeEmail(filter.Email))) &&
                (createdFrom == null || user.CreatedAt >= createdFrom) &&
                (createdTo == null || user.CreatedAt <= createdTo) &&
                (filter.DisplayEmailPublicly == null || filter.DisplayEmailPublicly == user.DisplayEmailPublicly) &&
                (filter.EmailConfirmed == null || filter.EmailConfirmed == user.EmailConfirmed)
            )
            .CountAsync();
        }

        return Ok(new { usersList = allFilteredUsers.ToArray(), totalResultsLength = allFilteredUsersLength });
    }






}