using Bug_Tracker.Authorization;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

public class TicketAccessToReadAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Ticket>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Ticket resource)
    {
        if (requirement.Name == Constants.TicketReadOperationName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
