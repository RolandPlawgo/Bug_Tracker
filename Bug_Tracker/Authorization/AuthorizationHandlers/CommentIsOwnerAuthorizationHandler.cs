using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Bug_Tracker.Authorization.AuthorizationHandlers
{
    public class CommentIsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Comment>
    {
        UserManager<IdentityUser> _userManager;
        public CommentIsOwnerAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Comment resource)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            if (resource.OwnerId == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
