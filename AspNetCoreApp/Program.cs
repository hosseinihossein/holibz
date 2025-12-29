using System.Net;
using System.Security.Claims;
using System.Text;
using AspNetCoreApp.Models;
using AspNetCoreApp.Filters;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);





        //******************* Kestrel *******************
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, 5443, listenOptions =>
            {
                listenOptions.UseHttps(/*"certFileName.pfx"*/);//user default certs
            });

            options.Limits.MaxRequestBodySize = 5 * 1024;// 5 KB
        });





        //******************* Identity_DbContext *******************
        builder.Services.AddDbContext<Identity_DbContext>(opts =>
        {
            opts.UseMySql(builder.Configuration["ConnectionStrings_MySql:IdentityConnection"],
            new MySqlServerVersion(new Version(8, 0, 42)));
        });

        //******************* Library_DbContext *******************
        builder.Services.AddDbContext<Library_DbContext>(opts =>
        {
            opts.UseMySql(builder.Configuration["ConnectionStrings_MySql:LibraryConnection"],
            new MySqlServerVersion(new Version(8, 0, 42)));
        });

        //******************* Review_DbContext *******************
        builder.Services.AddDbContext<Review_DbContext>(opts =>
        {
            opts.UseMySql(builder.Configuration["ConnectionStrings_MySql:ReviewConnection"],
            new MySqlServerVersion(new Version(8, 0, 42)));
        });

        //******************* Notification_DbContext *******************
        builder.Services.AddDbContext<Notification_DbContext>(opts =>
        {
            opts.UseMySql(builder.Configuration["ConnectionStrings_MySql:NotificationConnection"],
            new MySqlServerVersion(new Version(8, 0, 42)));
        });





        //******************* Identity *******************
        builder.Services.AddIdentity<Identity_UserDbModel, Identity_RoleDbModel>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";

            options.SignIn.RequireConfirmedEmail = true;

            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 8;
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
            options.Tokens.PasswordResetTokenProvider = "customTokenProvider";
            //options.Tokens.AuthenticatorTokenProvider = "customTokenProvider";
            //options.Tokens.ChangePhoneNumberTokenProvider = "customTokenProvider";

        })
        .AddTokenProvider<CustomTokenProvider>("customTokenProvider")
        .AddEntityFrameworkStores<Identity_DbContext>();

        //******************* Authentication *******************
        builder.Services.AddAuthentication(options =>
        {
            //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        /*.AddCookie(options =>
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
        })*/
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

                    string? userStringGuid = context.Principal?.FindFirst("UserGuid")?.Value;
                    if (userStringGuid is null ||
                    !Guid.TryParseExact(userStringGuid, "N", out Guid userGuid))
                    {
                        context.Fail("couldn't find user id in the token!");
                        return;
                    }

                    string? securityStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;
                    if (securityStamp is null)
                    {
                        context.Fail("couldn't find security stamp in the token!");
                        return;
                    }

                    Identity_UserDbModel? user = await userManager.Users
                    .Where(u => u.UserGuid == userGuid)
                    .Select(u => new Identity_UserDbModel()
                    {
                        Id = u.Id,
                        UserGuid = u.UserGuid,
                        SecurityStamp = u.SecurityStamp,
                    })
                    .FirstOrDefaultAsync();
                    if (user is null || user.SecurityStamp != securityStamp)
                    {
                        //Console.WriteLine("\n***** token is invalid!");
                        context.Fail("token is invalid!");
                        return;
                    }

                    context.Principal = await signinManager.CreateUserPrincipalAsync(user);
                },

            };
        });

        //******************* SecurityStampValidatorOptions *******************
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        //******************* Controllers *******************
        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new RequireHttpsAttribute());
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new GuidJsonConverter());
        });
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(new RequireHttpsAttribute());
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new GuidJsonConverter());
        });

        //******************* IHttpClientFactory *******************
        builder.Services.AddHttpClient();

        //******************* AntiForgery *******************
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            //options.Cookie.Name = "XSRF-TOKEN";//swap error
        });





        //**************************** Custom Services **************************
        builder.Services.AddSingleton<TurnstileService>();
        builder.Services.AddSingleton<Identity_Process>();
        builder.Services.AddSingleton<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<FileExtensionContentTypeProvider>();
        builder.Services.AddSingleton<Library_Process>();
        builder.Services.AddSingleton<FileNameValidator>();
        builder.Services.AddSingleton<Review_Process>();
        builder.Services.AddSingleton<Backup_Process>();
        builder.Services.AddSingleton<Notification_Process>();





        //******************* app *******************
        var app = builder.Build();

        //**************************** app.Use ************************
        app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });

        app.UseAuthentication();
        app.UseAuthorization();





        /********************** Migrate Pending DataBases **********************/
        Identity_DbContext identityDb = app.Services.CreateScope().ServiceProvider.GetRequiredService<Identity_DbContext>();
        identityDb.Database.Migrate();

        Library_DbContext libraryDb = app.Services.CreateScope().ServiceProvider.GetRequiredService<Library_DbContext>();
        libraryDb.Database.Migrate();

        Review_DbContext reviewDb = app.Services.CreateScope().ServiceProvider.GetRequiredService<Review_DbContext>();
        reviewDb.Database.Migrate();

        Notification_DbContext notifDb = app.Services.CreateScope().ServiceProvider.GetRequiredService<Notification_DbContext>();
        notifDb.Database.Migrate();

        Console.WriteLine("** All DB Migration Completed! **");

        //************************** Seed DataBases **************************
        //***** "admin" Identity *****
        UserManager<Identity_UserDbModel> userManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<Identity_UserDbModel>>();
        RoleManager<Identity_RoleDbModel> roleManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<RoleManager<Identity_RoleDbModel>>();
        Identity_UserDbModel? admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            string adminPassword = builder.Configuration["Identity:AdminPassword"]!;
            admin = new Identity_UserDbModel
            {
                UserName = "admin",
                //UserGuid = "admin",
                Email = "admin@yourdomain.com",
                EmailConfirmed = true,
                Description = "This identity belongs to the admin of the website."
            };
            IdentityResult result = await userManager.CreateAsync(admin, adminPassword);

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
        if (await roleManager.FindByNameAsync("Backup_Admins") == null)
        {
            await roleManager.CreateAsync(new Identity_RoleDbModel("Backup_Admins") { Description = "Backup Admins" });
            await userManager.AddToRoleAsync(admin, "Backup_Admins");
        }

        //***** Seed Users *****
        /*if (app.Configuration["Seed:Identity"] == "true")
        {
            Console.WriteLine("** Seeding Identity Service Started... **");
            Identity_Process account_Process = app.Services.CreateScope().ServiceProvider.GetRequiredService<Identity_Process>();
            await account_Process.SeedUsersToDb(userManager);
            Console.WriteLine("** Seeding Identity Service Completed! **");
        }*/

        //***** Create Default Library and Shelf for everyone *****
        /*List<string> AllConfirmedUsersGuidsExceptAdmin =
        await userManager.Users
        .Where(u => u.EmailConfirmed && u.UserGuid != "admin")
        .Select(u => u.UserGuid)
        .ToListAsync();

        Library_Process libraryProcess = app.Services.CreateScope().ServiceProvider.GetRequiredService<Library_Process>();

        foreach (string userGuid in AllConfirmedUsersGuidsExceptAdmin)
        {
            var ownerCreationResult = await libraryProcess.CreateNewOwner(libraryDb, userGuid);
        }*/
        //await libraryDb.SaveChangesAsync();





        //**************************** app.Map ************************
        app.MapControllers();
        app.MapDefaultControllerRoute();

        /*app.Map("/angular/{*catchAll}", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            // Send a new request token as a JavaScript-readable cookie
            var tokens = antiforgery.GetAndStoreTokens(context);

            context.Response.Cookies.Append(
                "XSRF-TOKEN",
                tokens.RequestToken!,
                new CookieOptions() { HttpOnly = false, Secure = true });

            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(
                Path.Combine(app.Environment.WebRootPath, "AngularApp", "browser", "index.html")
            );
        });*/

        app.Map("/angularapp/browser/{*catchAll}", async (HttpContext context/*, IAntiforgery antiforgery*/) =>
        {
            string? catchAll = context.Request.RouteValues["catchAll"]?.ToString();
            if (!string.IsNullOrWhiteSpace(catchAll))
            {
                string staticFilePath =
                Path.Combine(app.Environment.WebRootPath, "AngularApp", "browser", catchAll);
                if (File.Exists(staticFilePath))
                {
                    var provider = new FileExtensionContentTypeProvider();
                    if (provider.TryGetContentType(staticFilePath, out string? contentType))
                    {
                        context.Response.ContentType = contentType;
                        await context.Response.SendFileAsync(staticFilePath);
                        return;
                    }
                    else
                    {
                        context.Response.ContentType = "application/octet-stream";
                        await context.Response.SendFileAsync(staticFilePath);
                        return;
                    }
                }
            }


            // Send a new request token as a JavaScript-readable cookie
            /*var tokens = antiforgery.GetAndStoreTokens(context);

            context.Response.Cookies.Append(
                "XSRF-TOKEN",
                tokens.RequestToken!,
                new CookieOptions() { HttpOnly = false, Secure = true });*/

            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(
                Path.Combine(app.Environment.WebRootPath, "AngularApp", "browser", "index.html")
            );
        });

        /*app.Map("/email/", async (HttpContext context, IEmailSender emailSender) =>
        {
            await emailSender.SendEmailAsync("hossein", "@gmail.com", "test",
            "<h1>the link of a site</h1><a href='https://www.'>p30download</a>");

            //context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("email sent");
        });*/

        //********* app.Map("/user*") *********
        app.Map("/users", async (HttpContext context) =>
        {
            var allUsers = await userManager.Users
            .Select(u => new { u.UserName, u.Email, u.EmailConfirmed, u.UserGuid })
            //.AsAsyncEnumerable();
            .ToArrayAsync();

            await context.Response.WriteAsJsonAsync(allUsers);
        });
        app.Map("/deleteuser/{username}", async (HttpContext context) =>
        {
            string? username = context.Request.RouteValues["username"]?.ToString();
            if (username is null)
            {
                await context.Response.WriteAsync("username can NOT be null!");
                return;
            }
            if (username == "admin")
            {
                await context.Response.WriteAsync("admin can NOT be deleted!");
                return;
            }

            Identity_UserDbModel? user = await userManager.FindByNameAsync(username);
            if (user is null)
            {
                await context.Response.WriteAsync("user NOT found!");
                return;
            }

            IdentityResult result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await context.Response.WriteAsync($"{username} deleted successfully.");
                return;
            }

            await context.Response.WriteAsJsonAsync(result.Errors);
        });

        app.Map("/", () => "Hello World");





        //******************* app.Run ******************
        Console.WriteLine("app.Run();");
        app.Run();
    }
}
