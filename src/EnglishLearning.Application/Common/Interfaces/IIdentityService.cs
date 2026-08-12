using EnglishLearning.Application.Features.Authentication.DTOs;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Success, string? UserId, string? Error)> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? UserId, string? Error)> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<string> GenerateJwtAsync(
        string userId,
        string email,
        string userName,
        CancellationToken cancellationToken = default);
}
