using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace microblog_heads.Services;

public class UserService
{
    private const string UniqueEmailIndex = "uq_regular_user_email_ci";

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

    public async Task CreateUserAccountAsync(string firstName, string lastName, string email, string username, string password)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // Usernames and emails are unique case-insensitively (see the
        // uq_regular_user_*_ci indexes); check up front for a friendly message.
        var usernameLower = username.ToLower();
        var emailLower = email.ToLower();

        if (await db.RegularUsers.AnyAsync(u => u.Username.ToLower() == usernameLower))
        {
            throw new Exception("That username is already taken.");
        }
        if (await db.RegularUsers.AnyAsync(u => u.Email.ToLower() == emailLower))
        {
            throw new Exception("An account with that email already exists.");
        }

        var user = new RegularUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Username = username,
            Password = password,
            UserRoleID = UserRole.UserRoleId
        };

        try
        {
            db.RegularUsers.Add(user);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            // Someone else registered the same username/email between the check and the insert.
            throw new Exception(pg.ConstraintName == UniqueEmailIndex
                ? "An account with that email already exists."
                : "That username is already taken.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the user.", ex);
        }
    }

    public async Task<RegularUser> RegularUserLoginAsync(string username, string password)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.RegularUsers.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            throw new Exception("Invalid username or password.");
        }
        return user;
    }

    public async Task FollowUserAsync(int followerId, int followingId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var follower = await db.RegularUsers.FindAsync(followerId);
        var following = await db.RegularUsers.FindAsync(followingId);

        if (follower == null || following == null)
        {
            throw new Exception("Follower or followee not found.");
        }

        var newFollower = new Follower
        {
            RegularUserFollowerId = followerId,
            RegularUserFollowingId = followingId
        };

        try
        {
            db.Followers.Add(newFollower);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the follower.", ex);
        }
    }

    public async Task UnfollowUserAsync(int followerId, int followingId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var follower = await db.Followers.FirstOrDefaultAsync(f => f.RegularUserFollowerId == followerId && f.RegularUserFollowingId == followingId);

        if (follower == null)
        {
            throw new Exception("Follower relationship not found.");
        }

        try
        {
            db.Followers.Remove(follower);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while removing the follower.", ex);
        }
    }

    public async Task<List<RegularUser>> GetFollowersAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Followers
            .Where(f => f.RegularUserFollowingId == userId)
            .Select(f => f.RegularUserFollower)
            .ToListAsync();
    }

    public async Task<List<RegularUser>> GetFollowingAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Followers
            .Where(f => f.RegularUserFollowerId == userId)
            .Select(f => f.RegularUserFollowing)
            .ToListAsync();
    }

    public async Task BlockUserAsync(int blockingId, int blockedId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var follower = await db.RegularUsers.FindAsync(blockingId);
        var following = await db.RegularUsers.FindAsync(blockedId);

        if (follower == null || following == null)
        {
            throw new Exception("Follower or followee not found.");
        }

        var newBlocked = new BlockedUser
        {
            RegularUserBlockingId = blockingId,
            RegularUserBlockedId = blockedId
        };

        try
        {
            db.BlockedUsers.Add(newBlocked);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while adding the blocked user.", ex);
        }
    }

    public async Task UnblockUserAsync(int blockingId, int blockedId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var blockedUser = await db.BlockedUsers.FirstOrDefaultAsync(b => b.RegularUserBlockingId == blockingId && b.RegularUserBlockedId == blockedId);

        if (blockedUser == null)
        {
            throw new Exception("Blocked user relationship not found.");
        }

        try
        {
            db.BlockedUsers.Remove(blockedUser);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while removing the blocked user.", ex);
        }
    }

    public async Task<List<RegularUser>> GetBlockedUsersAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.BlockedUsers
            .Where(b => b.RegularUserBlockingId == userId)
            .Select(b => b.RegularUserBlocked)
            .ToListAsync();
    }
}
