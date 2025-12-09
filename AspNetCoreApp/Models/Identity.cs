using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreApp.Models;

//*********************************** IdentityDb ************************************
public class Identity_DbContext : IdentityDbContext<Identity_UserDbModel, Identity_RoleDbModel, int>
{
    public Identity_DbContext(DbContextOptions<Identity_DbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //*************************** Index Columns *********************************
        modelBuilder.Entity<Identity_UserDbModel>()
        .HasIndex(u => u.UserGuid)
        .IsUnique(true);
    }
}

public class Identity_UserDbModel : IdentityUser<int>
{
    public string UserGuid { get; set; } = Guid.NewGuid().ToString().Replace("-", "");
    public string? Description { get; set; }
    public bool DisplayEmailPublicly { get; set; } = false;
    public byte _integrityVersion { get; set; } = 0;
    [NotMapped]
    public int IntegrityVersion
    {
        get => _integrityVersion;
        set => _integrityVersion = value > 255 || value < 0 ? (byte)0 : (byte)value;
    }
    //public bool AllowToLogin { get; set; } = true; // add to admin db
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool HasImage { get; set; } = false;
}

public class Identity_RoleDbModel : IdentityRole<int>
{
    public Identity_RoleDbModel() : base() { }
    public Identity_RoleDbModel(string roleName) : base(roleName) { }
    public string Description { get; set; } = string.Empty;
}

//*********************************** data models ************************************
public class Identity_UserProfileModel
{
    public string Guid { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? Description { get; set; }
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public string? Email { get; set; }
    public bool? DisplayEmailPublicly { get; set; }
    public string[]? Roles { get; set; } = [];
}

public class Identity_LoginFormModel
{
    public string? ReturnUrl { get; set; } = string.Empty;

    [StringLength(60)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [StringLength(60)]
    public string Password { get; set; } = string.Empty;

    [StringLength(2048)]
    public string CfTurnstileResponse { get; set; } = string.Empty;
}

public class Identity_EmailValidationFormModel
{

    [StringLength(60)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(2048)]
    public string CfTurnstileResponse { get; set; } = string.Empty;
}

public class Identity_SignupFormModel
{
    [StringLength(60, MinimumLength = 8)]
    public string Username { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(2048)]
    public string CfTurnstileResponse { get; set; } = string.Empty;
}

public class Identity_ChangePasswordFormModel
{
    [StringLength(60, MinimumLength = 8)]
    public string CurrentPassword { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    [Compare(nameof(NewPassword))]
    public string RepeatNewPassword { get; set; } = string.Empty;
}

public class Identity_ResetPasswordFormModel
{
    [StringLength(32)]
    public string UserGuid { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    [Compare(nameof(NewPassword))]
    public string RepeatNewPassword { get; set; } = string.Empty;
}

public class UsersListFilterModel
{
    [StringLength(60)]
    public string? UserName { get; set; } = null;

    [StringLength(60)]
    public string? Email { get; set; } = null;

    public bool? EmailConfirmed { get; set; } = null;

    public bool? DisplayEmailPublicly { get; set; } = null;

    [StringLength(60)]
    public string? CreatedFrom { get; set; } = null;

    [StringLength(60)]
    public string? CreatedTo { get; set; } = null;

    public int? Page { get; set; } = 0;

    public int? PageSize { get; set; } = 50;

    [StringLength(48)]
    public string? SortProperty { get; set; } = null;

    [StringLength(4)]
    public string? SortDirection { get; set; } = null;
}

public class UsersListModel
{
    //public string? ImageAddress { get; set; } = null;
    public bool HasImage { get; set; }
    public int IntegrityVersion { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string UserGuid { get; set; } = null!;
    public bool DisplayEmailPublicly { get; set; }
    public DateTime CreatedAt { get; set; }
}



/******************************** Custom Token Provider *******************************/
public class CustomTokenProvider : DataProtectorTokenProvider<Identity_UserDbModel>
{
    readonly IConfiguration _configuration;
    public CustomTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DataProtectionTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<Identity_UserDbModel>> logger,
        IConfiguration configuration)
    : base(dataProtectionProvider, options, logger)
    {
        _configuration = configuration;
    }

    public override async Task<string> GenerateAsync(string purpose,
    UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserGuid),
            new Claim("AspNet.Identity.SecurityStamp", await userManager.GetSecurityStampAsync(user))
        };

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(jwtSettings["DurationInHours"] ?? "10")),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public override async Task<bool> ValidateAsync(string purpose, string token,
    UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal;
        SecurityToken validatedToken;

        var validationResult = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
        if (!validationResult.IsValid)
        {
            return false;
        }

        try
        {
            // Validate the token and return the claims principal
            principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return false;
        }

        // Ensure the token is a valid JWT
        if (validatedToken is JwtSecurityToken jwtToken &&
            jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase) &&
            principal is not null)
        {
            string? userGuid = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userGuid is null || user.UserGuid != userGuid)
            {
                return false;
            }

