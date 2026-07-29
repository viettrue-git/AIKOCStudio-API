using System.Net;
using System.Net.Http.Json;
using AiKocStudio.Application.Auth.Commands.Login;
using AiKocStudio.Application.Common.Models;
using AiKocStudio.Application.Personas;
using AiKocStudio.Application.Personas.Commands.CreatePersona;
using AiKocStudio.Application.Personas.Commands.UpdatePersona;
using AiKocStudio.Domain.Enums;
using FluentAssertions;

namespace AiKocStudio.Application.FunctionalTests.Personas;

public class PersonasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PersonasControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginCommand("admin@aikocstudio.local", "ChangeMe123!"));
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        return client;
    }

    [Fact]
    public async Task FullCrudRoundTrip_CreatesUpdatesSoftDeletesPersona()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/personas", new CreatePersonaCommand(
            Name: "Mia Chen",
            Description: "Beauty & skincare KOC",
            ToneOfVoice: "Warm, specific",
            TargetAudience: "Gen Z skincare enthusiasts",
            Platform: Platform.TikTok,
            DefaultAiProvider: null,
            SystemPromptTemplate: "You are Mia, 26, Shanghai-based esthetician."));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var personaId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        personaId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/personas/{personaId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persona = await getResponse.Content.ReadFromJsonAsync<PersonaDto>();
        persona!.Name.Should().Be("Mia Chen");

        var updateResponse = await client.PutAsJsonAsync($"/api/personas/{personaId}", new UpdatePersonaCommand(
            Id: personaId,
            Name: "Mia Chen (updated)",
            Description: persona.Description,
            ToneOfVoice: persona.ToneOfVoice,
            TargetAudience: persona.TargetAudience,
            Platform: persona.Platform,
            DefaultAiProvider: persona.DefaultAiProvider,
            SystemPromptTemplate: persona.SystemPromptTemplate,
            IsActive: persona.IsActive));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getUpdatedResponse = await client.GetAsync($"/api/personas/{personaId}");
        var updatedPersona = await getUpdatedResponse.Content.ReadFromJsonAsync<PersonaDto>();
        updatedPersona!.Name.Should().Be("Mia Chen (updated)");

        var deleteResponse = await client.DeleteAsync($"/api/personas/{personaId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Soft-deleted: no longer reachable by id (query filter excludes it) ...
        var getDeletedResponse = await client.GetAsync($"/api/personas/{personaId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // ... and no longer appears in the list.
        var listResponse = await client.GetAsync("/api/personas?searchTerm=Mia Chen");
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<PersonaDto>>();
        page!.Items.Should().NotContain(p => p.Id == personaId);
    }
}
