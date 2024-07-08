using System.Net;
using System.Text.Json;
using MovieCategories.Domain;

namespace MovieCategories.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: ");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        var details = _env.IsDevelopment() ? exception.Message : string.Empty;

        if (exception is CustomException customException)
        {
            context.Response.StatusCode = customException.Code ?? context.Response.StatusCode;
            message = customException.Message;
            details = _env.IsDevelopment() ? customException.Details ?? string.Empty : string.Empty;
        }

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = message,
            Detailed = details
        };

        var jsonResponse = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(jsonResponse);
    }
}