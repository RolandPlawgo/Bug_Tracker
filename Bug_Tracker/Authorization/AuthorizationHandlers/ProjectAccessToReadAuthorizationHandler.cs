using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Bug_Tracker.Authorization.AuthorizationHandlers
{
    public class ProjectAccessToReadAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Project>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Project resource)
        {
            if (requirement.Name == Constants.ProjectReadOperationName)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
