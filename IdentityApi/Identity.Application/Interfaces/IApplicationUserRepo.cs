using Identity.Domain;

namespace Identity.Application.Interfaces;

public interface IApplicationUserRepo
{
    Task<ApplicationUser?> FirstOrDefaultAsync(string email);
    Task<int> SaveAsync(ApplicationUser applicationUser);
}