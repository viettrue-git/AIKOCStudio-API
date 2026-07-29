FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AiKocStudio.sln .
COPY src/Domain/AiKocStudio.Domain.csproj src/Domain/
COPY src/Application/AiKocStudio.Application.csproj src/Application/
COPY src/Infrastructure/AiKocStudio.Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/AiKocStudio.WebApi.csproj src/WebApi/
RUN dotnet restore src/WebApi/AiKocStudio.WebApi.csproj

COPY src/ src/
RUN dotnet publish src/WebApi/AiKocStudio.WebApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AiKocStudio.WebApi.dll"]
