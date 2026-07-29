using System.Net;
using System.Net.Http.Json;
using AiKocStudio.Application.Auth.Commands.Login;
using AiKocStudio.Application.Common.Models;
using AiKocStudio.Application.Personas.Commands.CreatePersona;
using AiKocStudio.Application.Products;
using AiKocStudio.Application.Products.Commands.CreateProduct;
using AiKocStudio.Application.Products.Commands.UpdateProduct;
using AiKocStudio.Domain.Enums;
using FluentAssertions;

namespace AiKocStudio.Application.FunctionalTests.Products;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task FullCrudRoundTrip_CreatesUpdatesSoftDeletesProduct()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/products", new CreateProductCommand(
            Name: "Aurora Serum",
            Description: "Brightening vitamin C serum",
            Category: "Skincare",
            KeyFeatures: ["Vitamin C", "Fragrance-free"],
            TargetPersonaId: null));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/products/{productId}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product!.Name.Should().Be("Aurora Serum");

        var updateResponse = await client.PutAsJsonAsync($"/api/products/{productId}", new UpdateProductCommand(
            Id: productId,
            Name: "Aurora Serum (updated)",
            Description: product.Description,
            Category: product.Category,
            KeyFeatures: product.KeyFeatures,
            TargetPersonaId: product.TargetPersonaId,
            IsActive: product.IsActive));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeletedResponse = await client.GetAsync($"/api/products/{productId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletingLinkedPersona_NullsOutTargetPersonaId_OnReferencingProduct()
    {
        var client = await CreateAuthenticatedClientAsync();

        var personaResponse = await client.PostAsJsonAsync("/api/personas", new CreatePersonaCommand(
            Name: "Mia Chen",
            Description: "Beauty & skincare KOC",
            ToneOfVoice: "Warm",
            TargetAudience: "Gen Z",
            Platform: Platform.TikTok,
            DefaultAiProvider: null,
            SystemPromptTemplate: "You are Mia."));
        var personaId = await personaResponse.Content.ReadFromJsonAsync<Guid>();

        var productResponse = await client.PostAsJsonAsync("/api/products", new CreateProductCommand(
            Name: "Aurora Serum",
            Description: "Brightening vitamin C serum",
            Category: "Skincare",
            KeyFeatures: ["Vitamin C"],
            TargetPersonaId: personaId));
        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        await client.DeleteAsync($"/api/personas/{personaId}");

        var getProductResponse = await client.GetAsync($"/api/products/{productId}");
        var product = await getProductResponse.Content.ReadFromJsonAsync<ProductDto>();

        product!.TargetPersonaId.Should().BeNull(
            "the linked Persona was soft-deleted, which should null the FK rather than leaving the Product pointing at an invisible row");
    }
}
