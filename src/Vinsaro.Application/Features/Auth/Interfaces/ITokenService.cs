using System.Security.Claims;

namespace Vinsaro.Application.Features.Auth.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string firstName, string lastName, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
