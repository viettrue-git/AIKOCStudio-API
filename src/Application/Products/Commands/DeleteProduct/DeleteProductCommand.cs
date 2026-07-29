using MediatR;

namespace AiKocStudio.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest;
