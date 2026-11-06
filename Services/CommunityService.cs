using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class CommunityService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CommunityService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Community?> GetCommunityByIdAsync(int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Communities.FindAsync(communityId);
    }

    public async Task<List<Community>> GetAllCommunitiesAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Communities.ToListAsync();
    }
}
