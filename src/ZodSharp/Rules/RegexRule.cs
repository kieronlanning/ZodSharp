using System.Text.RegularExpressions;

namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for regex pattern matching.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct RegexRule : Core.IValidationRule<string>
{
    private readonly Regex _pattern;
    private readonly string? _message;

    public RegexRule(Regex pattern, string? message = null)
    {
        _pattern = pattern;
        _message = message;
    }

    public bool IsValid(in string value)
    {
        return _pattern.IsMatch(value);
    }

    public string GetErrorMessage(in string value)
    {
        return _message ?? $"String does not match the required pattern: {_pattern}";
    }
}

