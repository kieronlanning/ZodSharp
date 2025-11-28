namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for minimum string length.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct MinLengthRule : Core.IValidationRule<string>
{
    private readonly int _minLength;

    public MinLengthRule(int minLength)
    {
        _minLength = minLength;
    }

    public bool IsValid(in string value)
    {
        return value.Length >= _minLength;
    }

    public string GetErrorMessage(in string value)
    {
        return $"String must be at least {_minLength} characters long, but got {value.Length}";
    }
}

