using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.Authentication.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Authentication.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var user = await _identityService.GetUserAsync(userId, cancellationToken);

        return user ?? throw new AppException("USER_NOT_FOUND", "User was not found.", 404);
    }
}
