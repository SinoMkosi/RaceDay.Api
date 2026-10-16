using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Security
{
    /// <summary>
    /// Blocks requests that are not logged in (401).
    /// If a role is given, blocks other roles (403).
    /// </summary>
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string? _role;

        public SessionAuthorizeAttribute(string? role = null)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var role = session.GetString("Role");

            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult("Please log in first.");
                return;
            }

            if (_role != null && role != _role)
            {
                context.Result = new ObjectResult("You do not have access to this.")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}