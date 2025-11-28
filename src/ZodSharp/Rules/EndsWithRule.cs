namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for string suffix.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct EndsWithRule : Core.IValidationRule<string>
{
    private readonly string _suffix;
    private readonly string? _message;

    public EndsWithRule(string suffix, string? message = null)
    {
        _suffix = suffix ?? throw new ArgumentNullException(nameof(suffix));
        _message = message;
    }

    public bool IsValid(in string value)
    {
        if (value == null)
            return false;

        return value.EndsWith(_suffix, StringComparison.Ordinal);
    }

    public string GetErrorMessage(in string value)
    {
        return _message ?? $"String must end with '{_suffix}', but got '{value}'";
    }
}

