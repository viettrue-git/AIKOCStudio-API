using AiKocStudio.Application.Common.Models;
using FluentValidation;

namespace AiKocStudio.Application.Products.Queries.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationDefaults.MaxPageSize);
    }
}
