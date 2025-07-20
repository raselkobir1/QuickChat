using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
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
            var forbidden = new
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "You don't have permission for the requisted resource.",
            };
            return new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.HandleResponse();
                        string message = context.AuthenticateFailure switch
                        {
                            SecurityTokenExpiredException => "Token expired",
                            SecurityTokenInvalidSignatureException => "Invalid token signature",
                            null => "No token provided",
                            _ => "Token validation failed"
                        };

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            StatusCode = 401,
                            Message = message
                        });
                    }
                },
                OnForbidden = async httpContext =>
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await httpContext.Response.WriteAsJsonAsync(forbidden).ConfigureAwait(false);
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = async context =>
                {
                    if (!context.Response.HasStarted) {
                        var errorMessage = context.Exception switch
                        {
                            SecurityTokenExpiredException => "Token expired",
                            SecurityTokenInvalidSignatureException => "Invalid token signature",
                            _ => "Authentication failed"
                        };

                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = errorMessage,
                            details = context.Exception.Message // Only in development
                        });
                    }
                }
            };
        }
    }
}
