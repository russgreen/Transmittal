using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Transmittal.Library.Validation;

public class MustBeFalseAttribute : ValidationAttribute
{
    private const string _defaultErrorMessage = "The value must be false";

    public MustBeFalseAttribute()
    {

    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var boolValue = value as bool?;

        if (boolValue != null && boolValue == false)
        {
            return ValidationResult.Success;
        }

        return new(ErrorMessage ?? _defaultErrorMessage);
    }


}
