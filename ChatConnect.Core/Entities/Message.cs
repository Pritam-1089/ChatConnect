namespace ChatConnect.Core.Entities;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
