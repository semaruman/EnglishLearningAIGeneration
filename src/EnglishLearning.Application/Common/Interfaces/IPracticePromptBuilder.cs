using EnglishLearning.Application.Common.Models;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IPracticePromptBuilder
{
    string Build(PracticeTextRequest request);
}
