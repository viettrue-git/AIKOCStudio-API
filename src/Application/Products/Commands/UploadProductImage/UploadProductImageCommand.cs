using MediatR;

namespace AiKocStudio.Application.Products.Commands.UploadProductImage;

public record UploadProductImageCommand(Guid ProductId, Stream Content, string FileName, string ContentType) : IRequest<string>;
