namespace LostAndFoundBack.Models
{
    public class UpdateAdDto
    {
        public int CategoryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}