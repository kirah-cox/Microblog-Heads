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
}
