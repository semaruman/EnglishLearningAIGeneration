using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Common.Options;
using EnglishLearning.Domain.Enums;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Infrastructure.AI;

public class OpenAiLanguageModelService : ILanguageModelService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
    };

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly IPracticePromptBuilder _practicePromptBuilder;
    private readonly IWordDefinitionPromptBuilder _wordDefinitionPromptBuilder;

    public OpenAiLanguageModelService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        IPracticePromptBuilder practicePromptBuilder,
        IWordDefinitionPromptBuilder wordDefinitionPromptBuilder)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _practicePromptBuilder = practicePromptBuilder;
        _wordDefinitionPromptBuilder = wordDefinitionPromptBuilder;
    }

    public async Task<GeneratedWordData> GenerateWordDataAsync(string word, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        var systemPrompt =
            """
            You are a lexicographer for an English learning app for Russian speakers.
            Respond with a single valid JSON object only (no markdown, no commentary) using this schema:
            {
              "wordText": "string",
              "translation": "Russian translation",
              "definition": "English definition",
              "partOfSpeech": "noun|verb|adjective|adverb|...",
              "phonetic": "/ipa/",
              "pronunciation": "optional pronunciation hint",
              "exampleSentence": "A short English example sentence",
              "difficultyLevel": "A1|A2|B1|B2|C1|C2"
            }
            """;

        var userPrompt = _wordDefinitionPromptBuilder.Build(word);
        var content = await CompleteChatAsync(systemPrompt, userPrompt, cancellationToken);
        var parsed = TryParseWordData(content, word);

        if (parsed is not null)
        {
            return parsed;
        }

        content = await CompleteChatAsync(
            systemPrompt + "\nReturn ONLY valid JSON. Do not wrap it in code fences.",
            userPrompt,
            cancellationToken);

        parsed = TryParseWordData(content, word);
        if (parsed is null)
        {
            throw new InvalidOperationException("OpenAI returned invalid word data JSON after retry.");
        }

        return parsed;
    }

    public async Task<string> GeneratePracticeTextAsync(PracticeTextRequest request, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        var prompt = _practicePromptBuilder.Build(request);
        const string systemPrompt =
            "You generate English practice reading texts for language learners. Follow the user instructions exactly and return only the practice text.";

        return await CompleteChatAsync(systemPrompt, prompt, cancellationToken);
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is missing. Configure OpenAI:ApiKey before calling the language model.");
        }
    }

    private async Task<string> CompleteChatAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            model = string.IsNullOrWhiteSpace(_options.Model) ? "gpt-4o-mini" : _options.Model,
            temperature = 0.7,
            max_tokens = 1000,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        message.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI API request failed ({(int)response.StatusCode}): {Truncate(body, 500)}");
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("OpenAI API response did not contain any choices.");
        }

        var content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI API returned empty content.");
        }

        return content.Trim();
    }

    private static GeneratedWordData? TryParseWordData(string content, string fallbackWord)
    {
        var json = ExtractJsonObject(content);
        if (json is null)
        {
            return null;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<WordDataDto>(json, JsonOptions);
            if (dto is null ||
                string.IsNullOrWhiteSpace(dto.Translation) ||
                string.IsNullOrWhiteSpace(dto.Definition) ||
                string.IsNullOrWhiteSpace(dto.PartOfSpeech))
            {
                return null;
            }

            var difficulty = DifficultyLevel.A1;
            if (!string.IsNullOrWhiteSpace(dto.DifficultyLevel) &&
                !Enum.TryParse(dto.DifficultyLevel, ignoreCase: true, out difficulty))
            {
                return null;
            }

            var wordText = !string.IsNullOrWhiteSpace(dto.WordText)
                ? dto.WordText
                : !string.IsNullOrWhiteSpace(dto.Word)
                    ? dto.Word
                    : fallbackWord;

            return new GeneratedWordData
            {
                WordText = wordText.Trim(),
                PartOfSpeech = dto.PartOfSpeech.Trim(),
                Definition = dto.Definition.Trim(),
                Translation = dto.Translation.Trim(),
                Pronunciation = dto.Pronunciation?.Trim(),
                Phonetic = dto.Phonetic?.Trim(),
                ExampleSentence = dto.ExampleSentence?.Trim(),
                DifficultyLevel = difficulty
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractJsonObject(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var lines = trimmed.Split('\n');
            trimmed = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.TrimStart().StartsWith("```", StringComparison.Ordinal)));
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return trimmed[start..(end + 1)];
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";

    private sealed class WordDataDto
    {
        public string? WordText { get; set; }
        public string? Word { get; set; }
        public string? Translation { get; set; }
        public string? Definition { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Phonetic { get; set; }
        public string? Pronunciation { get; set; }
        public string? ExampleSentence { get; set; }
        public string? DifficultyLevel { get; set; }
    }
}
