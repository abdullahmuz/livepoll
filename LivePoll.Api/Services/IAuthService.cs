using LivePoll.Api.DTOs;

namespace LivePoll.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginUserDto dto);
}