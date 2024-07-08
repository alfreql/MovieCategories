using Identity.Application.Interfaces;
using Identity.Domain;

namespace Identity.Application.User;

public class ApplicationUserService : IApplicationUserService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IApplicationUserRepo _userRepo;

    public ApplicationUserService(IPasswordHasher passwordHasher, IApplicationUserRepo userRepo)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    }

    public async Task<int> CreateAsync(string email, string password)
    {
        if (await _userRepo.FirstOrDefaultAsync(email) is not null)
        {
            throw new CustomException("Email already in use.", 409);
        }

        var (hashedPassword, salt) = _passwordHasher.HashPassword(password);
        return await _userRepo.SaveAsync(new ApplicationUser { Email = email, PasswordHash = hashedPassword, Salt = salt });
    }
}