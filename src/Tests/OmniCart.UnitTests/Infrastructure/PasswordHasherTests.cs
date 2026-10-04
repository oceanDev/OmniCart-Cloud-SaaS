using FluentAssertions;
using OmniCart.Infrustructure.Services;
using Xunit;

namespace OmniCart.UnitTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;
    
    // Constructor
    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_WhenGivenValidPassword_ShouldReturnNonEmptyHashedString()
    {
        // Arrange
        var plainPassword = "SuperSecretPassword123!";

        // Act
        var hashedPassword = _hasher.HashPassword(plainPassword);

        // Assert by FluentAssertion

        hashedPassword.Should().NotBeNullOrWhiteSpace();
        hashedPassword.Should().NotBe(plainPassword);


    }

    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ShouldReturnTrue()
    {
        // Arrange

        var plainPassword = "SuperSecretPassword123!";
        var hashedPassword = _hasher.HashPassword(plainPassword);

        // Act

        var isMatch = _hasher.VerifyPassword(plainPassword,hashedPassword);

        // Assert

        isMatch.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordIsIncorrect_ShouldReturnFalse()
    {
        // Arrange

        var correctPassword = "SuperSecretPassword123!";
        var wrongPassword = "WrongPassword999!";
        var hashedPassword = _hasher.HashPassword(correctPassword);

        // Act

        var isMatch = _hasher.VerifyPassword(wrongPassword,hashedPassword);

        // Assert

        isMatch.Should().BeFalse();
    }

    [Theory]
    [InlineData("WrongPassword1!")]
    [InlineData("Paswword123")]
    [InlineData("paswword123")]
    [InlineData(" ")]

    public void VerifyPassword_WhenPasswordDoesNotMatches_ShouldReturnFalseForAllVariations(string wrongAttempt)
    {
        // 1. Arrange
        var realPassword = "Password123!";
        var hash = _hasher.HashPassword(realPassword);

        // 2. Act
        var isMatch = _hasher.VerifyPassword(wrongAttempt, hash);
        
        // 3. Assert
        isMatch.Should().BeFalse();
    }

}