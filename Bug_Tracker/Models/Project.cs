namespace Bug_Tracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Ticket> Tickets { get; set; }
    }
}
