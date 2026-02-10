namespace IndasEstimo.Application.DTOs.Auth;

public record UserInfoDto(
    int UserId,
    string Username,
    string? Email,
    string? FullName,
    List<string> Roles);
