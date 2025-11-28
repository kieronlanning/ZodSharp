using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema wrapper that makes a value optional.
/// </summary>
/// <typeparam name="T">The inner schema type</typeparam>
public class ZodOptional<T> : ZodType<T?, T?> where T : class
{
    private readonly IZodSchema<T, T> _innerSchema;

    public ZodOptional(IZodSchema<T, T> innerSchema)
    {
        _innerSchema = innerSchema;
    }

    protected override ValidationResult<T?> ParseInternal(T? value)
    {
        if (value == null)
        {
            return ValidationResult<T?>.Success(null);
        }

        var result = _innerSchema.Validate(value);
        if (!result.IsSuccess)
        {
            return ValidationResult<T?>.Failure(result.Errors);
        }

        return ValidationResult<T?>.Success(result.Value);
    }
}

