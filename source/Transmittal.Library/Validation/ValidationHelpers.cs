using System.ComponentModel.DataAnnotations;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Library.Validation;
public static class ValidationHelpers
{
    public static ValidationResult ValidateCompanyName(string value, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new ValidationResult("A company name is required");

        // You may need to get the service from the context or pass it in
        var instance = context.ObjectInstance;
        var service = (IContactDirectoryService?)context.GetService(typeof(IContactDirectoryService));

        if (service == null && instance is IHasContactDirectoryService hasService)
        {
            service = hasService.ContactDirectoryService;
        }            

        if (service != null)
        {
            var existingNames = service.GetCompanies_All()
                .Select(c => c.CompanyName?.Trim().ToLowerInvariant())
                .ToList();

            if (existingNames.Contains(value.Trim().ToLowerInvariant()))
                return new ValidationResult("This company name already exists.");
        }

        return ValidationResult.Success;
    }

}
