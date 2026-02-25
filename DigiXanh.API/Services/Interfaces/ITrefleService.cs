using DigiXanh.API.DTOs.Trefle;

namespace DigiXanh.API.Services.Interfaces;

public interface ITrefleService
{
    Task<IReadOnlyCollection<TreflePlantSearchItemDto>> SearchPlantsAsync(string query, CancellationToken cancellationToken = default);
    Task<TreflePlantDetailDto?> GetPlantDetailAsync(int id, CancellationToken cancellationToken = default);
}
