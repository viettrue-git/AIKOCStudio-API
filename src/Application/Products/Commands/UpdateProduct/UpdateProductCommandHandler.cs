using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.KeyFeatures = request.KeyFeatures;
        product.TargetPersonaId = request.TargetPersonaId;
        product.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);
    }
}
