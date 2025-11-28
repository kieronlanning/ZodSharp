using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema that adds custom validation logic (refinement).
/// Equivalent to Zod's refine method.
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public class ZodRefinement<T> : ZodType<T>
{
    private readonly IZodSchema<T> _baseSchema;
    private readonly Func<T, bool> _refinement;
    private readonly string? _message;

    public ZodRefinement(IZodSchema<T> baseSchema, Func<T, bool> refinement, string? message = null)
    {
        _baseSchema = baseSchema;
        _refinement = refinement;
        _message = message;
    }

    protected override ValidationResult<T> ParseInternal(T value)
    {
        var baseResult = _baseSchema.Validate(value);
        if (!baseResult.IsSuccess)
        {
            return baseResult;
        }

        if (!_refinement(baseResult.Value!))
        {
            return ValidationResult<T>.Failure(new ValidationError(
                "refinement_failed",
                _message ?? "Custom validation failed",
                Array.Empty<string>()
            ));
        }

        return ValidationResult<T>.Success(baseResult.Value!);
    }
}

