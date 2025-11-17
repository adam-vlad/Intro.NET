using System.ComponentModel.DataAnnotations;

namespace Orders.Application.Validation.CustomAttributes;

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        _min = (decimal)min;
        _max = (decimal)max;
        ErrorMessage = $"price must be between {_min:C2} and {_max:C2}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult(ErrorMessage);

        if (value is decimal price)
        {
            if (price >= _min && price <= _max)
                return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
}