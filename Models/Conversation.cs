namespace LostAndFoundBack.Models
{
    public class Conversation
    {
        public int ConversationID { get; set; }

        public int AdID { get; set; }
        public Ad? Ad { get; set; }

        public int User1ID { get; set; }
        public User? User1 { get; set; }

        public int User2ID { get; set; }
        public User? User2 { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Message> Messages { get; set; } = new();
    }
}