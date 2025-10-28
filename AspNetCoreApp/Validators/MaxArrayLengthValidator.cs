using System.ComponentModel.DataAnnotations;

namespace AspNetCoreApp.Validators;

public class MaxArrayLengthAttribute : ValidationAttribute
{
    private readonly int maxLength;
    public MaxArrayLengthAttribute(int _maxLength)
    {
        maxLength = _maxLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null)
        {
            if (value.GetType().IsArray)
            {
                object[] array = (object[])value;
                if (array.Length > maxLength)
                {
                    return new ValidationResult($"The array cannot contain more than {maxLength} items!");
                }
                return ValidationResult.Success;
            }
            return new ValidationResult("The value is not an array!");
        }
        return ValidationResult.Success;
    }

    //public override 
}