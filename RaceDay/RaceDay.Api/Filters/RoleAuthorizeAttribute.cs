using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Api.Services;

namespace RaceDay.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();

            if (sessionService == null || !sessionService.IsAuthenticated())
            {
                context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(sessionService.GetUserRole()))
            {
                context.Result = new ObjectResult(new { message = "You do not have permission to perform this action." })
                {
                    StatusCode = 403
                };
            }
        }
    }

    public class OrganiserOnlyAttribute : RoleAuthorizeAttribute
    {
        public OrganiserOnlyAttribute() : base("Organiser") { }
    }

    public class ParticipantOnlyAttribute : RoleAuthorizeAttribute
    {
        public ParticipantOnlyAttribute() : base("Participant") { }
    }

    public class AuthenticatedAttribute : RoleAuthorizeAttribute
    {
        public AuthenticatedAttribute() : base() { }
    }
}