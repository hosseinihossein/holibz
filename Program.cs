using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using holibz.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace holibz;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //******************* SQL Server DataBase Services *******************
        //***** Identity *****
        builder.Services.AddDbContext<Identity_DbContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration["WindowsConnectionStrings:IdentityConnection"]);
            //opts.UseSqlServer(builder.Configuration["UbuntuConnectionStrings:IdentityConnection"]);
        });
        builder.Services.AddIdentity<Identity_UserModel, Identity_RoleModel>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Tokens.EmailConfirmationTokenProvider = "emailTokenProvider";
        })
        .AddTokenProvider<Identity_EmailTokenProvider>("emailTokenProvider")
        .AddEntityFrameworkStores<Identity_DbContext>();

        //***** WebComponents *****
        builder.Services.AddDbContext<WebComponents_DbContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration["WindowsConnectionStrings:WebComponentsConnection"],
            //opts.UseSqlServer(builder.Configuration["UbuntuConnectionStrings:WebComponentsConnection"],
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        //****************************** Services *****************************
        builder.Services.AddControllersWithViews();
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddSingleton<ElasticsearchClient>((sp) =>
        {
            string elasticServerAddress = builder.Configuration["ElasticSearch:ServerAddress"]!;
            return new ElasticsearchClient(
                new ElasticsearchClientSettings(new Uri($"https://{elasticServerAddress}:9200"))
                .CertificateFingerprint(builder.Configuration["ElasticSearch:CertificateFingerprint"]!)
                .Authentication(new BasicAuthentication(builder.Configuration["ElasticSearch:Username"]!,
                builder.Configuration["ElasticSearch:Password"]!))
            );
        });
        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, opts =>
        {
            opts.AccessDeniedPath = "/Identity/AccessDenied";
            opts.LoginPath = "/Identity/Login";
        });

        //******************************** app ********************************
        var app = builder.Build();

        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapDefaultControllerRoute();

        //************************** Seed SQL Server DataBases ***************************
        IWebHostEnvironment env = app.Services.GetRequiredService<IWebHostEnvironment>();

        //***** Seed "admin" Identity *****
        Identity_DbContext identityDb = app.Services.CreateScope().ServiceProvider.GetRequiredService<Identity_DbContext>();
        identityDb.Database.Migrate();
        UserManager<Identity_UserModel> userManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<Identity_UserModel>>();
        RoleManager<Identity_RoleModel> roleManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<RoleManager<Identity_RoleModel>>();
        Identity_UserModel? admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new Identity_UserModel
            {
                UserName = "admin",
                Email = "admin@MyCompany.com",
                EmailConfirmed = true,
                UserGuid = "admin",
                PasswordLiteral = builder.Configuration["Identity:AdminPassword"]!
            };
            IdentityResult result = await userManager.CreateAsync(admin, admin.PasswordLiteral);
            if (!result.Succeeded)
            {

            }
        }
        if (await roleManager.FindByNameAsync("Identity_Admins") == null)
        {
            await roleManager.CreateAsync(new Identity_RoleModel("Identity_Admins") { Description = "Top admins of the Identity service." });
            await userManager.AddToRoleAsync(admin, "Identity_Admins");
        }
        if (await roleManager.FindByNameAsync("WebComponents_Admins") == null)
        {
            await roleManager.CreateAsync(new Identity_RoleModel("WebComponents_Admins") { Description = "Top admins of the WebComponents service." });
            await userManager.AddToRoleAsync(admin, "WebComponents_Admins");
        }


        app.Run();
    }
}
