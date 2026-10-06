using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Auth.DTOs;
using Vinsaro.Application.Features.Auth.Interfaces;
using Vinsaro.Infrastructure.Identity.Options;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly VinsaroDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        VinsaroDbContext dbContext,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<AuthResponse>.Failure("Email already in use.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result<AuthResponse>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Add default role Customer
        await EnsureRoleExistsAsync("Customer");
        await _userManager.AddToRoleAsync(user, "Customer");

        var roles = await _userManager.GetRolesAsync(user);
        return await GenerateAuthResponseAsync(user, roles, null, null, cancellationToken);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await GenerateAuthResponseAsync(user, roles, ipAddress, userAgent, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Result<AuthResponse>.Failure("Invalid token.");

        var idClaim = principal.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !Guid.TryParse(idClaim.Value, out var userId))
            return Result<AuthResponse>.Failure("Invalid token.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            return Result<AuthResponse>.Failure("User not found or inactive.");

        var tokenHash = HashString(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        // Revoke the old token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var response = await GenerateAuthResponseAsync(user, roles, ipAddress, userAgent, cancellationToken);

        if (response.IsSuccess)
        {
            refreshToken.ReplacedByTokenHash = HashString(response.Value!.RefreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return response;
    }

    public async Task<Result<bool>> RevokeTokenAsync(string token, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashString(token);
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Result<bool>.Failure("Invalid or expired refresh token.");
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ProvisionInitialAdminAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Explicit, secure way: check if ANY user exists in the system.
        var anyUserExists = await _dbContext.Users.AnyAsync(cancellationToken);
        if (anyUserExists)
        {
            return Result<bool>.Failure("System is already initialized. Cannot provision initial admin.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await EnsureRoleExistsAsync("Admin");
        await _userManager.AddToRoleAsync(user, "Admin");

        return Result<bool>.Success(true);
    }

    private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(
        ApplicationUser user, 
        IList<string> roles, 
        string? ipAddress, 
        string? userAgent, 
        CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.FirstName, user.LastName, roles);
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashString(refreshTokenString),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            accessToken,
            refreshTokenString,
            _jwtOptions.ExpiryMinutes * 60
        );

        return Result<AuthResponse>.Success(response);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new ApplicationRole(roleName));
        }
    }

    private string HashString(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
