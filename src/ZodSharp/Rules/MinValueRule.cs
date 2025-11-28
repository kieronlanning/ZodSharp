namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for minimum numeric value.
/// Uses struct to avoid allocations.
/// </summary>
/// <typeparam name="T">The numeric type</typeparam>
public readonly struct MinValueRule<T> : Core.IValidationRule<T> where T : IComparable<T>
{
    private readonly T _minValue;

    public MinValueRule(T minValue)
    {
        _minValue = minValue;
    }

    public bool IsValid(in T value)
    {
        return value.CompareTo(_minValue) >= 0;
    }

    public string GetErrorMessage(in T value)
    {
        return $"Value must be at least {_minValue}, but got {value}";
    }
}

