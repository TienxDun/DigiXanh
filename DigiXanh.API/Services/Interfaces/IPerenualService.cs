using DigiXanh.API.DTOs.Perenual;

namespace DigiXanh.API.Services.Interfaces;

public interface IPerenualService
{
    Task<IReadOnlyCollection<PerenualPlantSearchItemDto>> SearchPlantsAsync(string query, CancellationToken cancellationToken = default);
    Task<PerenualPlantDetailDto?> GetPlantDetailAsync(int id, CancellationToken cancellationToken = default);
}
