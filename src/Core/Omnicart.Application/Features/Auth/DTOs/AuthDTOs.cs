using Omnicart.Domain.Enums;

namespace OmniCart.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber,
    UserRole Role = UserRole.Customer,
    Guid? TenantId = null
);

public record LoginRequest(
    string Email,
    string Password,
    Guid? TenantId = null
);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);

public record AuthResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    Guid TenantId,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc
);

public record UserDto(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive
);
