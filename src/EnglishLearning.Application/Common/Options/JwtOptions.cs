namespace EnglishLearning.Application.Common.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Alias for <see cref="SecretKey"/> (appsettings "Jwt:Key").</summary>
    public string Key
    {
        get => SecretKey;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                SecretKey = value;
            }
        }
    }

    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>Alias for <see cref="ExpirationMinutes"/> (appsettings "Jwt:ExpireMinutes").</summary>
    public int ExpireMinutes
    {
        get => ExpirationMinutes;
        set
        {
            if (value > 0)
            {
                ExpirationMinutes = value;
            }
        }
    }
}
