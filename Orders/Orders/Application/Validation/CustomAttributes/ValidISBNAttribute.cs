using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Orders.Application.Validation.CustomAttributes;

public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
{
    public ValidISBNAttribute()
    {
        ErrorMessage = "isbn must be valid (10 or 13 digits)";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        var isbn = value.ToString()!.Replace("-", "").Replace(" ", "");

        if ((isbn.Length == 10 || isbn.Length == 13) && isbn.All(char.IsDigit))
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-isbn", ErrorMessage ?? "isbn must be valid");
    }
}