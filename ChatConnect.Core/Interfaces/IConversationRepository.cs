namespace ChatConnect.Core.Interfaces;
using ChatConnect.Core.Entities;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(int id);
    Task<Conversation?> GetWithMembersAsync(int id);
    Task<Conversation?> GetPrivateConversationAsync(int userId1, int userId2);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
    Task<Conversation> AddAsync(Conversation conversation);
    Task UpdateAsync(Conversation conversation);
}
