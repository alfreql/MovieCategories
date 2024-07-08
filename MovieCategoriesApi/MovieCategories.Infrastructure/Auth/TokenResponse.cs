namespace MovieCategories.Infrastructure.Auth;

public class TokenResponse
{
    public string Token { get; set; }
    public DateTime ExpireTime { get; set; }
}