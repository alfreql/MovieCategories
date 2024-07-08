using MovieCategories.Domain;

namespace MovieCategories.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<MovieCategory>> GetAllAsync();
    Task<MovieCategory?> GetByIdAsync(int id);
    Task<int> CreateAsync(MovieCategory category);
    Task<int> UpdateAsync(MovieCategory category);
    Task DeleteAsync(int id);

}