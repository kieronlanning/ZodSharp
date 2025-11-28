using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for boolean validation.
/// </summary>
public class ZodBoolean : ZodType<bool>
{
    protected override ValidationResult<bool> ParseInternal(bool value)
    {
        return ValidationResult<bool>.Success(value);
    }
}

