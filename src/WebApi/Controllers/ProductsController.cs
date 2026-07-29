using AiKocStudio.Application.Common.Models;
using AiKocStudio.Application.Products.Commands.CreateProduct;
using AiKocStudio.Application.Products.Commands.DeleteProduct;
using AiKocStudio.Application.Products.Commands.UpdateProduct;
using AiKocStudio.Application.Products.Commands.UploadProductImage;
using AiKocStudio.Application.Products.Queries.GetProductById;
using AiKocStudio.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiKocStudio.WebApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryFilter = null,
        [FromQuery] bool? isActiveFilter = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetProductsQuery(pageNumber, pageSize, searchTerm, categoryFilter, isActiveFilter),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProductByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("Route id does not match body id.");
        }

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(AllowedImageContentTypes.MaxSizeBytes)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        if (file.Length > AllowedImageContentTypes.MaxSizeBytes)
        {
            return BadRequest($"File exceeds the {AllowedImageContentTypes.MaxSizeBytes} byte limit.");
        }

        await using var stream = file.OpenReadStream();
        var url = await mediator.Send(
            new UploadProductImageCommand(id, stream, file.FileName, file.ContentType),
            cancellationToken);

        return Ok(new { url });
    }
}
