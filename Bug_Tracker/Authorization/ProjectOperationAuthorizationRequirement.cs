using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Bug_Tracker.Authorization
{
    public class ProjectOperationAuthorizationRequirements
    {
        public static readonly OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement() { Name = Constants.ProjectCreateOperationName};
        public static readonly OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement() { Name = Constants.ProjectReadOperationName };
        public static readonly OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement() { Name = Constants.ProjectUpdateOperationName };
        public static readonly OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement() { Name = Constants.ProjectDeleteOperationName };
    }
}
