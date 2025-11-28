namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for finite number check.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct FiniteRule : Core.IValidationRule<double>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
    {
        _message = message;
    }

    public bool IsValid(in double value)
    {
        return double.IsFinite(value);
    }

    public string GetErrorMessage(in double value)
    {
        return _message ?? $"Number must be finite, but got {value}";
    }
}

