using Identity.Application.Interfaces;
using Identity.Domain;
using System.Data;
using System.Data.Common;

namespace Identity.Infrastructure;

public class ApplicationUserRepo : IApplicationUserRepo
{
    private readonly IDbConnection _connection;

    public ApplicationUserRepo(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<ApplicationUser?> FirstOrDefaultAsync(string email)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Id, Email, PasswordHash, Salt FROM ApplicationUser WHERE Email = @Email";

        var emailParam = command.CreateParameter();
        emailParam.ParameterName = "@Email";
        emailParam.Value = email;
        command.Parameters.Add(emailParam);

        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await using var reader = await ((DbCommand)command).ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ApplicationUser
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Salt = reader.GetString(3)
            };
        }

        return null;
    }

    public async Task<int> SaveAsync(ApplicationUser applicationUser)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"INSERT INTO ApplicationUser (Email, PasswordHash, Salt) 
                                    VALUES (@Email, @Password, @Salt);";

        var emailParam = command.CreateParameter();
        emailParam.ParameterName = "@Email";
        emailParam.Value = applicationUser.Email;
        command.Parameters.Add(emailParam);

        var passwordParam = command.CreateParameter();
        passwordParam.ParameterName = "@Password";
        passwordParam.Value = applicationUser.PasswordHash;
        command.Parameters.Add(passwordParam);

        var saltParam = command.CreateParameter();
        saltParam.ParameterName = "@Salt";
        saltParam.Value = applicationUser.Salt;
        command.Parameters.Add(saltParam);

        if (_connection.State == ConnectionState.Closed)
        {
            await ((DbConnection)_connection).OpenAsync();
        }

        await ((DbCommand)command).ExecuteNonQueryAsync();

        var insertedUser = await FirstOrDefaultAsync(applicationUser.Email);
        return insertedUser!.Id;
    }
}