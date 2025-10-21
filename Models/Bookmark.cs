namespace SimpleWebBrowser.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        
        // Foreign key
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}