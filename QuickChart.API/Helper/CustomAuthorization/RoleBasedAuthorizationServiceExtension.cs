using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace QuickChart.API.Helper.CustomAuthorization
{
    public static class RoleBasedAuthorizationServiceExtension
    {
        public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            var requireValidation = configuration.GetValue<bool>("AppConfiguration:IsRoleWiseMenuActionPermissionEnabled", false);
            var roleNames = configuration.GetSection("AppConfiguration:AllowedRoles").Get<string[]>();

            services.AddScoped<IAuthorizationHandler, RoleBasedAuthorizationRequirementHandler>();

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder() // applies to all [Authorize] endpoints
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                            .AddRequirements(new RoleBasedRequirement(roleNames!, requireValidation))
                                .Build())
                .AddPolicy(RolebasedPolicy.AdminOnly, new AuthorizationPolicyBuilder()
                   .RequireAuthenticatedUser()
                        .AddRequirements(new RoleBasedRequirement(new[] { "Admin" }, true))
                            .Build())
                .AddPolicy(RolebasedPolicy.SupperAdminOnly, new AuthorizationPolicyBuilder()
                   .RequireAuthenticatedUser()
                        .AddRequirements(new RoleBasedRequirement(new[] { "SupperAdmin" }, true))
                            .Build());
            return services;
        }
    }
}
