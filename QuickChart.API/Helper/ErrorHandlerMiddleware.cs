using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using Npgsql.Replication.PgOutput.Messages;

namespace QuickChart.API.Helper
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task Invoke(HttpContext context)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            try
            {
                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { statusCode= 401, message= "UnAuthorize"}, settings));
                }
            }
            catch (Exception ex)
            {
                string res = string.Empty;
                var response = context.Response;
                response.ContentType = "application/json";
                var errorMessage =  ex.Message + (ex.InnerException != null ? $" - InnerException -{ex.InnerException} -\n Message - {ex.Message}----- {ex.StackTrace}----- {ex.Source}" : "");
                switch (ex)
                {
                    case UnauthorizedAccessException e:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        res = JsonConvert.SerializeObject(new { statusCode = 401, message = "UnAuthorize user" }, settings);
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        res = JsonConvert.SerializeObject(new { statusCode = 500, message = "Internal server error." }, settings);
                        break;
                }
                _logger.LogError(errorMessage);
                await response.WriteAsync(res);
            }
        }
    }
}
