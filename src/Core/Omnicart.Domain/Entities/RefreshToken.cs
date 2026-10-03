using Omnicart.Domain.Common;

namespace Omnicart.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty; // JWT টোকেনের ইউনিক jti আইডি
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public DateTime ExpiresAtUtc { get; set; }

    public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAtUtc;

    // Navigation Property
    public User? User { get; set; }
}
