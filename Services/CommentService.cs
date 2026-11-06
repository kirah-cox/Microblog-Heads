using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class CommentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CommentService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Comment?> GetCommentByIdAsync(int commentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Comments.FindAsync(commentId);
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Comments
            .Where(c => c.PostId == postId)
            .ToListAsync();
    }
}
