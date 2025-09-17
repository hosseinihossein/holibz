using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApp.ControllersApi;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    readonly SignInManager<Identity_UserDbModel> signInManager;
    readonly UserManager<Identity_UserDbModel> userManager;
    readonly IWebHostEnvironment env;
    readonly IEmailSender emailSender;

    public IdentityController(SignInManager<Identity_UserDbModel> signInManager, UserManager<Identity_UserDbModel> userManager,
    IWebHostEnvironment env, IEmailSender _emailSender/*, RoleManager<Identity_RoleModel> roleManager,
    WebComponents_DbContext webComponentsDb*/)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.env = env;
        emailSender = _emailSender;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(Identity_LoginModel loginModel)
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
                ModelState.AddModelError("", "Your Account is inactive! Contact to admin.");
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result =
                await signInManager.PasswordSignInAsync(user, loginModel.Password, loginModel.IsPersistent, false);

                if (result.Succeeded)
                {
                    return Redirect(loginModel.ReturnUrl ?? "/");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Credentials");
                }
            }
        }
        else
        {
            ModelState.AddModelError("", "Invalid Username or Email");
        }

        return BadRequest(ModelState);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> CreateNewAccount()
    {

    }
}