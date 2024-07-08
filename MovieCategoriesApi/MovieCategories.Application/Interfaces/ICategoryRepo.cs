using MovieCategories.Domain;

namespace MovieCategories.Application.Interfaces;

public interface ICategoryRepo
{
    Task<IEnumerable<MovieCategory>> GetAllAsync();
    Task<MovieCategory?> GetByIdAsync(int id);
    Task<MovieCategory?> GetByNameAsync(string categoryName);
    Task<int> CreateAsync(MovieCategory category);
    Task<int> UpdateAsync(MovieCategory category);
    Task DeleteAsync(int id);
}