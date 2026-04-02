namespace LostAndFoundBack.Dtos
{
    public class UploadImageDto
    {
        public required IFormFile File { get; set; }
        public int AdId { get; set; }
    }
}
