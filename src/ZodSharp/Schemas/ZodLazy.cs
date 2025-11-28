using System.Collections.Concurrent;
using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema that lazily evaluates its inner schema.
/// Useful for recursive schemas and circular references.
/// Equivalent to Zod's lazy method.
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public class ZodLazy<T> : ZodType<T>
{
    private readonly Func<IZodSchema<T, T>> _schemaGetter;
    private IZodSchema<T, T>? _cachedSchema;
    private readonly object _lock = new();

    public ZodLazy(Func<IZodSchema<T, T>> schemaGetter)
    {
        _schemaGetter = schemaGetter;
    }

    /// <summary>
    /// Gets the inner schema, evaluating it lazily if needed.
    /// </summary>
    public IZodSchema<T, T> Schema
    {
        get
        {
            if (_cachedSchema == null)
            {
                lock (_lock)
                {
                    if (_cachedSchema == null)
                    {
                        _cachedSchema = _schemaGetter();
                    }
                }
            }
            return _cachedSchema;
        }
    }

    protected override ValidationResult<T> ParseInternal(T value)
    {
        return Schema.Validate(value);
    }
}

