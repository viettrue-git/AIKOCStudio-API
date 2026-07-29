using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;

namespace AiKocStudio.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            KeyFeatures = request.KeyFeatures,
            TargetPersonaId = request.TargetPersonaId,
            IsActive = true,
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
