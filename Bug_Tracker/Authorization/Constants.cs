namespace Bug_Tracker.Authorization
{
    public class Constants
    {
        public static readonly string AdministratorsRole = "admin";
        public static readonly string ManagersRole = "manager";
        public static readonly string UsersRole = "user";


        public static readonly string ProjectCreateOperationName = "CreateProject";
        public static readonly string ProjectReadOperationName = "ReadProject";
        public static readonly string ProjectUpdateOperationName = "UpdateProject";
        public static readonly string ProjectDeleteOperationName = "DeleteProject";

        public static readonly string TicketCreateOperationName = "CreateTicket";
        public static readonly string TicketReadOperationName = "ReadTicket";
        public static readonly string TicketUpdateOperationName = "UpdateTicket";
        public static readonly string TicketDeleteOperationName = "DeleteTicket";

        public static readonly string CommentCreateOperationName = "CreateComment";
        public static readonly string CommentReadOperationName = "ReadComment";
        public static readonly string CommentUpdateOperationName = "UpdateComment";
        public static readonly string CommentDeleteOperationName = "DeleteComment";
    }
}
