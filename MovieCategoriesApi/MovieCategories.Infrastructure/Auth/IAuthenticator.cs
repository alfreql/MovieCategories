namespace MovieCategories.Infrastructure.Auth;

public interface IAuthenticator
{
    Task<TokenResponse?> AuthenticateAsync(string user, string password);
}