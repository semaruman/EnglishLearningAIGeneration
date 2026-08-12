using Microsoft.AspNetCore.Identity;

namespace EnglishLearning.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
