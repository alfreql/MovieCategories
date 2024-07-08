namespace Identity.Application.Interfaces;

public interface ITokenGenerator
{
    public Task<(string token, DateTime expire)> GenerateTokenAsync(string email, string password);
}