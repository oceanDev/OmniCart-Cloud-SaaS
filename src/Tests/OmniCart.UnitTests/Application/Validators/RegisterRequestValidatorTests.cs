using FluentAssertions;
using OmniCart.Application.Features.Auth.DTOs;
using OmniCart.Application.Features.Auth.Validators;
using Xunit;

namespace OmniCart.UnitTests.Application.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldPassValidation()
    {
        // 1. Arrange 
        var request = new RegisterRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Password: "SecurePassword123!",
            PhoneNumber: "+8801700000000",
            TenantId: Guid.NewGuid()
        );

        // 2. Act
        var result = _validator.Validate(request);

        // 3. Assert 
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]                   
    [InlineData("   ")]                
    [InlineData("notanemail")]      
    [InlineData("user@")]              
    public void Validate_WhenEmailIsInvalid_ShouldHaveEmailValidationError(string invalidEmail)
    {
        // 1. Arrange
        var request = new RegisterRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: invalidEmail,
            Password: "SecurePassword123!",
            PhoneNumber: null,
            TenantId: Guid.NewGuid()
        );

        // 2. Act
        var result = _validator.Validate(request);

        // 3. Assert 
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]        
    [InlineData("123")]     
    [InlineData("abcde")]  
    public void Validate_WhenPasswordIsTooShortOrEmpty_ShouldHavePasswordValidationError(string shortPassword)
    {
        // 1. Arrange
        var request = new RegisterRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Password: shortPassword,
            PhoneNumber: null,
            TenantId: Guid.NewGuid()
        );

        // 2. Act
        var result = _validator.Validate(request);

        // 3. Assert 
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenFirstNameOrLastNameIsEmpty_ShouldHaveValidationError(string emptyName)
    {
        // 1. Arrange
        var request = new RegisterRequest(
            FirstName: emptyName,
            LastName: emptyName,
            Email: "john.doe@example.com",
            Password: "SecurePassword123!",
            PhoneNumber: null,
            TenantId: Guid.NewGuid()
        );

        // 2. Act
        var result = _validator.Validate(request);

        // 3. Assert 
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }
}