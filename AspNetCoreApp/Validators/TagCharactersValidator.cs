using System.ComponentModel.DataAnnotations;

namespace AspNetCoreApp.Validators;

public class TagCharactersValidator : ValidationAttribute
{
    private readonly string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

    /*public TagCharactersValidator(string _allowedCharacters)
    {
        allowedCharacters = _allowedCharacters;
    }*/

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null)
        {
            if (value is string[] tags)
            {
                foreach (string tag in tags)
                {
                    if (tag.Length < 3)
                    {
                        return new ValidationResult("tags cannot be less than 3 characters long!");
                    }
                    foreach (char c in tag)
                    {
                        if (!allowedCharacters.Contains(c))
                        {
                            return new ValidationResult($"tags cannot contain character '{c}'!");
                        }
                    }
                }
            }
            else if (value is string tag)
            {
                if (tag.Length < 3)
                {
                    return new ValidationResult("the tag cannot be less than 3 characters long!");
                }
                foreach (char c in tag)
                {
                    if (!allowedCharacters.Contains(c))
                    {
                        return new ValidationResult($"tags cannot contain character '{c}'!");
                    }
                }
            }
            else
            {
                return new ValidationResult("value is not a string or a string array!");
            }
        }
        return ValidationResult.Success;
    }
}