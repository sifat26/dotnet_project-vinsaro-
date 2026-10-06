namespace Vinsaro.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword);

public record LoginRequest(
    string Email,
    string Password);

public record AuthResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string AccessToken,
    string RefreshToken,
    long ExpiresIn);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);
