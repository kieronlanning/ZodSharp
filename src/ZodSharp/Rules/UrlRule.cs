using System.Text.RegularExpressions;

namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for URL format.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct UrlRule : Core.IValidationRule<string>
{
    private static readonly Regex UrlRegex = new(
        @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    private readonly string? _message;

    public UrlRule(string? message = null)
    {
        _message = message;
    }

    public bool IsValid(in string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return UrlRegex.IsMatch(value) || Uri.TryCreate(value, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public string GetErrorMessage(in string value)
    {
        return _message ?? $"Invalid URL format: {value}";
    }
}

