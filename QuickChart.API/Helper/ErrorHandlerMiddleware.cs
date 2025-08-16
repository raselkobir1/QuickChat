using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QuickChart.API.Helper
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // Let the pipeline continue
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        //private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        //{
        //    var response = context.Response;

        //    if (response.HasStarted)
        //    {
        //        _logger.LogWarning("Response already started. Skipping error handling.");
        //        return;
        //    }

        //    response.ContentType = "application/json";
        //    response.StatusCode = (int)HttpStatusCode.InternalServerError;

        //    var errorResponse = new
        //    {
        //        statusCode = response.StatusCode,
        //        message = ex.Message ?? "An unexpected error occurred.",
        //        details = _env.IsDevelopment() ? ex.ToString() : null
        //    };

        //    _logger.LogError(ex, "Unhandled exception occurred.");



        //    var json = JsonConvert.SerializeObject(
        //        errorResponse,
        //        new JsonSerializerSettings
        //        {
        //            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        //            Formatting = Formatting.Indented
        //        });

        //    await response.WriteAsync(json);
        //}

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var response = CreateResponse(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response);

            LogException(exception, statusCode);
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ForbiddenAccessException => StatusCodes.Status403Forbidden,
                ConflictException => StatusCodes.Status409Conflict,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static ApiResponse<object> CreateResponse(Exception exception)
        {
            return exception switch
            {
                ValidationException validationEx => ApiResponse<object>.CreateFail(
                    "Validation error occurred",
                    validationEx.Errors),

                NotFoundException notFoundEx => ApiResponse<object>.CreateFail(
                    notFoundEx.Message ?? "The requested resource was not found"),

                UnauthorizedAccessException => ApiResponse<object>.CreateFail(
                    "Unauthorized access"),

                ForbiddenAccessException => ApiResponse<object>.CreateFail(
                    "Access to this resource is forbidden"),

                ConflictException conflictEx => ApiResponse<object>.CreateFail(
                    conflictEx.Message ?? "Conflict occurred"),

                NotImplementedException => ApiResponse<object>.CreateFail(
                    "This feature is not implemented"),

                _ => ApiResponse<object>.CreateFail(
                    "An unexpected error occurred",
                    new[] { exception.Message })
            };
        }

        private void LogException(Exception exception, int statusCode)
        {
            var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

            _logger.Log(logLevel, exception,
                "An exception occurred: {Message} (Status Code: {StatusCode})",
                exception.Message, statusCode);
        }
    }










    // Custom exception classes (examples)
    public class ValidationException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public ValidationException(IEnumerable<string> errors) : base("One or more validation errors occurred")
        {
            Errors = errors;
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ForbiddenAccessException : Exception { }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
