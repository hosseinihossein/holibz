using System.Security.Claims;
using System.Text;
using AspNetCoreApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //******************* SQL Server DataBase Services *******************
        //***** Identity *****
        builder.Services.AddDbContext<Identity_DbContext>(opts =>
        {
            opts.UseMySql(builder.Configuration["ConnectionStrings_MySql:IdentityConnection"],
            new MySqlServerVersion(new Version(8, 0, 42)));
        });
        builder.Services.AddIdentity<Identity_UserDbModel, Identity_RoleDbModel>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";

            options.SignIn.RequireConfirmedEmail = true;

            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 1;

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
        .AddEntityFrameworkStores<Identity_DbContext>();

        //******************* SecurityStampValidatorOptions *******************
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new RequireHttpsAttribute());
        });
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(new RequireHttpsAttribute());
        });

        //builder.Services.AddScoped<Identity_Process>();
        builder.Services.AddTransient<IEmailSender, EmailSender>();

        //******************* Authentication *******************
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromHours(10);
            options.Cookie.Expiration = TimeSpan.FromHours(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SlidingExpiration = true;
            options.AccessDeniedPath = "/Identity/AccessDenied";
            options.LoginPath = "/Identity/Login";
            //options.LogoutPath = "";
            //options.ReturnUrlParameter = "";

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

        app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapDefaultControllerRoute();


        /********************** migrate pending databases **********************/
        Identity_DbContext identityDb = app.Services.CreateScope().ServiceProvider.GetRequiredService<Identity_DbContext>();
        identityDb.Database.Migrate();

        Console.WriteLine("** All DB Migration Completed! **");


        //************************** Seed DataBases ***************************
        IWebHostEnvironment env = app.Services.GetRequiredService<IWebHostEnvironment>();

        //***** Seed "admin" Identity *****
        UserManager<Identity_UserDbModel> userManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<Identity_UserDbModel>>();
        RoleManager<Identity_RoleDbModel> roleManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<RoleManager<Identity_RoleDbModel>>();
        Identity_UserDbModel? admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new Identity_UserDbModel
            {
                UserName = "admin",
                UserGuid = "admin",
                Email = "admin@yourdomain.com",
                EmailConfirmed = true,
                PasswordLiteral = builder.Configuration["Identity:AdminPassword"]!,
                Description = "This identity belongs to the admin of the website."
            };
            IdentityResult result = await userManager.CreateAsync(admin, admin.PasswordLiteral);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
                return;
            }
        }

        //***** Seed Roles *****
        if (await roleManager.FindByNameAsync("Identity_Admins") == null)
        {
            await roleManager.CreateAsync(new Identity_RoleDbModel("Identity_Admins") { Description = "Identity Admins" });
            await userManager.AddToRoleAsync(admin, "Identity_Admins");
        }

        //***** seed IdentityDb *****
        /*if (app.Configuration["Seed:Account"] == "true")
        {
            Console.WriteLine("** Seeding Account Service **");
            Identity_Process account_Process = app.Services.CreateScope().ServiceProvider.GetRequiredService<Identity_Process>();
            await account_Process.SeedDb();
            Console.WriteLine("** Seeding Account Service Completed! **");
        }*/

        app.Map("/", () => "Hello World");

        app.Run();
    }
}
