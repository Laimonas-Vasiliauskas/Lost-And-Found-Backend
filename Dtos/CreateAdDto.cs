namespace LostAndFoundBack.Models
{
    public class CreateAdDto
    {
        public string Type { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}