namespace ZodSharp.Core;

/// <summary>
/// Represents a validation error.
/// Uses struct to minimize allocations.
/// </summary>
public readonly struct ValidationError
{
    /// <summary>
    /// The error code (e.g., "invalid_type", "too_small", "too_big")
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The error message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The path to the field that failed validation (e.g., ["user", "email"])
    /// </summary>
    public string[] Path { get; }

    /// <summary>
    /// Additional error parameters
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Parameters { get; }

    public ValidationError(string code, string message, string[]? path = null, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        Code = code;
        Message = message;
        Path = path ?? Array.Empty<string>();
        Parameters = parameters;
    }
}

