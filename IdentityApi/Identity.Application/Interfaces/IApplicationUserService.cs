namespace Identity.Application.Interfaces;

public interface IApplicationUserService
{
    Task<int> CreateAsync(string email, string password);
}