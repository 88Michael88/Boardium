using Boardium.Models.Game;

namespace Boardium.Areas.Admin.Services;

public interface IPublishersService
{
    Task<List<Publisher>> GetAllAsync();

    Task<Publisher?> GetByIdAsync(int id);

    Task<bool> CreateAsync(Publisher publisher);

    Task<bool> UpdateAsync(Publisher publisher);

    Task<bool> DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);
}