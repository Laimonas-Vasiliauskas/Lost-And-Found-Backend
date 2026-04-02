using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LostAndFoundBack.Models
{
    [Table("AdImages")]
    public class AdImage
    {
        [Key]
        public int ImageID { get; set; }

        public int AdID { get; set; }

        public string ImageUrl { get; set; } = null!;
    }
}