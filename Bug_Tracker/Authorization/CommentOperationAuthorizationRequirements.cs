using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Bug_Tracker.Authorization
{
    public class CommentOperationAuthorizationRequirements
    {
        public static readonly OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement() { Name = Constants.CommentCreateOperationName };
        public static readonly OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement() { Name = Constants.CommentReadOperationName };
        public static readonly OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement() { Name = Constants.CommentUpdateOperationName };
        public static readonly OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement() { Name = Constants.CommentDeleteOperationName };
    }
}
