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

    public async Task<Comment> CreateCommentAsync (int userId, int postId, string content, int? commentId)
    {
        content = content?.Trim() ?? ""; // optional, we may wish to keep white space in later versions
        if (content.Length == 0) 
            throw new ArgumentException("A comment cannot be empty");
        if (content.Length > 300) 
            throw new ArgumentException("A comment cannot exceed 300 characters");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var comment = new Comment
        {
            RegularUserId = userId,
            Content = content,
            // DatePosted = DateOnly.FromDateTime(DateTime.UtcNow), // Comment doesn't have a date field currently
            // CommunityId = communityId // Comment doesn't have a community field
            PostId = postId,
            ParentCommentId = commentId
        };
        try 
        {
            db.Comments.Add(comment);
            await db.SaveChangesAsync();    
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not save the comment. Does user exist?", ex);
        }
        return comment;
    }

    public async Task<bool> ToggleLikeCommentAsync (int userId, int commentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var existing = await db.CommentLikes.FirstOrDefaultAsync(l => l.RegularUserId == userId && l.CommentId == commentId);
        if (existing is not null)
        {
            db.CommentLikes.Remove(existing);
            await db.SaveChangesAsync();

            return false; // if a like exists, we are 'unliking'the post
        }

        var like = new CommentLike
        {
            RegularUserId = userId,
            CommentId = commentId
        };

        db.CommentLikes.Add(like);
                   
        await db.SaveChangesAsync();

        return true;
    }
}

