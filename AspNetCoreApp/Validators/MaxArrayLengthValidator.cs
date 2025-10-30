using System.ComponentModel.DataAnnotations;

namespace AspNetCoreApp.Validators;

public class MaxStringArrayLengthAttribute : ValidationAttribute
{
    private readonly int maxArrayLength;
    private readonly int maxStringLength;
    public MaxStringArrayLengthAttribute(int _maxArrayLength, int _maxStringLength)
    {
        maxArrayLength = _maxArrayLength;
        maxStringLength = _maxStringLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null)
        {
            if (value.GetType().IsArray)
            {
                string[] stringArray = (string[])value;
                if (stringArray.Length > maxArrayLength)
                {
                    return new ValidationResult($"The array cannot contain more than {maxArrayLength} items!");
                }
                if (stringArray.Any(s => s.Length > maxStringLength))
                {
                    return new ValidationResult($"Each string elements of the array cannot be more than {maxStringLength} characters long!");
                }
                return ValidationResult.Success;
            }
            return new ValidationResult("The value is not an array!");
        }
        return ValidationResult.Success;
    }

    //public override 
}