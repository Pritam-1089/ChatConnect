namespace ChatConnect.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ChatConnect.Application.DTOs;
using ChatConnect.Application.Interfaces;
using ChatConnect.Core.Entities;
using ChatConnect.Core.Interfaces;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config) { _userRepo = userRepo; _config = config; }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.GetByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email already registered.");
        var user = new User { FullName = dto.FullName, Email = dto.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) };
        await _userRepo.AddAsync(user);
        return new AuthResponseDto(GenerateToken(user), user.Id, user.FullName, user.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email) ?? throw new UnauthorizedAccessException("Invalid credentials.");
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) throw new UnauthorizedAccessException("Invalid credentials.");
        return new AuthResponseDto(GenerateToken(user), user.Id, user.FullName, user.Email);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"], audience: _config["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Email, user.Email) },
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
