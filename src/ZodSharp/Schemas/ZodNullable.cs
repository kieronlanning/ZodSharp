using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema wrapper that makes a value nullable.
/// </summary>
/// <typeparam name="T">The inner schema type</typeparam>
public class ZodNullable<T> : ZodType<T?, T?> where T : struct
{
    private readonly IZodSchema<T, T> _innerSchema;

    public ZodNullable(IZodSchema<T, T> innerSchema)
    {
        _innerSchema = innerSchema;
    }

    protected override ValidationResult<T?> ParseInternal(T? value)
    {
        if (value == null)
        {
            return ValidationResult<T?>.Success(null);
        }

        var result = _innerSchema.Validate(value.Value);
        if (!result.IsSuccess)
        {
            return ValidationResult<T?>.Failure(result.Errors);
        }

        return ValidationResult<T?>.Success(result.Value);
    }
}

