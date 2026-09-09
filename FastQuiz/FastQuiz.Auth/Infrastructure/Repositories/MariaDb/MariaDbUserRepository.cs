using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Infrastructure.Repositories.MariaDb;

public class MariaDbUserRepository(MariaDbAuthDbContext _dbContext) : IUserRepository
{
    private readonly DbSet<User> Users = _dbContext.Users;

    public async Task CreateAsync(User user)
    {
        try
        {
            await Users.AddAsync(user).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Create user fail.", ex);
        }
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email)
            .ConfigureAwait(false);
    }

    public async Task<User?> FindByIdAsync(string id)
    {
        return await Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<User>> GetUsersByIdsAsync(IEnumerable<string> ids)
    {
        return await Users.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user)
    {
        await Users
            .Where(x => x.Id == user.Id)
            .ExecuteUpdateAsync(u =>
                u.SetProperty(x => x.Username, user.Username)
                    .SetProperty(x => x.PasswordHash, user.PasswordHash)
                    .SetProperty(x => x.Email, user.Email)
            );
    }

    public async Task DeleteByIdAsync(string userId)
    {
        await Users.Where(x => x.Id == userId).ExecuteDeleteAsync();
    }
}
