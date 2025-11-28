namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for multiple-of check.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct MultipleOfRule : Core.IValidationRule<double>
{
    private readonly double _divisor;
    private readonly string? _message;

    public MultipleOfRule(double divisor, string? message = null)
    {
        if (divisor == 0)
            throw new ArgumentException("Divisor cannot be zero", nameof(divisor));
        _divisor = divisor;
        _message = message;
    }

    public bool IsValid(in double value)
    {
        var remainder = Math.Abs(value % _divisor);
        return remainder < double.Epsilon || Math.Abs(remainder - _divisor) < double.Epsilon;
    }

    public string GetErrorMessage(in double value)
    {
        return _message ?? $"Number must be a multiple of {_divisor}, but got {value}";
    }
}

