using AiKocStudio.Application.Common.Models;
using FluentValidation;

namespace AiKocStudio.Application.Personas.Queries.GetPersonas;

public class GetPersonasQueryValidator : AbstractValidator<GetPersonasQuery>
{
    public GetPersonasQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationDefaults.MaxPageSize);
    }
}
