using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Bug_Tracker.Authorization
{
    public static class TicketOperationAuthorizationRequirements
    {
        public static readonly OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement() { Name = Constants.TicketCreateOperationName};
        public static readonly OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement() { Name = Constants.TicketReadOperationName };
        public static readonly OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement() { Name = Constants.TicketUpdateOperationName };
        public static readonly OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement() { Name = Constants.TicketDeleteOperationName };
    }
}
