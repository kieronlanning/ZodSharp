namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for maximum numeric value.
/// Uses struct to avoid allocations.
/// </summary>
/// <typeparam name="T">The numeric type</typeparam>
public readonly struct MaxValueRule<T> : Core.IValidationRule<T> where T : IComparable<T>
{
    private readonly T _maxValue;

    public MaxValueRule(T maxValue)
    {
        _maxValue = maxValue;
    }

    public bool IsValid(in T value)
    {
        return value.CompareTo(_maxValue) <= 0;
    }

    public string GetErrorMessage(in T value)
    {
        return $"Value must be at most {_maxValue}, but got {value}";
    }
}

