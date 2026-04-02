namespace ChatConnect.API.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatConnect.Application.Interfaces;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chat;
    private static readonly Dictionary<int, string> _userConnections = new();

    public ChatHub(IChatService chat) => _chat = chat;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _userConnections[userId] = Context.ConnectionId;
        await _chat.SetUserOnlineAsync(userId, true);
        await Clients.Others.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _userConnections.Remove(userId);
        await _chat.SetUserOnlineAsync(userId, false);
        await Clients.Others.SendAsync("UserOffline", userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(int conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{conversationId}");
    }

    public async Task SendMessage(int conversationId, string content)
    {
        var userId = GetUserId();
        var message = await _chat.SendMessageAsync(conversationId, new Application.DTOs.SendMessageDto(content), userId);
        await Clients.Group($"chat-{conversationId}").SendAsync("ReceiveMessage", conversationId, message);
    }

    public async Task Typing(int conversationId)
    {
        var userName = Context.User?.Identity?.Name ?? "Someone";
        await Clients.OthersInGroup($"chat-{conversationId}").SendAsync("UserTyping", conversationId, userName);
    }

    private int GetUserId() => int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
