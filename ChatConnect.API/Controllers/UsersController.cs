namespace ChatConnect.API.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatConnect.Application.DTOs;
using ChatConnect.Application.Interfaces;

[ApiController, Route("api/[controller]"), Authorize]
public class UsersController : ControllerBase
{
    private readonly IChatService _chat;
    public UsersController(IChatService chat) => _chat = chat;
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Search([FromQuery] string q) => Ok(await _chat.SearchUsersAsync(q ?? "", UserId));

    [HttpGet("online")]
    public async Task<ActionResult<IEnumerable<UserDto>>> Online() => Ok(await _chat.GetOnlineUsersAsync());

    [HttpPut("me/avatar")]
    public async Task<ActionResult<object>> UpdateAvatar([FromBody] UpdateAvatarDto dto)
    {
        var url = await _chat.UpdateAvatarAsync(UserId, dto.AvatarUrl);
        return Ok(new { avatarUrl = url });
    }
}
