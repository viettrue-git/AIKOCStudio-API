using MediatR;

namespace AiKocStudio.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    string Category,
    List<string> KeyFeatures,
    Guid? TargetPersonaId,
    bool IsActive) : IRequest;
