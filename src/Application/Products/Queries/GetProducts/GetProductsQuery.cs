using AiKocStudio.Application.Common.Models;
using MediatR;

namespace AiKocStudio.Application.Products.Queries.GetProducts;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SearchTerm = null,
    string? CategoryFilter = null,
    bool? IsActiveFilter = null) : IRequest<PagedResult<ProductDto>>;
