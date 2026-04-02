namespace LostAndFoundBack.Models
{
    public class Ad
    {
        public int AdID { get; set; }
        public int UserID { get; set; }
        public int CategoryID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}