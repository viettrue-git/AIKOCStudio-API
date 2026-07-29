using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Products.Commands.UploadProductImage;

public class UploadProductImageCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorageService) : IRequestHandler<UploadProductImageCommand, string>
{
    public async Task<string> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var url = await fileStorageService.UploadAsync(request.Content, request.FileName, request.ContentType, cancellationToken);

        product.ImageUrl = url;
        await context.SaveChangesAsync(cancellationToken);

        return url;
    }
}
