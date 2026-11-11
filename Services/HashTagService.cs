using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class HashTagService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public HashTagService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<HashTag?> GetHashTagByIdAsync(int hashTagId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.HashTags.FindAsync(hashTagId);
    }

    public async Task<List<HashTag>> GetAllHashTagsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.HashTags.ToListAsync();
    }

    public async Task FollowHashTagAsync(int userId, int hashTagId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var user = await db.RegularUsers.FindAsync(userId);
        var hashTag = await db.HashTags.FindAsync(hashTagId);

        if (user == null || hashTag == null)
        {
            throw new Exception("User or hashtag not found.");
        }

        var newFollow = new HashTagFollow
        {
            RegularUserId = userId,
            HashTagId = hashTagId
        };

        try
        {
            db.HashTagFollows.Add(newFollow);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while following the hashtag.", ex);
        }
    }

    public async Task UnfollowHashTagAsync(int userId, int hashTagId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var follow = await db.HashTagFollows.FirstOrDefaultAsync(f => f.RegularUserId == userId && f.HashTagId == hashTagId);

        if (follow == null)
        {
            throw new Exception("Follow relationship not found.");
        }

        try
        {
            db.HashTagFollows.Remove(follow);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while unfollowing the hashtag.", ex);
        }
    }

    public async Task<List<HashTag>> GetFollowedHashTagsByUserIdAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.HashTagFollows
            .Where(f => f.RegularUserId == userId)
            .Select(f => f.HashTag)
            .ToListAsync();
    }
}