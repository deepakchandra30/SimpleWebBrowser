using System;

namespace SimpleWebBrowser.Models
{
    public class HistoryEntry
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime VisitedAt { get; set; }
        
        // Foreign key
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}