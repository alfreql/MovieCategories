using MovieCategories.Application.Interfaces;
using MovieCategories.Domain;

namespace MovieCategories.Application.Category;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepo _repository;

    public CategoryService(ICategoryRepo repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IEnumerable<MovieCategory>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public async Task<MovieCategory?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(MovieCategory category)
    {
        var exist = await _repository.GetByNameAsync(category.Category);
        if (exist is not null)
        {
            throw new CustomException($"Category '{category.Category}' already exist", 409);
        }
        return await _repository.CreateAsync(category);
    }

    public async Task<int> UpdateAsync(MovieCategory category)
    {
        var existedName = await _repository.GetByNameAsync(category.Category);
        if (existedName is not null && existedName.Id != category.Id)
        {
            throw new CustomException($"Category '{category.Category}' already exist", 409);
        }
        return await _repository.UpdateAsync(category);
    }

    public Task DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}