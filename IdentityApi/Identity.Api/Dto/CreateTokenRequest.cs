namespace Identity.Api.Dto;

public class CreateTokenRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}