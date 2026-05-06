using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Validation;
public static class ValidationHelpers
{
    public static ValidationResult ValidateCompanyName(string value, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult("A company name is required");
        }

        return ValidationResult.Success;
    }

}
