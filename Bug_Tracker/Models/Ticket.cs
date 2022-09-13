namespace Bug_Tracker.Models
{
    public enum Status { bug, feature}
    public enum Priority { none, low, medium, high}
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime Date { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
