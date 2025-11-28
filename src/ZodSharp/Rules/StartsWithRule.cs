namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for string prefix.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct StartsWithRule : Core.IValidationRule<string>
{
    private readonly string _prefix;
    private readonly string? _message;

    public StartsWithRule(string prefix, string? message = null)
    {
        _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        _message = message;
    }

    public bool IsValid(in string value)
    {
        if (value == null)
            return false;

        return value.StartsWith(_prefix, StringComparison.Ordinal);
    }

    public string GetErrorMessage(in string value)
    {
        return _message ?? $"String must start with '{_prefix}', but got '{value}'";
    }
}

