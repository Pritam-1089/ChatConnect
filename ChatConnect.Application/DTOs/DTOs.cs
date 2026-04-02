namespace ChatConnect.Application.DTOs;

public record RegisterDto(string FullName, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, int UserId, string FullName, string Email);
public record UserDto(int Id, string FullName, string Email, string? AvatarUrl, bool IsOnline, DateTime LastSeen);
public record ConversationDto(int Id, string Name, bool IsGroup, List<UserDto> Members, MessageDto? LastMessage, int UnreadCount);
public record CreateGroupDto(string Name, List<int> MemberIds);
public record MessageDto(int Id, string Content, int SenderId, string SenderName, bool IsRead, DateTime SentAt);
public record SendMessageDto(string Content);
