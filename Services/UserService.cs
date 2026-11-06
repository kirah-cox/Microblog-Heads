using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class UserService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public UserService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<RegularUser?> GetUserByIdAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.RegularUsers.FindAsync(userId);
    }

    public async Task<RegularUser?> GetUserByUsernameAsync(string username)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.RegularUsers.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<RegularUser>> GetAllUsersAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.RegularUsers.ToListAsync();
    }

    public async Task AddUserAsync(string firstName, string lastName, string email, string username, string password)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var user = new RegularUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Username = username,
            Password = password
        };

        try
        {
            db.RegularUsers.Add(user);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the user.", ex);
        }
    }
}
