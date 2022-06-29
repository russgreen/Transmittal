using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Transmittal.Library.Validation;
public sealed class BeginsWithAttribute : ValidationAttribute
{
    public string PropertyName { get; }

    private const string _defaultErrorMessage = "The value does not begin with the expected value";

    public BeginsWithAttribute(string propertyName)
    {
        PropertyName = propertyName;

    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        object
            instance = validationContext.ObjectInstance,
            otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);

        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (((IComparable)value).ToString().StartsWith(otherValue.ToString()))
        {
            return ValidationResult.Success;
        }

        return new(ErrorMessage ?? _defaultErrorMessage);

    }

}
