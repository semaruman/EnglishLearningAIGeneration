# English Learning AI

Production-oriented web app for learning English through a **personal vocabulary**: build your word list, study with spaced repetition, then practice by reading AI texts generated **only** from words you already know.

## Project overview

Users:

1. Register and get an isolated personal account
2. Add words from the global library or ready-made word sets
3. Learn / review words with a simple spaced-repetition scheduler
4. Generate practice texts constrained to their vocabulary
5. Track progress on the dashboard

Backend validates AI output: any word outside the user’s vocabulary triggers a retry (max 3).

## Architecture

Clean Architecture + CQRS (MediatR):

```text
Api → Application → Domain
Infrastructure → Application + Domain
```

```text
GLOBAL WORD DATABASE
        │
        ▼
USER ADDS WORDS / WORD SETS
        │
        ▼
PERSONAL USER VOCABULARY
        │
        ├──► LEARN (spaced repetition)
        │
        └──► PRACTICE (AI text + vocabulary validation)
```

## Technology stack

| Layer | Stack |
|-------|--------|
| Backend | ASP.NET Core, .NET 10, C# |
| Data | PostgreSQL, EF Core, Npgsql |
| Auth | ASP.NET Core Identity + JWT |
| Application | MediatR, FluentValidation |
| Logging | Serilog |
| AI | OpenAI (swappable via `ILanguageModelService`) |
| Frontend | Vanilla JS (ES modules), Tailwind CSS, Fetch API |
| Tests | xUnit, FluentAssertions, WebApplicationFactory |
| Ops | Docker, Docker Compose |

## Project structure

```text
EnglishLearning/
├── src/
│   ├── EnglishLearning.Api/
│   ├── EnglishLearning.Application/
│   ├── EnglishLearning.Domain/
│   └── EnglishLearning.Infrastructure/
├── tests/
│   ├── EnglishLearning.UnitTests/
│   └── EnglishLearning.IntegrationTests/
├── frontend/
├── data/seed/
│   ├── words.json
│   └── word-sets.json
├── docker-compose.yml
├── Dockerfile
├── .env.example
└── README.md
```

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 16+ (local or Docker)
- OpenAI API key (for Practice generation and AI word enrichment)
- Docker Desktop (optional, for compose)

## Configuration

Copy `.env.example` and set secrets via environment variables or `appsettings.Development.json`.

Important settings:

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Key` / `Jwt:SecretKey` | Signing key (long random string) |
| `OpenAI:ApiKey` | OpenAI API key |
| `OpenAI:Model` | Default `gpt-4o-mini` |
| `Practice:MaxVocabularyWords` | Cap of words sent to the LLM (default 2000) |
| `Practice:MaxGenerationRetries` | Validation retries (default 3) |

**Never commit API keys or production JWT secrets.**

### How to add OpenAI API key

1. Create a key at https://platform.openai.com/api-keys
2. Set environment variable:

```powershell
$env:OpenAI__ApiKey = "sk-..."
```

Or in `src/EnglishLearning.Api/appsettings.Development.json`:

```json
"OpenAI": {
  "ApiKey": "sk-...",
  "Model": "gpt-4o-mini"
}
```

For Docker Compose:

```powershell
$env:OPENAI_API_KEY = "sk-..."
docker compose up --build
```

## Installation (local)

```powershell
git clone <repo-url>
cd EnglishLearningAIGeneration
dotnet restore EnglishLearning.slnx
```

Ensure PostgreSQL is running and the connection string matches your instance.

## Database & migrations

Migrations live in `EnglishLearning.Infrastructure`.

On application startup, `DatabaseInitializer`:

1. Applies EF migrations (or `EnsureCreated` for InMemory tests)
2. Seeds words/sets from `data/seed/` if the dictionary is empty

### Run migrations manually

```powershell
dotnet ef database update `
  --project src/EnglishLearning.Infrastructure `
  --startup-project src/EnglishLearning.Api
```

### Create a new migration

```powershell
dotnet ef migrations add <Name> `
  --project src/EnglishLearning.Infrastructure `
  --startup-project src/EnglishLearning.Api `
  --output-dir Persistence/Migrations
```

### How to seed database

Place JSON files in `data/seed/`:

- `words.json` — global dictionary entries
- `word-sets.json` — named collections referencing word texts

Restart the API (seed runs when `Words` table is empty).

### How to import words

Use `IWordImportService` (JSON/CSV). Format:

```json
[
  {
    "word": "beautiful",
    "translation": "красивый",
    "definition": "pleasing to the senses",
    "partOfSpeech": "adjective",
    "level": "B1"
  }
]
```

Import normalizes text, skips duplicates, and batch-inserts.

## Running locally

```powershell
dotnet run --project src/EnglishLearning.Api
```

- App UI: http://localhost:5xxx/ (see launchSettings)
- Swagger: `/swagger`

Default launch profile ports are in `src/EnglishLearning.Api/Properties/launchSettings.json`.

## Docker

```powershell
$env:OPENAI_API_KEY = "sk-..."
docker compose up --build
```

- API + UI: http://localhost:8080
- Postgres: localhost:5432 (`postgres` / `postgres`)

## AI configuration

Abstraction: `ILanguageModelService`  
Implementation: `OpenAiLanguageModelService`  

Practice flow:

1. Select subset of user vocabulary (`IVocabularySelectionStrategy`)
2. Build hardened prompt (`IPracticePromptBuilder`) — topic is data only
3. Call LLM
4. Validate tokens (`ITextVocabularyValidator` + `IWordNormalizer`)
5. Retry up to `MaxGenerationRetries`
6. Persist `PracticeSession`

## API documentation

Swagger UI documents all endpoints with JWT bearer auth.

Main routes:

```text
POST   /api/auth/register
POST   /api/auth/login
GET    /api/auth/me

GET    /api/words
GET    /api/words/{id}
GET    /api/words/search
POST   /api/words                 # add by text (DB or LLM)

GET    /api/vocabulary
POST   /api/vocabulary
DELETE /api/vocabulary/{wordId}
GET    /api/vocabulary/{wordId}

GET    /api/word-sets
GET    /api/word-sets/{id}
POST   /api/word-sets/{id}/add

POST   /api/learning/session
GET    /api/learning/next
POST   /api/learning/{wordId}/answer

POST   /api/practice/generate
GET    /api/practice/history

GET    /api/statistics
```

Error envelope:

```json
{
  "success": false,
  "error": {
    "code": "PRACTICE_GENERATION_FAILED",
    "message": "..."
  }
}
```

User data is always scoped from JWT claims — clients never supply `userId`.

## Frontend

Glassmorphism (Tahoe-inspired) multi-page Vanilla JS app in `frontend/`, served as static files by the API.

Pages: Dashboard, My Vocabulary, Learn, Practice, Word Library, Word Sets, Profile, Login, Register.

## Testing

```powershell
dotnet test EnglishLearning.slnx
```

Coverage includes:

- Domain knowledge / spaced repetition
- Vocabulary text validation & normalization
- Auth flow
- Cross-user vocabulary isolation
- AI text rejection when unknown words appear

## Security notes

- Password hashing via Identity
- JWT authentication / authorization
- FluentValidation on inputs
- Rate limiting on Practice generate and AI word-add
- Prompt-injection hardening (topic treated as data)
- No secrets in source control

## License

Private / educational project — adjust as needed.
