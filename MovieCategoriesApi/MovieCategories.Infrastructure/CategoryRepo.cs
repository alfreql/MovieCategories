using System.Data;
using System.Data.Common;
using MovieCategories.Application.Interfaces;
using MovieCategories.Domain;

namespace MovieCategories.Infrastructure;

public class CategoryRepo : ICategoryRepo
{
    private readonly IDbConnection _connection;

    public CategoryRepo(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<IEnumerable<MovieCategory>> GetAllAsync()
    {
        var categories = new List<MovieCategory>();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Category, Description FROM MovieCategory";
        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await using var reader = await ((DbCommand)cmd).ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new MovieCategory
            {
                Id = reader.GetInt32(0),
                Category = reader.GetString(1),
                Description = reader.GetString(2)
            });
        }

        return categories;
    }

    public async Task<MovieCategory?> GetByIdAsync(int id)
    {
        MovieCategory? category = null;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Category, Description FROM MovieCategory WHERE Id = @Id";
        var idParam = cmd.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = id;
        cmd.Parameters.Add(idParam);
        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await using var reader = await ((DbCommand)cmd).ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            category = new MovieCategory
            {
                Id = reader.GetInt32(0),
                Category = reader.GetString(1),
                Description = reader.GetString(2)
            };
        }

        return category;
    }

    public async Task<MovieCategory?> GetByNameAsync(string categoryName)
    {
        MovieCategory? category = null;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Category, Description FROM MovieCategory WHERE Category = @Category";
        var idParam = cmd.CreateParameter();
        idParam.ParameterName = "@Category";
        idParam.Value = categoryName;
        cmd.Parameters.Add(idParam);
        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await using var reader = await ((DbCommand)cmd).ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            category = new MovieCategory
            {
                Id = reader.GetInt32(0),
                Category = reader.GetString(1),
                Description = reader.GetString(2)
            };
        }

        return category;
    }

    public async Task<int> CreateAsync(MovieCategory category)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT INTO MovieCategory (Category, Description) VALUES (@Category, @Description)";
        var categoryParam = cmd.CreateParameter();
        categoryParam.ParameterName = "@Category";
        categoryParam.Value = category.Category;
        var descriptionParam = cmd.CreateParameter();
        descriptionParam.ParameterName = "@Description";
        descriptionParam.Value = category.Description;
        cmd.Parameters.Add(categoryParam);
        cmd.Parameters.Add(descriptionParam);
        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await ((DbCommand)cmd).ExecuteNonQueryAsync();

        var insertedCategory = await GetByNameAsync(category.Category);

        return insertedCategory!.Id;
    }

    public async Task<int> UpdateAsync(MovieCategory category)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "UPDATE MovieCategory SET Category = @Category, Description = @Description WHERE Id = @Id";
        var idParam = cmd.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = category.Id;
        var categoryParam = cmd.CreateParameter();
        categoryParam.ParameterName = "@Category";
        categoryParam.Value = category.Category;
        var descriptionParam = cmd.CreateParameter();
        descriptionParam.ParameterName = "@Description";
        descriptionParam.Value = category.Description;
        cmd.Parameters.Add(idParam);
        cmd.Parameters.Add(categoryParam);
        cmd.Parameters.Add(descriptionParam);
        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        return await ((DbCommand)cmd).ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM MovieCategory WHERE Id = @Id";
        var idParam = cmd.CreateParameter();
        idParam.ParameterName = "@Id";
        idParam.Value = id;
        cmd.Parameters.Add(idParam);
        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await ((DbCommand)cmd).ExecuteNonQueryAsync();
    }
}

