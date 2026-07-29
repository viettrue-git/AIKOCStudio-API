using AiKocStudio.Application.Common.Models;
using AiKocStudio.Domain.Enums;
using MediatR;

namespace AiKocStudio.Application.Personas.Queries.GetPersonas;

public record GetPersonasQuery(
    int PageNumber = 1,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SearchTerm = null,
    Platform? PlatformFilter = null,
    bool? IsActiveFilter = null) : IRequest<PagedResult<PersonaDto>>;
