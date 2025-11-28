using System.Text.RegularExpressions;
using ZodSharp.Core;
using ZodSharp.Rules;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for string validation.
/// Provides fluent API for common string validations.
/// </summary>
public class ZodString : ZodType<string>
{
    private static readonly string[] EmptyPath = Array.Empty<string>();

    protected override ValidationResult<string> ParseInternal(string value)
    {
        if (value == null)
        {
            return ValidationResult<string>.Failure(new ValidationError(
                "invalid_type",
                "Expected string, but got null",
                EmptyPath
            ));
        }

        return ValidationResult<string>.Success(value);
    }

    public ValidationResult<string> ValidateSpan(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty && value.Length == 0)
        {
            return ValidationResult<string>.Success(string.Empty);
        }

        var str = value.ToString();
        return Validate(str);
    }

    public ZodString Min(int minLength)
    {
        AddRule(new MinLengthRule(minLength));
        return this;
    }

    public ZodString Max(int maxLength)
    {
        AddRule(new MaxLengthRule(maxLength));
        return this;
    }

    public ZodString Email()
    {
        AddRule(new EmailRule());
        return this;
    }

    public ZodString Regex(System.Text.RegularExpressions.Regex pattern, string? message = null)
    {
        AddRule(new RegexRule(pattern, message));
        return this;
    }

    public ZodString Regex(string pattern, string? message = null)
    {
        var regex = new Regex(
            pattern,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100)
        );
        return Regex(regex, message);
    }

    public ZodString Length(int length)
    {
        AddRule(new MinLengthRule(length));
        AddRule(new MaxLengthRule(length));
        return this;
    }

    public ZodString Url(string? message = null)
    {
        AddRule(new UrlRule(message));
        return this;
    }

    public ZodString Uuid(string? message = null)
    {
        AddRule(new UuidRule(message));
        return this;
    }

    public ZodString StartsWith(string prefix, string? message = null)
    {
        AddRule(new StartsWithRule(prefix, message));
        return this;
    }

    public ZodString EndsWith(string suffix, string? message = null)
    {
        AddRule(new EndsWithRule(suffix, message));
        return this;
    }

    public ZodString ToLower()
    {
        return Transform(s => s.ToLowerInvariant());
    }

    public ZodString ToUpper()
    {
        return Transform(s => s.ToUpperInvariant());
    }

    public ZodString Trim()
    {
        return Transform(s => s.Trim());
    }
}

