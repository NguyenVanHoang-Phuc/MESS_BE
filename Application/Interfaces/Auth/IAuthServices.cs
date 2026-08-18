using MESS.Domain.Entities;

namespace MESS.Application.Interfaces.Auth;

public interface ITokenService
{
    string GenerateToken(User user, string[] roles);
    string GenerateRefreshToken();
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}
