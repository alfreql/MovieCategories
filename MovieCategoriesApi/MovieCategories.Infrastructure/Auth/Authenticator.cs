using MovieCategories.Domain;
using System.Text.Json;
using System.Text;

namespace MovieCategories.Infrastructure.Auth;

public class Authenticator : IAuthenticator
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public Authenticator(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<TokenResponse?> AuthenticateAsync(string email, string password)
    {
        try
        {
            var jsonPayload = JsonSerializer.Serialize(new { Email = email, Password = password });
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync("/token", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new CustomException($"Error consuming API. StatusCode: {response.StatusCode}", (int)response.StatusCode);
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<TokenResponse>(responseStream, Options);
        }
        catch (Exception e) when (e is not CustomException)
        {
            throw new CustomException($"Error consuming API. Exception: {e}", e);
        }
    }
}