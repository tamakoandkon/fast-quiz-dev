using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Domain.Repositories;

public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<User?> FindByIdAsync(string id);
    Task<IReadOnlyList<User>> GetUsersByIdsAsync(IEnumerable<string> ids);
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByEmailAsync(string email);
    Task UpdateAsync(User user);
    Task DeleteByIdAsync(string userId);
}
