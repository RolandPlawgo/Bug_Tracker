using System.ComponentModel;

namespace Bug_Tracker.Models
{
    public enum Status { bug, feature}
    public enum Priority { none, low, medium, high}
    public class Ticket
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string Title { get; set; }
        [DisplayName("Description")]
        public string ShortDescription { get; set; }
        [DisplayName("Long description")]
        public string LongDescription { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime Date { get; set; }
        public List<Comment> Comments { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
