﻿using System.ComponentModel;

namespace Bug_Tracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        [DisplayName("Comment")]
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}
