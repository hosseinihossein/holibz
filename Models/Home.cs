using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace holibz.Models;
public class Home_ContactUsModel
{
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [FromForm(Name = "cf-turnstile-response")]
    public string CfTurnstileResponse { get; set; } = string.Empty;
}