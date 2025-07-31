using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace QuickChart.API.Helper.CustomAuthorization
{
    public class RoleBasedRequirement: IAuthorizationRequirement
    {
        public IEnumerable<string> Roles { get; }
        public bool RequireValidation { get; }
        public RoleBasedRequirement(IEnumerable<string> roles, bool requireValidation)
        {
            Roles = roles;
            RequireValidation = requireValidation;
        }
    }

    public class RoleBasedAuthorizationRequirementHandler : AuthorizationHandler<RoleBasedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleBasedRequirement requirement)
        {
            try
            {
                if (!requirement.RequireValidation || context.Resource is not HttpContext httpContext || IsGloballyAllowedPath(httpContext.Request.Path.Value))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                var user = context.User;
                var hasValidRole = requirement.Roles.Any(role => user.IsInRole(role));

                if (hasValidRole)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
                else
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
                #region more complex logic if need for role based action permissions
                //var permissions = await GetPermissionsFromCacheOrDb();
                //if (permissions == null || !HasPermission(permissions, userRoleId, apiRoute, frontendRoute))
                //{
                //    context.Fail();
                //    return;
                //}
                #endregion
            }
            catch (Exception)
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }

        private static bool IsGloballyAllowedPath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return true;

            FieldInfo[] fields = typeof(GloballyAllowedHttpPath)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);

            return Array.Exists(fields, field =>
            {
                if (field.FieldType == typeof(string))
                {
                    return field.GetValue(null) is string fieldValue &&
                           path.Contains(fieldValue, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            });
        }
    }
}
