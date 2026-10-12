using System.ComponentModel.DataAnnotations;

namespace LivePoll.Api.DTOs;

public class RegisterUserDto
{
    [Required, StringLength(50, MinimumLength = 4)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginUserDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}