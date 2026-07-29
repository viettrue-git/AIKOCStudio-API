using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.IsDeleted = true;
        product.DeletedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
