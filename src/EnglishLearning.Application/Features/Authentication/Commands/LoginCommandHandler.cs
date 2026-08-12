using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.Authentication.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Authentication.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (success, userId, error) = await _identityService.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!success || userId is null)
        {
            throw new AppException("INVALID_CREDENTIALS", error ?? "Invalid email or password.", 401);
        }

        var user = await _identityService.GetUserAsync(userId, cancellationToken)
            ?? throw new AppException("USER_NOT_FOUND", "User was not found.", 404);

        var token = await _identityService.GenerateJwtAsync(
            user.UserId,
            user.Email,
            user.UserName,
            cancellationToken);

        return new AuthResultDto(token, user.Email, user.UserName, user.UserId);
    }
}
