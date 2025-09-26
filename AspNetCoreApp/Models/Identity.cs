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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreApp.Models;

public class Identity_UserDbModel : IdentityUser<int>
{
    public string UserGuid { get; set; } = string.Empty;
    public string PasswordLiteral { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EmailValidationCode { get; set; }
    public DateTime? EmailValidationDate { get; set; }//rename to evcDate
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

public class Identity_LoginModel
{
    public string? ReturnUrl { get; set; } = string.Empty;

    //[StringLength(50, MinimumLength = 8)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    //[StringLength(50, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    //public bool IsPersistent { get; set; } = false;
}

public class Identity_SignupModel
{
    [StringLength(60, MinimumLength = 8)]
    public string Username { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password))]
    public string RepeatPassword { get; set; } = string.Empty;
}

/*********************************** IdentityDb ************************************/
public class Identity_DbContext : IdentityDbContext<Identity_UserDbModel, Identity_RoleDbModel, int>
{
    public Identity_DbContext(DbContextOptions<Identity_DbContext> options) : base(options) { }
}

/******************************** EmailTokenProvider *******************************/
/*public class Identity_EmailTokenProvider : AuthenticatorTokenProvider<Identity_UserDbModel>
{
    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
    {
        return base.CanGenerateTwoFactorTokenAsync(userManager, user);
    }
    public override async Task<string> GenerateAsync(string purpose, UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
    {
        string code = new Random().Next(10_000, 99_999).ToString();
        user.EmailValidationCode = code;
        user.EmailValidationDate = DateTime.Now;
        IdentityResult result = await userManager.UpdateAsync(user);
        string token = result.Succeeded ? code : string.Empty;
        return token;
    }
    public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
    {
        bool isValid = false;
        if (token == user.EmailValidationCode)
        {
            isValid = true;
            user.EmailValidationCode = null;
            user.EmailValidationDate = null;
            await userManager.UpdateAsync(user);
        }
        return isValid;
    }
}*/

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

    public override async Task<string> GenerateAsync(string purpose, UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
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

    public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<Identity_UserDbModel> userManager, Identity_UserDbModel user)
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