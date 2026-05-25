namespace LostAndFoundBack.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public Conversation? Conversation { get; set; }
        public int SenderID { get; set; }
        public User? Sender { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}