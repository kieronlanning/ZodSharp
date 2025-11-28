namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for integer values.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct IntRule : Core.IValidationRule<double>
{
    public bool IsValid(in double value)
    {
        return value == Math.Truncate(value);
    }

    public string GetErrorMessage(in double value)
    {
        return $"Expected integer, but got {value}";
    }
}

