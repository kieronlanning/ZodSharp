using ZodSharp.Core;
using ZodSharp.Rules;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for number validation.
/// Provides fluent API for common number validations.
/// </summary>
public class ZodNumber : ZodType<double>
{
    private static readonly string[] EmptyPath = Array.Empty<string>();

    protected override ValidationResult<double> ParseInternal(double value)
    {
        if (double.IsNaN(value))
        {
            return ValidationResult<double>.Failure(new ValidationError(
                "invalid_type",
                "Expected number, but got NaN",
                EmptyPath
            ));
        }

        return ValidationResult<double>.Success(value);
    }

    public ZodNumber Min(double minValue)
    {
        AddRule(new MinValueRule<double>(minValue));
        return this;
    }

    public ZodNumber Max(double maxValue)
    {
        AddRule(new MaxValueRule<double>(maxValue));
        return this;
    }

    public ZodNumber Int()
    {
        AddRule(new IntRule());
        return this;
    }

    public ZodNumber Positive()
    {
        AddRule(new MinValueRule<double>(0.0));
        return this;
    }

    public ZodNumber Negative()
    {
        AddRule(new MaxValueRule<double>(0.0));
        return this;
    }

    public ZodNumber MultipleOf(double divisor, string? message = null)
    {
        AddRule(new MultipleOfRule(divisor, message));
        return this;
    }

    public ZodNumber Finite(string? message = null)
    {
        AddRule(new FiniteRule(message));
        return this;
    }

    public ZodNumber Safe(string? message = null)
    {
        AddRule(new SafeIntegerRule(message));
        return this;
    }
}

