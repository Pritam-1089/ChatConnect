namespace ChatConnect.Application.Services;
using ChatConnect.Application.DTOs;
using ChatConnect.Application.Interfaces;
using ChatConnect.Core.Entities;
using ChatConnect.Core.Interfaces;

public class ChatService : IChatService
{
    private readonly IConversationRepository _convoRepo;
    private readonly IMessageRepository _msgRepo;
    private readonly IUserRepository _userRepo;

    public ChatService(IConversationRepository convoRepo, IMessageRepository msgRepo, IUserRepository userRepo)
    { _convoRepo = convoRepo; _msgRepo = msgRepo; _userRepo = userRepo; }

    public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(int userId)
    {
        var convos = await _convoRepo.GetUserConversationsAsync(userId);
        var result = new List<ConversationDto>();
        foreach (var c in convos)
        {
            var unread = await _msgRepo.GetUnreadCountAsync(c.Id, userId);
            var lastMsg = c.Messages.FirstOrDefault();
            result.Add(new ConversationDto(c.Id,
                c.IsGroup ? c.Name! : c.Members.First(m => m.UserId != userId).User.FullName,
                c.IsGroup, c.Members.Select(m => MapUser(m.User)).ToList(),
                lastMsg != null ? new MessageDto(lastMsg.Id, lastMsg.Content, lastMsg.SenderId, lastMsg.Sender.FullName, lastMsg.IsRead, lastMsg.SentAt) : null,
                unread));
        }
        return result;
    }

    public async Task<ConversationDto> GetOrCreatePrivateAsync(int userId, int otherUserId)
    {
        var existing = await _convoRepo.GetPrivateConversationAsync(userId, otherUserId);
        if (existing != null)
        {
            var full = await _convoRepo.GetWithMembersAsync(existing.Id);
            return new ConversationDto(full!.Id, full.Members.First(m => m.UserId != userId).User.FullName, false,
                full.Members.Select(m => MapUser(m.User)).ToList(), null, 0);
        }
        var convo = new Conversation { IsGroup = false };
        await _convoRepo.AddAsync(convo);
        convo.Members.Add(new ConversationMember { ConversationId = convo.Id, UserId = userId, IsAdmin = false });
        convo.Members.Add(new ConversationMember { ConversationId = convo.Id, UserId = otherUserId, IsAdmin = false });
        await _convoRepo.UpdateAsync(convo);
        var other = await _userRepo.GetByIdAsync(otherUserId);
        var me = await _userRepo.GetByIdAsync(userId);
        return new ConversationDto(convo.Id, other!.FullName, false,
            new List<UserDto> { MapUser(me!), MapUser(other) }, null, 0);
    }

    public async Task<ConversationDto> CreateGroupAsync(CreateGroupDto dto, int userId)
    {
        var convo = new Conversation { Name = dto.Name, IsGroup = true };
        await _convoRepo.AddAsync(convo);
        convo.Members.Add(new ConversationMember { ConversationId = convo.Id, UserId = userId, IsAdmin = true });
        foreach (var memberId in dto.MemberIds.Where(id => id != userId))
            convo.Members.Add(new ConversationMember { ConversationId = convo.Id, UserId = memberId });
        await _convoRepo.UpdateAsync(convo);
        var full = await _convoRepo.GetWithMembersAsync(convo.Id);
        return new ConversationDto(convo.Id, dto.Name, true, full!.Members.Select(m => MapUser(m.User)).ToList(), null, 0);
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId, int skip = 0)
    {
        var messages = await _msgRepo.GetConversationMessagesAsync(conversationId, skip);
        return messages.Select(m => new MessageDto(m.Id, m.Content, m.SenderId, m.Sender.FullName, m.IsRead, m.SentAt));
    }

    public async Task<MessageDto> SendMessageAsync(int conversationId, SendMessageDto dto, int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        var message = new Message { Content = dto.Content, ConversationId = conversationId, SenderId = userId };
        await _msgRepo.AddAsync(message);
        return new MessageDto(message.Id, message.Content, userId, user!.FullName, false, message.SentAt);
    }

    public async Task MarkAsReadAsync(int conversationId, int userId) => await _msgRepo.MarkAsReadAsync(conversationId, userId);
    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query, int userId) =>
        (await _userRepo.SearchUsersAsync(query, userId)).Select(MapUser);
    public async Task<IEnumerable<UserDto>> GetOnlineUsersAsync() =>
        (await _userRepo.GetOnlineUsersAsync()).Select(MapUser);

    public async Task SetUserOnlineAsync(int userId, bool isOnline)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null) { user.IsOnline = isOnline; user.LastSeen = DateTime.UtcNow; await _userRepo.UpdateAsync(user); }
    }

    private static UserDto MapUser(User u) => new(u.Id, u.FullName, u.Email, u.AvatarUrl, u.IsOnline, u.LastSeen);
}
