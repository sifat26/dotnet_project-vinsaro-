using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Auth.DTOs;

namespace Vinsaro.Application.Features.Auth.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<Result<bool>> RevokeTokenAsync(string token, string? ipAddress, CancellationToken cancellationToken = default);
    Task<Result<bool>> ProvisionInitialAdminAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
