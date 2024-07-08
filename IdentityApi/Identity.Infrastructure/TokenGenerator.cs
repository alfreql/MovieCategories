using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Identity.Application.Interfaces;
using Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;


namespace Identity.Infrastructure;

public class TokenGenerator : ITokenGenerator
{
    private readonly IApplicationUserRepo _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _tokenLifeTime = TimeSpan.FromHours(1);//change to 1,

    public TokenGenerator(IApplicationUserRepo userRepo, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        if (int.TryParse(_configuration["Jwt:TokenLifeTimeHours"], out var hours))
        {
            _tokenLifeTime = TimeSpan.FromHours(hours);
        }
    }

    public async Task<(string token, DateTime expire)> GenerateTokenAsync(string email, string password)
    {
        var user = await _userRepo.FirstOrDefaultAsync(email);

        if (user == null)
        {
            throw new CustomException("Wrong User or Password", (int)HttpStatusCode.Unauthorized);
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, user.Salt, password))
        {
            throw new CustomException("Wrong User or Password", (int)HttpStatusCode.Unauthorized);
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(_tokenLifeTime),
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
    }
}