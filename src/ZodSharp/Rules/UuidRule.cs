using System.Text.RegularExpressions;

namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for UUID format.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct UuidRule : Core.IValidationRule<string>
{
    private static readonly Regex UuidRegex = new(
        @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    private readonly string? _message;

    public UuidRule(string? message = null)
    {
        _message = message;
    }

    public bool IsValid(in string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return UuidRegex.IsMatch(value);
    }

    public string GetErrorMessage(in string value)
    {
        return _message ?? $"Invalid UUID format: {value}";
    }
}

