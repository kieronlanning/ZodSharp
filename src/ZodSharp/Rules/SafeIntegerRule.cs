namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for safe integer check.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct SafeIntegerRule : Core.IValidationRule<double>
{
    private readonly string? _message;

    public SafeIntegerRule(string? message = null)
    {
        _message = message;
    }

    public bool IsValid(in double value)
    {
        return value == Math.Truncate(value) && 
               value >= int.MinValue && 
               value <= int.MaxValue;
    }

    public string GetErrorMessage(in double value)
    {
        return _message ?? $"Number must be a safe integer, but got {value}";
    }
}

