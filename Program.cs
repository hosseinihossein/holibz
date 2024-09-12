using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using holibz.Models;

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
            opts.UseSqlServer(builder.Configuration["ConnectionStrings:IdentityConnection"]);
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
            opts.UseSqlServer(builder.Configuration["ConnectionStrings:WebComponentsConnection"],
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        //****************************** Services *****************************
        builder.Services.AddControllersWithViews();
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<ElasticsearchClient>((sp) =>
        {
            return new ElasticsearchClient(
                new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
                .CertificateFingerprint("b8e2f502856cd163d3a43cbe30f0344b811ef05813ea31099544a3df375717f6")
                .Authentication(new BasicAuthentication("elastic", "cg_R=r*g-*QwYNZFWQdR"))
            );
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
                PasswordLiteral = "P@ssw0rd"
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
