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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = context.Response;

            if (response.HasStarted)
            {
                _logger.LogWarning("Response already started. Skipping error handling.");
                return;
            }

            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                statusCode = response.StatusCode,
                message = "An unexpected error occurred.",
                details = _env.IsDevelopment() ? ex.ToString() : null
            };

            _logger.LogError(ex, "Unhandled exception occurred.");

            var json = JsonConvert.SerializeObject(
                errorResponse,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                });

            await response.WriteAsync(json);
        }
    }
}
