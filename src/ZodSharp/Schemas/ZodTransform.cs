using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema that transforms a value during validation.
/// Equivalent to Zod's transform method.
/// </summary>
/// <typeparam name="TInput">The input type (after validation by inner schema)</typeparam>
/// <typeparam name="TOutput">The output type after transformation</typeparam>
public class ZodTransform<TInput, TOutput> : ZodType<TOutput, TInput>
{
    private readonly IZodSchema<TInput, TInput> _inputSchema;
    private readonly Func<TInput, TOutput> _transform;

    public ZodTransform(IZodSchema<TInput, TInput> inputSchema, Func<TInput, TOutput> transform)
    {
        _inputSchema = inputSchema;
        _transform = transform;
    }

    protected override ValidationResult<TOutput> ParseInternal(TInput value)
    {
        var validationResult = _inputSchema.Validate(value);
        if (!validationResult.IsSuccess)
        {
            return ValidationResult<TOutput>.Failure(validationResult.Errors);
        }

        try
        {
            var transformedValue = _transform(validationResult.Value!);
            return ValidationResult<TOutput>.Success(transformedValue);
        }
        catch (Exception ex)
        {
            return ValidationResult<TOutput>.Failure(new ValidationError(
                "transform_error",
                $"Transform failed: {ex.Message}",
                Array.Empty<string>()
            ));
        }
    }
}

