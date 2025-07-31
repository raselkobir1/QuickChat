using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Extensions;
using QuickChart.API.Helper.Enums;

namespace QuickChart.API.Helper.CustomAuthorization
{
    public static class RoleBasedAuthorizationServiceExtension
    {
        public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            var requireValidation = configuration.GetValue<bool>("AppConfiguration:IsRoleWiseMenuActionPermissionEnabled", false);
            var roles = Enum.GetNames(typeof(Roles)).ToList(); 

            services.AddScoped<IAuthorizationHandler, RoleBasedAuthorizationRequirementHandler>();

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder() // applies to all [Authorize] endpoints
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                            .AddRequirements(new RoleBasedRequirement(roles, requireValidation))
                                .Build())
                .AddPolicy(RolebasedPolicy.AdminOnly, new AuthorizationPolicyBuilder()
                   .RequireAuthenticatedUser()
                        .AddRequirements(new RoleBasedRequirement(new[] { Roles.Admin.ToString() }, true))
                            .Build())
                .AddPolicy(RolebasedPolicy.SupperAdminOnly, new AuthorizationPolicyBuilder()
                   .RequireAuthenticatedUser()
                        .AddRequirements(new RoleBasedRequirement(new[] { Roles.SuperAdmin.ToString() }, true))
                            .Build());
            return services;
        }
    }
}
