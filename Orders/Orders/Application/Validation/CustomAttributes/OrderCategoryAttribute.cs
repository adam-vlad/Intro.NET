using System.ComponentModel.DataAnnotations;
using Orders.Domain.Enums;

namespace Orders.Application.Validation.CustomAttributes;

public class OrderCategoryAttribute : ValidationAttribute
{
    private readonly OrderCategory[] _allowedCategories;

    public OrderCategoryAttribute(params OrderCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
        var categories = string.Join(", ", _allowedCategories.Select(c => c.ToString().ToLower()));
        ErrorMessage = $"category must be one of: {categories}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult(ErrorMessage);

        if (value is OrderCategory category)
        {
            if (_allowedCategories.Contains(category))
                return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
}