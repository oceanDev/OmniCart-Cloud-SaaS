using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using OmniCart.Application.Common.Interfaces;
using OmniCart.Application.Features.Auth.DTOs;
using OmniCart.Infrustructure.Services;
using Omnicart.Domain.Entities;
using Xunit;

namespace OmniCart.UnitTests.Infrastructure;
public class AuthServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ICurrentTenantService> _mockTenantService;

     private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockTenantService = new Mock<ICurrentTenantService>();

        _authService = new AuthService(
            _mockDbContext.Object,
            _mockPasswordHasher.Object,
            _mockConfiguration.Object,
            _mockTenantService.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnFailureResult()
    {
        // Arrange

        var tenantId = Guid.NewGuid();
        _mockTenantService.Setup(t => t.TenantId).Returns(tenantId);

        var emptyUserList = new List<User>();
        var mockUsersDbSet = emptyUserList.BuildMockDbSet();

        _mockDbContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

        var loginRequest = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "AnyPassword123!",
            TenantId: tenantId
        );

        // -------------------------------------------------------------
        // 2. Act 
        // -------------------------------------------------------------

        var result = await _authService.LoginAsync(loginRequest);

        // -------------------------------------------------------------
        // 3. Assert 
        // -------------------------------------------------------------

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email or password.");

        // Is Calling Verify Password.

        _mockPasswordHasher.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),Times.Never);
    }

        [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ShouldReturnFailureResult()
    {
        // 1. Arrange
        var tenantId = Guid.NewGuid();
        _mockTenantService.Setup(t => t.TenantId).Returns(tenantId);

        // ১ জন ডামি ইউজার ডাটাবেজে তৈরি করে দিলাম
        var usersList = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = "existinguser@example.com",
                PasswordHash = "RealHashedPasswordFromDb",
                IsActive = true
            }
        };

        var mockUsersDbSet = usersList.BuildMockDbSet();
        _mockDbContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

        // পাসওয়ার্ড হ্যাশারকে নির্দেশ দিলাম: পাসওয়ার্ড চেক করতে গেলে false ফেরত দেবে (ভুল পাসওয়ার্ড)
        _mockPasswordHasher
            .Setup(p => p.VerifyPassword("WrongPassword123!", "RealHashedPasswordFromDb"))
            .Returns(false);

        var loginRequest = new LoginRequest(
            Email: "existinguser@example.com",
            Password: "WrongPassword123!",
            TenantId: tenantId
        );

        // 2. Act
        var result = await _authService.LoginAsync(loginRequest);

        // 3. Assert
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email or password.");

        // এখানে কিন্তু সত্যিই ১ বার পাসওয়ার্ড চেক হওয়া উচিত!
        _mockPasswordHasher.Verify(
            p => p.VerifyPassword("WrongPassword123!", "RealHashedPasswordFromDb"), 
            Times.Once
        );
    }

        [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnSuccessWithTokens()
    {
        // -------------------------------------------------------------
        // 1. Arrange (প্রস্তুতি)
        // -------------------------------------------------------------
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var email = "success@example.com";
        var validPassword = "CorrectPassword123!";
        var passwordHash = "ValidHashValue";

        _mockTenantService.Setup(t => t.TenantId).Returns(tenantId);

        // JWT তৈরির জন্য কনফিগারেশন সেকশন মক করা
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(s => s["Secret"]).Returns("SuperSecretKeyForTestingPurposesAtLeast32CharsLong!");
        mockJwtSection.Setup(s => s["Issuer"]).Returns("OmniCart.API");
        mockJwtSection.Setup(s => s["Audience"]).Returns("OmniCart.Client");
        mockJwtSection.Setup(s => s["ExpiryInMinutes"]).Returns("60");
        mockJwtSection.Setup(s => s["RefreshTokenExpiryInDays"]).Returns("7");

        _mockConfiguration.Setup(c => c.GetSection("JwtSettings")).Returns(mockJwtSection.Object);

        var usersList = new List<User>
        {
            new User
            {
                Id = userId,
                TenantId = tenantId,
                FirstName = "Jewel",
                LastName = "Sarder",
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            }
        };

        var mockUsersDbSet = usersList.BuildMockDbSet();
        _mockDbContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

        // RefreshTokens টেবিল মক করা
        var emptyRefreshTokens = new List<RefreshToken>();
        var mockRefreshTokensDbSet = emptyRefreshTokens.BuildMockDbSet();
        _mockDbContext.Setup(c => c.RefreshTokens).Returns(mockRefreshTokensDbSet.Object);

        // পাসওয়ার্ড সঠিক হিসেবে মক করা (Returns true)
        _mockPasswordHasher
            .Setup(p => p.VerifyPassword(validPassword, passwordHash))
            .Returns(true);

        var loginRequest = new LoginRequest(
            Email: email,
            Password: validPassword,
            TenantId: tenantId
        );

        // -------------------------------------------------------------
        // 2. Act (মেথড কল করা)
        // -------------------------------------------------------------
        var result = await _authService.LoginAsync(loginRequest);

        // -------------------------------------------------------------
        // 3. Assert (ফলাফল যাচাই)
        // -------------------------------------------------------------
        result.Succeeded.Should().BeTrue();
        Assert.NotNull(result.Data);
        result.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Data.Email.Should().Be(email);
        result.Data.FullName.Should().Be("Jewel Sarder");
    }
}