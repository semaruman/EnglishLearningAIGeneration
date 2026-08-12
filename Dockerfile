# Dockerfile for English Learning API + static frontend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EnglishLearning.slnx ./
COPY src/EnglishLearning.Domain/EnglishLearning.Domain.csproj src/EnglishLearning.Domain/
COPY src/EnglishLearning.Application/EnglishLearning.Application.csproj src/EnglishLearning.Application/
COPY src/EnglishLearning.Infrastructure/EnglishLearning.Infrastructure.csproj src/EnglishLearning.Infrastructure/
COPY src/EnglishLearning.Api/EnglishLearning.Api.csproj src/EnglishLearning.Api/

RUN dotnet restore src/EnglishLearning.Api/EnglishLearning.Api.csproj

COPY src/ src/
COPY data/ data/
COPY frontend/ frontend/

RUN dotnet publish src/EnglishLearning.Api/EnglishLearning.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /src/frontend /app/frontend
COPY --from=build /src/data /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "EnglishLearning.Api.dll"]
