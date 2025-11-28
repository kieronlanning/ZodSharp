using System.Text.RegularExpressions;

namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for email format.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct EmailRule : Core.IValidationRule<string>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    public bool IsValid(in string value)
    {
        return EmailRegex.IsMatch(value);
    }

    public string GetErrorMessage(in string value)
    {
        return $"Invalid email format: {value}";
    }
}