            string? securityStamp = principal?.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
            if (securityStamp is null || user.SecurityStamp != securityStamp)
            {
                return false;
            }

            return true;
        }

        return false;
    }


}


/******************************** Identity Process *******************************/
public class Identity_Process
{
    public readonly DirectoryInfo Storage_Users;
    readonly string SeedFileName = "data.json";
    public Identity_Process(IWebHostEnvironment _env)
    {
        Storage_Users = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Identity", "Users"));
    }

    //************************************ seed User data **********************************
    public async Task Update_UserSeed(Identity_UserDbModel user,
    UserManager<Identity_UserDbModel> userManager)
    {
        Identity_UserSeedModel? seedModel = await Identity_UserSeedModel.Factory(user, userManager);
        if (seedModel is null) return;

        string json = JsonSerializer.Serialize(seedModel);
        DirectoryInfo seedDirectory = Directory.CreateDirectory(Path.Combine(Storage_Users.FullName, user.UserGuid));
        string seedPath = Path.Combine(seedDirectory.FullName, SeedFileName);
        await File.WriteAllTextAsync(seedPath, json);
    }
    public void Delete_UserSeed(string userGuid)
    {
        string seedPath = Path.Combine(Storage_Users.FullName, userGuid, SeedFileName);
        if (File.Exists(seedPath))
        {
            try
            {
                File.Delete(seedPath);
            }
            catch (Exception e)
            {
                //log
                Console.WriteLine($"\n     ***** {e.Message} *****");
            }
        }
    }
    public async Task Seed_UsersToDb(UserManager<Identity_UserDbModel> userManager)
    {
        foreach (var seedDirectory in Storage_Users.EnumerateDirectories())
        {
            var dbModelExist = await userManager.Users
            .AnyAsync(o => o.UserGuid == seedDirectory.Name);
            if (dbModelExist)
            {
                continue;
            }

            string seedPath = Path.Combine(Storage_Users.FullName, seedDirectory.Name, SeedFileName);
            if (!File.Exists(seedPath))
            {
                continue;
            }

            string json = await File.ReadAllTextAsync(seedPath);
            Identity_UserSeedModel? seedModel;
            try
            {
                seedModel = JsonSerializer.Deserialize<Identity_UserSeedModel>(json);
            }
            catch
            {
                //log
                Console.WriteLine($"\n     ***** an exception occured during deserializing User seed data! guid: '{seedDirectory.Name}'");
                continue;
            }
            if (seedModel is not null)
            {
                Identity_UserDbModel userDbModel = seedModel.GetDbModel();
                IdentityResult result = await userManager.CreateAsync(userDbModel);
                if (!result.Succeeded)
                {
                    //log
                    Console.WriteLine($"\n     ***** Couldn't seed a user to the db! userGuid '{userDbModel.UserGuid}'");
                    continue;
                }

                foreach (string roleName in seedModel.Roles)
                {
                    await userManager.AddToRoleAsync(userDbModel, roleName);
                }
            }
        }
    }


}

//************************* Seed Models *************************
public class Identity_UserSeedModel
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string UserGuid { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool DisplayEmailPublicly { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasImage { get; set; }
    public string[] Roles { get; set; } = [];

    public static async Task<Identity_UserSeedModel?> Factory(Identity_UserDbModel user,
    UserManager<Identity_UserDbModel> userManager)
    {
        string[] roles = (await userManager.GetRolesAsync(user)).ToArray();

        Identity_UserSeedModel? seedModel = new()
        {
            CreatedAt = user.CreatedAt,
            Description = user.Description,
            DisplayEmailPublicly = user.DisplayEmailPublicly,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            HasImage = user.HasImage,
            PasswordHash = user.PasswordHash!,
            UserGuid = user.UserGuid,
            UserName = user.UserName!,
            Roles = roles,
        };

        return seedModel;
    }

    public Identity_UserDbModel GetDbModel()
    {
        Identity_UserDbModel userDbModel = new()
        {
            CreatedAt = CreatedAt,
            Description = Description,
            DisplayEmailPublicly = DisplayEmailPublicly,
            Email = Email,
            EmailConfirmed = EmailConfirmed,
            HasImage = HasImage,
            PasswordHash = PasswordHash,
            UserGuid = UserGuid,
            UserName = UserName,
        };

        return userDbModel;
    }
}



