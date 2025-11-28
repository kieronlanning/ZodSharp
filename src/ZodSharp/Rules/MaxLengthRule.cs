namespace ZodSharp.Rules;

/// <summary>
/// Validation rule for maximum string length.
/// Uses struct to avoid allocations.
/// </summary>
public readonly struct MaxLengthRule : Core.IValidationRule<string>
{
    private readonly int _maxLength;

    public MaxLengthRule(int maxLength)
    {
        _maxLength = maxLength;
    }

    public bool IsValid(in string value)
    {
        return value.Length <= _maxLength;
    }

    public string GetErrorMessage(in string value)
    {
        return $"String must be at most {_maxLength} characters long, but got {value.Length}";
    }
}

