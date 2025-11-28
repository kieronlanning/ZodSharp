using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema that provides a default value when input is null or missing.
/// Equivalent to Zod's default method.
/// </summary>
/// <typeparam name="T">The type</typeparam>
public class ZodDefault<T> : ZodType<T, T?>
{
    private readonly IZodSchema<T> _innerSchema;
    private readonly T _defaultValue;

    public ZodDefault(IZodSchema<T> innerSchema, T defaultValue)
    {
        _innerSchema = innerSchema;
        _defaultValue = defaultValue;
    }

    protected override ValidationResult<T> ParseInternal(T? value)
    {
        if (value == null)
        {
            return ValidationResult<T>.Success(_defaultValue);
        }

        return _innerSchema.Validate(value);
    }
}

