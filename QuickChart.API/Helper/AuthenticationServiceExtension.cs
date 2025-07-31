using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace QuickChart.API.Helper.Extensions
{
    public static class AuthenticationServiceExtension
    {
        public static IServiceCollection AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;
                options.Events = ConfigureJwtBearerEvents();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidIssuer = configuration["jwtSettings:Issuer"],
                    ValidAudience = configuration["jwtSettings:Audience"],
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtSettings:Key"] ?? string.Empty))
                };
            });

            return services;
        }

        private static JwtBearerEvents ConfigureJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();

                    if (!context.Response.HasStarted)
                    {
                        var message = context.AuthenticateFailure switch
                        {
                            SecurityTokenExpiredException => "Token expired",
                            SecurityTokenInvalidSignatureException => "Invalid token signature",
                            null => "No token provided",
                            _ => "Token validation failed"
                        };

                        await WriteJsonAsync(context.Response, StatusCodes.Status401Unauthorized, new
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Message = message
                        });
                    }
                },

                OnForbidden = async context =>
                {
                    await WriteJsonAsync(context.Response, StatusCodes.Status403Forbidden, new
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You don't have permission for the requested resource."
                    });
                },

                OnTokenValidated = context =>
                {
                    //Use for logging, user checks active/inactive, or adding claims
                    Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}"); 
                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = async context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        var errorMessage = context.Exception switch
                        {
                            SecurityTokenExpiredException => "Token expired",
                            SecurityTokenInvalidSignatureException => "Invalid token signature",
                            _ => "Authentication failed"
                        };

                        await WriteJsonAsync(context.Response, StatusCodes.Status401Unauthorized, new
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Message = errorMessage,
                            Details = context.Exception.Message // Optional: only show in development
                        });
                    }
                },

                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        }

        private static async Task WriteJsonAsync(HttpResponse response, int statusCode, object content)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            try
            {
                await response.WriteAsJsonAsync(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write JSON response: {ex.Message}");
                throw;
            }
        }
    }
}
