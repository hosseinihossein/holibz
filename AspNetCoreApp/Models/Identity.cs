using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

    [StringLength(50, MinimumLength = 8)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public bool IsPersistent { get; set; } = false;
}

public class Identity_SignupModel
{
    [StringLength(50, MinimumLength = 8)]
    public string Username { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 8)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password))]
    public string RepeatPassword { get; set; } = string.Empty;
}

/*********************************** IdentityDb ************************************/
public class IdentityDb : IdentityDbContext<Identity_UserDbModel, Identity_RoleDbModel, int>
{
    public IdentityDb(DbContextOptions<IdentityDb> options) : base(options) { }
}

/******************************** EmailTokenProvider *******************************/
public class Identity_EmailTokenProvider : AuthenticatorTokenProvider<Identity_UserDbModel>
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
}

