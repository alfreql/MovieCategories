namespace Identity.Application.Interfaces;

public interface IPasswordHasher
{
    public (string hashedPassword, string salt) HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string salt, string passwordToCheck);

}