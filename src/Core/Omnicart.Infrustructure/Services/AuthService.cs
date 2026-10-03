using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OmniCart.Application.Common.Interfaces;
using OmniCart.Application.Common.Models;
using OmniCart.Application.Features.Auth.DTOs;
using Omnicart.Domain.Entities;

namespace OmniCart.Infrustructure.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenantService _currentTenantService;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ICurrentTenantService currentTenantService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // ১. টেন্যান্ট আইডি নির্ধারণ (রিকোয়েস্ট থেকে, অথবা মিডলওয়্যার থেকে, অথবা ডিফল্ট টেন্যান্ট)
        var tenantId = request.TenantId ?? _currentTenantService.TenantId;

        if (!tenantId.HasValue || tenantId == Guid.Empty)
        {
            // যদি কোনো টেন্যান্ট না থাকে, ডাটাবেজ থেকে প্রথম সক্রিয় টেন্যান্ট নিবে
            var defaultTenant = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);
            if (defaultTenant == null)
            {
                // টেস্টিং ও ডেভেলপমেন্টের সুবিধার জন্য অটোমেটিক ডিফল্ট টেন্যান্ট তৈরি
                defaultTenant = new Tenant
                {
                    Name = "OmniCart Default Store",
                    Subdomain = "default",
                    OwnerEmail = request.Email,
                    Status = Omnicart.Domain.Enums.TenantStatus.Active
                };
                _context.Tenants.Add(defaultTenant);
                await _context.SaveChangesAsync(cancellationToken);
            }
            tenantId = defaultTenant.Id;
        }

        // ২. একই ইমেইল দিয়ে এই টেন্যান্টে আগেই ইউজার আছে কিনা চেক করা
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId.Value && u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            return Result<AuthResponse>.Failure($"User with email '{request.Email}' already exists in this store.");
        }

        // ৩. পাসওয়ার্ড নিরাপদভাবে হ্যাশ করা
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // ৪. নতুন ইউজার এনটিটি তৈরি
        var user = new User
        {
            TenantId = tenantId.Value,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower().Trim(),
            PasswordHash = passwordHash,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // ৫. JWT Access Token ও Refresh Token তৈরি করা
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = request.TenantId ?? _currentTenantService.TenantId;

        // ইউজার খোঁজা (ইমেইল এবং টেন্যান্ট অনুযায়ী)
        var userQuery = _context.Users.IgnoreQueryFilters().AsQueryable();

        if (tenantId.HasValue && tenantId != Guid.Empty)
        {
            userQuery = userQuery.Where(u => u.TenantId == tenantId.Value);
        }

        var user = await userQuery.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower().Trim(), cancellationToken);

        if (user == null)
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure("User account is inactive. Please contact support.");
        }

        // পাসওয়ার্ড ভেরিফাই করা
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        // JWT Access Token ও Refresh Token তৈরি করা
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        // ১. সেভ করা রিফ্রেশ টোকেন ডাটাবেজ থেকে রিড করা
        var savedRefreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (savedRefreshToken == null)
        {
            return Result<AuthResponse>.Failure("Invalid refresh token.");
        }

        if (savedRefreshToken.IsRevoked)
        {
            return Result<AuthResponse>.Failure("Refresh token has been revoked.");
        }

        if (savedRefreshToken.IsUsed)
        {
            // Security: যদি ব্যবহৃত টোকেন আবার কেউ পাঠানোর চেষ্টা করে, ফ্রড সন্দেহে সব সেশন বাতিল করা যায়
            return Result<AuthResponse>.Failure("Refresh token already used.");
        }

        if (savedRefreshToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Result<AuthResponse>.Failure("Refresh token has expired. Please login again.");
        }

        if (savedRefreshToken.User == null || !savedRefreshToken.User.IsActive)
        {
            return Result<AuthResponse>.Failure("User account is invalid or inactive.");
        }

        // ২. বর্তমান রিফ্রেশ টোকেনকে ব্যবহৃত হিসেবে মার্ক করা (Token Rotation)
        savedRefreshToken.IsUsed = true;
        savedRefreshToken.UpdatedAtUtc = DateTime.UtcNow;

        // ৩. নতুন জোড়া (New Access Token + New Refresh Token) তৈরি করে ফেরত দেওয়া
        return await GenerateAuthResponseAsync(savedRefreshToken.User, cancellationToken);
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        var userDto = new UserDto(
            user.Id,
            user.TenantId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString(),
            user.IsActive
        );

        return Result<UserDto>.Success(userDto);
    }

    private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? "SuperSecretKeyForOmniCartCloudEnterpriseSaaSPlatform2026!#*%";
        var issuer = jwtSettings["Issuer"] ?? "OmniCart.API";
        var audience = jwtSettings["Audience"] ?? "OmniCart.Client";
        var expiryInMinutes = double.TryParse(jwtSettings["ExpiryInMinutes"], out var exp) ? exp : 60;
        var refreshTokenExpiryDays = double.TryParse(jwtSettings["RefreshTokenExpiryInDays"], out var rExp) ? rExp : 7;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenId = Guid.NewGuid().ToString();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryInMinutes);

        // JWT Claims (টোকেনের ভেতরে নিরাপদ পে-লোড)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("tenantId", user.TenantId.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // Refresh Token তৈরি (Cryptographically Secure Random String)
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshTokenString = Convert.ToBase64String(randomBytes);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            JwtId = tokenId,
            IsUsed = false,
            IsRevoked = false,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var authResponse = new AuthResponse(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email,
            user.Role.ToString(),
            user.TenantId,
            accessToken,
            refreshTokenString,
            expiresAtUtc
        );

        return Result<AuthResponse>.Success(authResponse);
    }
}
