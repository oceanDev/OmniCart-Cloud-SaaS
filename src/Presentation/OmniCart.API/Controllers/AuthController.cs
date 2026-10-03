using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniCart.Application.Common.Interfaces;
using OmniCart.Application.Features.Auth.DTOs;

namespace OmniCart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// নতুন ইউজার বা স্টোর অ্যাডমিন রেজিস্ট্রেশন
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// ইউজার লগইন ও JWT Access + Refresh Token প্রাপ্তি
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(new { result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// রিফ্রেশ টোকেন রোটেশন (নতুন অ্যাক্সেস টোকেন জেনারেট করা)
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(new { result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// বর্তমান লগইন করা ইউজারের প্রোফাইল তথ্য দেখা (Requires Bearer Token)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token claims." });
        }

        var result = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(new { result.Errors });
        }

        return Ok(result.Data);
    }
}
