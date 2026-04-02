namespace ChatConnect.Application.Interfaces;
using ChatConnect.Application.DTOs;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
