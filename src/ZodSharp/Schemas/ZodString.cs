using ZodSharp.Core;
using ZodSharp.Rules;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for string validation.
/// Provides fluent API for common string validations.
/// </summary>
public class ZodString : ZodType<string>
{
    protected override ValidationResult<string> ParseInternal(string value)
    {
        if (value == null)
        {
            return ValidationResult<string>.Failure(new ValidationError(
                "invalid_type",
                "Expected string, but got null",
                Array.Empty<string>()
            ));
        }

        return ValidationResult<string>.Success(value);
    }

    /// <summary>
    /// Adds a minimum length validation.
    /// Equivalent to Zod's .min() method.
    /// </summary>
    public ZodString Min(int minLength)
    {
        AddRule(new MinLengthRule(minLength));
        return this;
    }

    /// <summary>
    /// Adds a maximum length validation.
    /// Equivalent to Zod's .max() method.
    /// </summary>
    public ZodString Max(int maxLength)
    {
        AddRule(new MaxLengthRule(maxLength));
        return this;
    }

    /// <summary>
    /// Adds an email format validation.
    /// Equivalent to Zod's .email() method.
    /// </summary>
    public ZodString Email()
    {
        AddRule(new EmailRule());
        return this;
    }

    /// <summary>
    /// Adds a regex pattern validation.
    /// Equivalent to Zod's .regex() method.
    /// </summary>
    public ZodString Regex(System.Text.RegularExpressions.Regex pattern, string? message = null)
    {
        AddRule(new RegexRule(pattern, message));
        return this;
    }

    /// <summary>
    /// Adds a regex pattern validation from a string.
    /// Equivalent to Zod's .regex() method.
    /// </summary>
    public ZodString Regex(string pattern, string? message = null)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            pattern,
            System.Text.RegularExpressions.RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100)
        );
        return Regex(regex, message);
    }
}

