using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class PostService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public PostService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Post?> GetPostByIdAsync(int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Posts.FindAsync(postId);
    }

    public async Task<List<Post>> GetPostsByUserIdAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Posts
            .Where(p => p.RegularUserId == userId)
            .ToListAsync();
    }
}
