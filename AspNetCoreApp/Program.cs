using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //******************* SQL Server DataBase Services *******************
        //***** Identity *****
        builder.Services.AddDbContext<IdentityDb>(opts =>
        {
            opts.UseMySql(builder.Configuration["ConnectionStrings_MySql:IdentityConnection"],
            new MySqlServerVersion(new Version(8, 0, 42)));
        });
        builder.Services.AddIdentity<Identity_UserDbModel, Identity_RoleDbModel>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";

            /*options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 1;*/

            options.SignIn.RequireConfirmedEmail = true;

            //options.Lockout.AllowedForNewUsers = true;
            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //options.Lockout.MaxFailedAccessAttempts = 5;

            //options.ClaimsIdentity.SecurityStampClaimType = "AspNet.Identity.SecurityStamp";
            //options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;

            options.Tokens.EmailConfirmationTokenProvider = "customTokenProvider";
            options.Tokens.ChangeEmailTokenProvider = "customTokenProvider";
            //options.Tokens.PasswordResetTokenProvider = "customTokenProvider";
            //options.Tokens.AuthenticatorTokenProvider = "customTokenProvider";
            //options.Tokens.ChangePhoneNumberTokenProvider = "customTokenProvider";

        })
        .AddTokenProvider<CustomTokenProvider>("customTokenProvider")
        .AddEntityFrameworkStores<IdentityDb>();

        //******************* SecurityStampValidatorOptions *******************
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        //******************* Authentication *******************
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = redirectContext =>
                {
                    if (redirectContext.Request.Path.StartsWithSegments("/api"))
                    {
                        redirectContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        redirectContext.Response.Redirect(redirectContext.RedirectUri);
                    }
                    return Task.CompletedTask;
                },

                OnRedirectToAccessDenied = redirectContext =>
                {
                    if (redirectContext.Request.Path.StartsWithSegments("/api"))
                    {
                        redirectContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }
                    else
                    {
                        redirectContext.Response.Redirect(redirectContext.RedirectUri);
                    }
                    return Task.CompletedTask;
                },
            };
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");

            options.RequireHttpsMetadata = true;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<Identity_UserDbModel>>();
                    var signinManager = context.HttpContext.RequestServices
                        .GetRequiredService<SignInManager<Identity_UserDbModel>>();

                    string? userGuid = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userGuid is null)
                    {
                        context.Fail("couldn't find user id in the token!");
                        return;
                    }

                    string? securityStamp = context.Principal?.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
                    if (securityStamp is null)
                    {
                        context.Fail("couldn't find security stamp in the token!");
                        return;
                    }

                    Identity_UserDbModel? user =
                        await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
                    if (user is null || user.SecurityStamp != securityStamp)
                    {
                        context.Fail("token is invalid!");
                        return;
                    }

                    context.Principal = await signinManager.CreateUserPrincipalAsync(user);
                },

            };
        });



        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
