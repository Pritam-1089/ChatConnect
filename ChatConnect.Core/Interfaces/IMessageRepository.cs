namespace ChatConnect.Core.Interfaces;
using ChatConnect.Core.Entities;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message);
    Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50);
    Task MarkAsReadAsync(int conversationId, int userId);
    Task<int> GetUnreadCountAsync(int conversationId, int userId);
}
