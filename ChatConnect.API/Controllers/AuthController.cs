namespace ChatConnect.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using ChatConnect.Application.DTOs;
using ChatConnect.Application.Interfaces;

[ApiController, Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto) => Ok(await _auth.RegisterAsync(dto));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto) => Ok(await _auth.LoginAsync(dto));
}
