using System.Reflection;
using EnglishLearning.Application.Common.Behaviors;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Interfaces;
using EnglishLearning.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishLearning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<IReviewScheduler, SpacedRepetitionScheduler>();
        services.AddSingleton<IWordNormalizer, BasicWordNormalizer>();
        services.AddSingleton<ITextVocabularyValidator, TextVocabularyValidator>();
        services.AddSingleton<IPracticePromptBuilder, PracticePromptBuilder>();
        services.AddSingleton<IWordDefinitionPromptBuilder, WordDefinitionPromptBuilder>();
        services.AddScoped<IVocabularySelectionStrategy, DefaultVocabularySelectionStrategy>();

        return services;
    }
}
