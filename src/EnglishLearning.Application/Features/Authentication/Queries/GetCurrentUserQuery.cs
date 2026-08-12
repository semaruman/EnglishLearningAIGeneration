using EnglishLearning.Application.Features.Authentication.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Authentication.Queries;

public record GetCurrentUserQuery : IRequest<UserDto>;
