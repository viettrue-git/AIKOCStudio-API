using MediatR;

namespace AiKocStudio.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    string Category,
    List<string> KeyFeatures,
    Guid? TargetPersonaId) : IRequest<Guid>;
