using AiKocStudio.Domain.Entities;

namespace AiKocStudio.Application.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    List<string> KeyFeatures,
    Guid? TargetPersonaId,
    string? ImageUrl,
    bool IsActive)
{
    public static ProductDto FromEntity(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Category,
        product.KeyFeatures,
        product.TargetPersonaId,
        product.ImageUrl,
        product.IsActive);
}
