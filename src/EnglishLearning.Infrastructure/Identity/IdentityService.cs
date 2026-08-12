using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.Authentication.DTOs;
using Microsoft.AspNetCore.Identity;

namespace EnglishLearning.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(bool Success, string? UserId, string? Error)> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            return (false, null, "A user with this email already exists.");
        }

        var existingByName = await _userManager.FindByNameAsync(userName);
        if (existingByName is not null)
        {
            return (false, null, "A user with this username already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, null, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return (true, user.Id, null);
    }

    public async Task<(bool Success, string? UserId, string? Error)> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return (false, null, "Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (false, null, "Invalid email or password.");
        }

        return (true, user.Id, null);
    }

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        return new UserDto(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty);
    }

    public Task<string> GenerateJwtAsync(
        string userId,
        string email,
        string userName,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_jwtTokenService.GenerateToken(userId, email, userName));
}
