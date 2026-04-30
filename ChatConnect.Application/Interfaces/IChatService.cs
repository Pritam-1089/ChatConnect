namespace ChatConnect.Application.Interfaces;
using ChatConnect.Application.DTOs;

public interface IChatService
{
    Task<IEnumerable<ConversationDto>> GetConversationsAsync(int userId);
    Task<ConversationDto> GetOrCreatePrivateAsync(int userId, int otherUserId);
    Task<ConversationDto> CreateGroupAsync(CreateGroupDto dto, int userId);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId, int skip = 0);
    Task<MessageDto> SendMessageAsync(int conversationId, SendMessageDto dto, int userId);
    Task MarkAsReadAsync(int conversationId, int userId);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string query, int userId);
    Task<IEnumerable<UserDto>> GetOnlineUsersAsync();
    Task SetUserOnlineAsync(int userId, bool isOnline);
    Task UnsendMessageAsync(int messageId, int userId);
    Task<string?> UpdateAvatarAsync(int userId, string avatarUrl);
}
