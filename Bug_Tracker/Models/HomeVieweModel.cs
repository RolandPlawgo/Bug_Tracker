namespace Bug_Tracker.Models
{
    public class HomeVieweModel
    {
        // Projects, in which the user has lately created tickets or comments (and other if there's too little of those)
        public List<Project> Projects { get; set; }

        // Latest comments on the tickets owned by the user
        public List<Comment> NewCommentsOnUsersTickets { get; set; }
    }
}
