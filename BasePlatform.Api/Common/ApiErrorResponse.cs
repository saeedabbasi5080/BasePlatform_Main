namespace BasePlatform.Api.Common;

/// <summary>
/// Uniform error envelope returned by every API endpoint:
/// { "message": "...", "code": "...", "errors": { "field": ["msg"] } }.
/// </summary>
public sealed record ApiErrorResponse(
    string Message,
    string Code,
    IReadOnlyDictionary<string, string[]>? Errors = null);
