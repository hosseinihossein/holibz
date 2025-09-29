using System.ComponentModel.DataAnnotations;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreApp.ControllersApi;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    readonly SignInManager<Identity_UserDbModel> signInManager;
    readonly UserManager<Identity_UserDbModel> userManager;

    public IdentityController(SignInManager<Identity_UserDbModel> signInManager, UserManager<Identity_UserDbModel> userManager,
    IWebHostEnvironment env)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
    }

    /*[HttpGet("csrft")]
    public IActionResult GetAntiForgeryToken(IAntiforgery antiforgery)
    {
        // Generate and return the anti-forgery token
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { csrft = tokens.RequestToken });
    }*/

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024)]// 5 KB
    public async Task<IActionResult> Login([FromBody] Identity_LoginModel loginModel,
    [FromServices] IConfiguration configuration)
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
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result =
                await signInManager.CheckPasswordSignInAsync(user, loginModel.Password, true);

                if (result.Succeeded)
                {
                    string token = await userManager.GenerateUserTokenAsync(user, "customTokenProvider", "login");
                    var jwtSettings = configuration.GetSection("JwtSettings");
                    return Ok(new { token, expiresInHours = jwtSettings["DurationInHours"] ?? "10" });
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

        return BadRequest(ModelState);
    }

    /*[HttpPost("signup")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024)]// 5 KB
    public async Task<IActionResult> CreateNewAccount([FromBody] Identity_SignupModel signupModel,
    [FromServices] IEmailSender emailSender)
    {

    }*/

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
}