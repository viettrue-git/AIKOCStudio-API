using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryFilter))
        {
            query = query.Where(p => p.Category == request.CategoryFilter);
        }

        if (request.IsActiveFilter is not null)
        {
            query = query.Where(p => p.IsActive == request.IsActiveFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(ProductDto.FromEntity).ToList();

        return new PagedResult<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
