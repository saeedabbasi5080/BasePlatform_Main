using BasePlatform.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BasePlatform.Api.Common;

/// <summary>
/// Base controller that maps a failed <see cref="Result"/> to an HTTP response using the
/// uniform <see cref="ApiErrorResponse"/> envelope. Inherit this instead of duplicating
/// per-controller error mapping.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Problem(Result result) => MapError(result.Error);

    protected IActionResult Problem<T>(Result<T> result) => MapError(result.Error);

    private IActionResult MapError(Error error)
    {
        var body = new ApiErrorResponse(error.Description, error.Code, error.ValidationErrors);

        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return StatusCode(statusCode, body);
    }
}
