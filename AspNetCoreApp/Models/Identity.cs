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

public class Identity_UserDbModel : IdentityUser<int>
{
    public string UserGuid { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool DisplayEmailPublicly { get; set; } = false;
    public bool ActivityAllowed { get; set; } = true;
    public byte _version { get; set; } = 0;
    [NotMapped]
    public int Version
    {
        get => _version;
        set => _version = value > 255 | value < 0 ? (byte)0 : (byte)value;
    }
}

public class Identity_RoleDbModel : IdentityRole<int>
{
    public Identity_RoleDbModel() : base() { }
    public Identity_RoleDbModel(string roleName) : base(roleName) { }
    public string Description { get; set; } = string.Empty;
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
    [StringLength(60, MinimumLength = 8)]
    public string CurrentPassword { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    [Compare(nameof(NewPassword))]
    public string RepeatNewPassword { get; set; } = string.Empty;
}




/*********************************** IdentityDb ************************************/
public class Identity_DbContext : IdentityDbContext<Identity_UserDbModel, Identity_RoleDbModel, int>
{
    public Identity_DbContext(DbContextOptions<Identity_DbContext> options) : base(options) { }
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


/******************************** Custom Token Provider *******************************/
public class Identity_Process
{
    readonly DirectoryInfo UserSeedDirectoryInfo;
    //readonly IWebHostEnvironment env;
    readonly UserManager<Identity_UserDbModel> userManager;
    public Identity_Process(IWebHostEnvironment _env, UserManager<Identity_UserDbModel> _userManager)
    {
        userManager = _userManager;
        UserSeedDirectoryInfo = Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Storage", "Identity", "UserSeedData"));
    }

    public async Task UpdateUserSeed(Identity_UserDbModel user)
    {
        Identity_UserSeedModel userSeedModel = new()
        {
            UserName = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            UserGuid = user.UserGuid,
            PasswordHash = user.PasswordHash!,
            Description = user.Description,
            DisplayEmailPublicly = user.DisplayEmailPublicly,
            ActivityAllowed = user.ActivityAllowed,
            Roles = [.. await userManager.GetRolesAsync(user)],
        };

        string json = JsonSerializer.Serialize(userSeedModel);
        string userSeedPath = Path.Combine(UserSeedDirectoryInfo.FullName, user.UserGuid);

        await File.WriteAllTextAsync(userSeedPath, json);
    }

    public void DeleteUserSeed(Identity_UserDbModel user)
    {
        string userSeedPath = Path.Combine(UserSeedDirectoryInfo.FullName, user.UserGuid);
        File.Delete(userSeedPath);
    }

    public async Task SeedUsersToDb()
    {
        foreach (var fileInfo in UserSeedDirectoryInfo.EnumerateFiles())
        {
            var myUser = await userManager.Users.FirstOrDefaultAsync(u => u.UserGuid == fileInfo.Name);
            if (myUser != null) continue;

            string json = await File.ReadAllTextAsync(fileInfo.FullName);
            Identity_UserSeedModel? userSeedModel;
            try
            {
                userSeedModel = JsonSerializer.Deserialize<Identity_UserSeedModel>(json);
            }
            catch
            {
                //log
                continue;
            }
            if (userSeedModel is not null)
            {
                myUser = new Identity_UserDbModel()
                {
                    UserName = userSeedModel.UserName,
                    Email = userSeedModel.Email,
                    EmailConfirmed = userSeedModel.EmailConfirmed,
                    UserGuid = userSeedModel.UserGuid,
                    PasswordHash = userSeedModel.PasswordHash,
                    Description = userSeedModel.Description,
                    DisplayEmailPublicly = userSeedModel.DisplayEmailPublicly,
                    ActivityAllowed = userSeedModel.ActivityAllowed,
                };
                IdentityResult result = await userManager.CreateAsync(myUser);
                if (!result.Succeeded)
                {
                    //log
                    continue;
                }

                foreach (string roleName in userSeedModel.Roles)
                {
                    await userManager.AddToRoleAsync(myUser, roleName);
                }
            }
        }
    }
}
public class Identity_UserSeedModel
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string UserGuid { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool DisplayEmailPublicly { get; set; } = false;
    public bool ActivityAllowed { get; set; } = true;
    public string[] Roles { get; set; } = [];
}



