namespace AiKocStudio.Application.Common.Models;

public record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);
