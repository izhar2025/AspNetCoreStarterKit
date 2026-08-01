using System.Text.Json;
using AspNetCoreStarterKit.Application.Common.Exceptions;
using AspNetCoreStarterKit.Application.Common.Models;

namespace AspNetCoreStarterKit.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = exception switch
        {
            ValidationException validationEx => new
            {
                StatusCode = 400,
                Response = ApiResponse<object>.Fail(validationEx.Message, 400, validationEx.Errors)
            },
            NotFoundException notFoundEx => new
            {
                StatusCode = 404,
                Response = ApiResponse<object>.NotFound(notFoundEx.Message)
            },
            UnauthorizedAccessException => new
            {
                StatusCode = 401,
                Response = ApiResponse<object>.Unauthorized("Unauthorized access")
            },
            _ => new
            {
                StatusCode = 500,
                Response = ApiResponse<object>.Fail("An internal error occurred. Please try again later.", 500)
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Response, jsonOptions));
    }
}