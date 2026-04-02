namespace ChatConnect.Core.Entities;

public class ConversationMember
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int UserId { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
}
