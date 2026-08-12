using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.Learning.DTOs;
using EnglishLearning.Domain.Entities;
using MediatR;

namespace EnglishLearning.Application.Features.Learning.Commands;

public class StartLearningSessionCommandHandler : IRequestHandler<StartLearningSessionCommand, LearningSessionDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILearningSessionRepository _learningSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartLearningSessionCommandHandler(
        ICurrentUserService currentUser,
        ILearningSessionRepository learningSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _learningSessionRepository = learningSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningSessionDto> Handle(StartLearningSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var active = await _learningSessionRepository.GetActiveAsync(userId, cancellationToken);
        if (active is not null)
        {
            return active.ToDto();
        }

        var session = LearningSession.Start(userId);
        await _learningSessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return session.ToDto();
    }
}
