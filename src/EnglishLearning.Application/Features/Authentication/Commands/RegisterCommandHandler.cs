using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.Authentication.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Authentication.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (success, userId, error) = await _identityService.RegisterAsync(
            request.Email,
            request.UserName,
            request.Password,
            cancellationToken);

        if (!success || userId is null)
        {
            throw new AppException("REGISTRATION_FAILED", error ?? "Registration failed.");
        }

        var token = await _identityService.GenerateJwtAsync(
            userId,
            request.Email,
            request.UserName,
            cancellationToken);

        return new AuthResultDto(token, request.Email, request.UserName, userId);
    }
}
