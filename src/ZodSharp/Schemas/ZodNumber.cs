using ZodSharp.Core;
using ZodSharp.Rules;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for number validation.
/// Provides fluent API for common number validations.
/// </summary>
public class ZodNumber : ZodType<double>
{
    protected override ValidationResult<double> ParseInternal(double value)
    {
        if (double.IsNaN(value))
        {
            return ValidationResult<double>.Failure(new ValidationError(
                "invalid_type",
                "Expected number, but got NaN",
                Array.Empty<string>()
            ));
        }

        return ValidationResult<double>.Success(value);
    }

    /// <summary>
    /// Adds a minimum value validation.
    /// </summary>
    public ZodNumber Min(double minValue)
    {
        AddRule(new MinValueRule<double>(minValue));
        return this;
    }

    /// <summary>
    /// Adds a maximum value validation.
    /// </summary>
    public ZodNumber Max(double maxValue)
    {
        AddRule(new MaxValueRule<double>(maxValue));
        return this;
    }

    /// <summary>
    /// Adds an integer validation (must be a whole number).
    /// </summary>
    public ZodNumber Int()
    {
        AddRule(new IntRule());
        return this;
    }

    /// <summary>
    /// Adds a positive number validation.
    /// </summary>
    public ZodNumber Positive()
    {
        AddRule(new MinValueRule<double>(0.0));
        return this;
    }

    /// <summary>
    /// Adds a negative number validation.
    /// </summary>
    public ZodNumber Negative()
    {
        AddRule(new MaxValueRule<double>(0.0));
        return this;
    }
}

