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
    Task UpdateProfileAsync(User user);
    Task<bool> TryChangePasswordAsync(string userId, string previousHash, string newHash);
    Task DeleteByIdAsync(string userId);
}
