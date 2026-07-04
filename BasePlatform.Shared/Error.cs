namespace BasePlatform.Shared;

public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Optional per-field validation messages (field name → messages).
    /// Surfaced to clients in the "errors" object of the error envelope.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Validation(
        string code,
        string description,
        IReadOnlyDictionary<string, string[]> validationErrors) =>
        new(code, description, ErrorType.Validation) { ValidationErrors = validationErrors };

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);
}