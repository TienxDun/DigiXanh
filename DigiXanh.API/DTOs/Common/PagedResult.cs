namespace DigiXanh.API.DTOs.Common;

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
