namespace Bug_Tracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}
