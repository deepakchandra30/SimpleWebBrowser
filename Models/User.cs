using System.Collections.Generic;

namespace SimpleWebBrowser.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string HomePage { get; set; } = "";
        
        // Navigation properties
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
        public virtual ICollection<HistoryEntry> History { get; set; }
    }
}