namespace ChatConnect.API.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatConnect.Application.DTOs;
using ChatConnect.Application.Interfaces;

[ApiController, Route("api/[controller]"), Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chat;
    public ConversationsController(IChatService chat) => _chat = chat;
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetAll() => Ok(await _chat.GetConversationsAsync(UserId));

    [HttpPost("private/{otherUserId}")]
    public async Task<ActionResult<ConversationDto>> GetOrCreatePrivate(int otherUserId) => Ok(await _chat.GetOrCreatePrivateAsync(UserId, otherUserId));

    [HttpPost("group")]
    public async Task<ActionResult<ConversationDto>> CreateGroup(CreateGroupDto dto) => Ok(await _chat.CreateGroupAsync(dto, UserId));

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int id, [FromQuery] int skip = 0) => Ok(await _chat.GetMessagesAsync(id, UserId, skip));

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(int id, SendMessageDto dto) => Ok(await _chat.SendMessageAsync(id, dto, UserId));

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id) { await _chat.MarkAsReadAsync(id, UserId); return NoContent(); }

    [HttpDelete("{convoId}/messages/{msgId}")]
    public async Task<IActionResult> UnsendMessage(int convoId, int msgId) { await _chat.UnsendMessageAsync(msgId, UserId); return NoContent(); }
}
