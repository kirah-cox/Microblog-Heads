using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class CommentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly NotificationService _notifications;

    public CommentService(IDbContextFactory<AppDbContext> dbFactory, NotificationService notifications)
    {
        _dbFactory = dbFactory;
        _notifications = notifications;
    }

    public async Task<Comment?> GetCommentByIdAsync(int commentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Comments.FindAsync(commentId);
    }

    /// <summary>
    /// Deletes a comment. Allowed for the comment's author, site admins, and admins/moderators of
    /// the community the post is in. The database's cascade deletes remove its replies, likes,
    /// reports and notifications.
    /// </summary>
    public async Task DeleteCommentAsync(int userId, int commentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var comment = await db.Comments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment is null)
            throw new InvalidOperationException("That comment no longer exists.");

        if (comment.RegularUserId != userId)
        {
            var isSiteAdmin = await db.RegularUsers
                .AnyAsync(u => u.Id == userId && u.UserRoleID == UserRole.AdminRoleId);
            var isStaff = comment.Post.CommunityId is int cid
                && await db.CommunityUserRoles.AnyAsync(r => r.CommunityId == cid
                    && r.RegularUserId == userId
                    && (r.CommunityRole.Role == "Admin" || r.CommunityRole.Role == "Moderator"));

            if (!isSiteAdmin && !isStaff)
                throw new InvalidOperationException("You can only delete your own comments.");
        }

        // The comment and every reply under it (the cascade removes those too).
        var postComments = await db.Comments
            .Where(c => c.PostId == comment.PostId)
            .Select(c => new { c.Id, c.ParentCommentId })
            .ToListAsync();
        var deletedIds = new HashSet<int> { comment.Id };
        bool added;
        do
        {
            added = false;
            foreach (var c in postComments)
            {
                if (c.ParentCommentId is int parentId && deletedIds.Contains(parentId) && deletedIds.Add(c.Id))
                    added = true;
            }
        } while (added);

        await _notifications.NotifyForDeletedCommentsAsync(db, deletedIds);
        db.Comments.Remove(comment);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not delete the comment.", ex);
        }
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
            DatePosted = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), // column is "timestamp without time zone"; Npgsql rejects Kind=Utc
            // CommunityId = communityId // Comment doesn't have a community field
            PostId = postId,
            ParentCommentId = commentId
        };
        try 
        {
            db.Comments.Add(comment);
            await _notifications.AddForNewCommentAsync(db, comment);
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
            await _notifications.RemoveForCommentUnlikeAsync(db, userId, commentId);
            await db.SaveChangesAsync();

            return false; // if a like exists, we are 'unliking'the post
        }

        var like = new CommentLike
        {
            RegularUserId = userId,
            CommentId = commentId
        };

        db.CommentLikes.Add(like);
        await _notifications.AddForCommentLikeAsync(db, userId, commentId);

        await db.SaveChangesAsync();

        return true;
    }

    public async Task<List<int>> GetLikedCommentIdsByUserIdAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CommentLikes
            .Where(l => l.RegularUserId == userId)
            .Select(l => l.CommentId)
            .ToListAsync();
    }
}

