using MediatR;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreStarterKit.Application.Common.Models;

namespace AspNetCoreStarterKit.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected ActionResult HandleResult<T>(ApiResponse<T> response)
    {
        return response.StatusCode switch
        {
            200 => Ok(response),
            201 => StatusCode(201, response),
            400 => BadRequest(response),
            404 => NotFound(response),
            401 => Unauthorized(response),
            _ => StatusCode(response.StatusCode, response)
        };
    }

    protected ActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
        Response.Headers.Append("X-Current-Page", result.PageNumber.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());

        return Ok(ApiResponse<PagedResult<T>>.Ok(result));
    }
}