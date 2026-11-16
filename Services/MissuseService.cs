using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace microblog_heads.Services;

public class MissuseService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public MissuseService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    //Report a pot for missuse
    public async Task<PostMissuse> ReportPostAsync(int userId, int postId, string missuseName, string description)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var report = new PostMissuse
        {
            RegularUserId = userId,
            PostId = postId,
            MissuseName = missuseName,
            MissuseDescription = description
        };

        db.PostMissuses.Add(report);
        await db.SaveChangesAsync();
        return report;
    }

    //Report a comment for missuse
    public async Task<CommentMissuse> ReportCommentAsync(int userId, int commentId, string missuseName, string description)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var report = new CommentMissuse
        {
            RegularUserId = userId,
            CommentId = commentId,
            MissuseName = missuseName,
            MissuseDescription = description
        };

        db.CommentMissuses.Add(report);
        await db.SaveChangesAsync();
        return report;
    }

    //Get all pot missue reports
    public async Task<List<PostMissuse>> GetAllPostReportsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PostMissuses
            .Include(p => p.Post)
            .Include(p => p.RegularUser)
            .ToListAsync();
    }

    // Get all comment missuse reports
    public async Task<List<CommentMissuse>> GetAllCommentReportsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CommentMissuses
            
            .Include(c => c.Comment)
            .Include(c => c.RegularUser)
            .ToListAsync();
    }

    //get grouped post reports
    public async Task<List<GroupedPostReport>> GetGroupedPostReportsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var reports = await db.PostMissuses
            .Where(r => r.Post.CommunityId == null)
            .Include(p => p.Post).ThenInclude(p => p.RegularUser)
            .Include(p => p.RegularUser)
            .ToListAsync();

        return reports
            .GroupBy(r => r.PostId)
            .Select(g => new GroupedPostReport
            {
                Post = g.First().Post,
                Reports = g.ToList()
            })
            .ToList();
    }

    // Get grouped comment reports
    public async Task<List<GroupedCommentReport>> GetGroupedCommentReportsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var reports = await db.CommentMissuses
            .Where(r => r.Comment.Post.CommunityId == null)
            .Include(c => c.Comment).ThenInclude(c => c.RegularUser)
            .Include(c => c.RegularUser)
            .ToListAsync();

        return reports
            .GroupBy(r => r.CommentId)
            .Select(g => new GroupedCommentReport
            {
                Comment = g.First().Comment,
                Reports = g.ToList()
            })
            .ToList();
    }

    //Delete all reports for a post
    public async Task DeletePostReportsAsync(int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var reports = await db.PostMissuses.Where(r => r.PostId == postId).ToListAsync();
        db.PostMissuses.RemoveRange(reports);
        await db.SaveChangesAsync();
    }

    //delete all reports for a comment
    public async Task DeleteCommentReportsAsync(int commentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var reports = await db.CommentMissuses.Where(r => r.CommentId == commentId).ToListAsync();
        db.CommentMissuses.RemoveRange(reports);
        await db.SaveChangesAsync();
    }
    private static Task<bool> IsCommunityStaffAsync(AppDbContext db, int userId, int communityId) =>
    db.CommunityUserRoles.AnyAsync(r => r.CommunityId == communityId
        && r.RegularUserId == userId
        && (r.CommunityRole.Role == "Admin" || r.CommunityRole.Role == "Moderator"));

    public async Task<List<GroupedPostReport>> GetGroupedPostReportsForCommunityAsync(int actingUserId, int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        if (!await IsCommunityStaffAsync(db, actingUserId, communityId))
            throw new InvalidOperationException("Only an admin or moderator can see this community's reports.");

        var reports = await db.PostMissuses
            .Where(r => r.Post.CommunityId == communityId)
            .Include(r => r.Post).ThenInclude(p => p.RegularUser)
            .Include(r => r.RegularUser)
            .ToListAsync();

        return reports
            .GroupBy(r => r.PostId)
            .Select(g => new GroupedPostReport { Post = g.First().Post, Reports = g.ToList() })
            .ToList();
    }

    public async Task<List<GroupedCommentReport>> GetGroupedCommentReportsForCommunityAsync(int actingUserId, int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        if (!await IsCommunityStaffAsync(db, actingUserId, communityId))
            throw new InvalidOperationException("Only an admin or moderator can see this community's reports.");

        var reports = await db.CommentMissuses
            .Where(r => r.Comment.Post.CommunityId == communityId)
            .Include(r => r.Comment).ThenInclude(c => c.RegularUser)
            .Include(r => r.RegularUser)
            .ToListAsync();

        return reports
            .GroupBy(r => r.CommentId)
            .Select(g => new GroupedCommentReport { Comment = g.First().Comment, Reports = g.ToList() })
            .ToList();
    }

    public async Task DismissCommunityPostReportsAsync(int actingUserId, int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var communityId = await db.Posts.Where(p => p.Id == postId).Select(p => p.CommunityId).FirstOrDefaultAsync();
        if (communityId is not int cid)
            throw new InvalidOperationException("That post doesn't belong to a community.");
        if (!await IsCommunityStaffAsync(db, actingUserId, cid))
            throw new InvalidOperationException("Only an admin or moderator can dismiss reports.");

        db.PostMissuses.RemoveRange(await db.PostMissuses.Where(r => r.PostId == postId).ToListAsync());
        await db.SaveChangesAsync();
    }

    public async Task DismissCommunityCommentReportsAsync(int actingUserId, int commentId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var communityId = await db.Comments.Where(c => c.Id == commentId)
            .Select(c => c.Post.CommunityId).FirstOrDefaultAsync();
        if (communityId is not int cid)
            throw new InvalidOperationException("That comment doesn't belong to a community.");
        if (!await IsCommunityStaffAsync(db, actingUserId, cid))
            throw new InvalidOperationException("Only an admin or moderator can dismiss reports.");

        db.CommentMissuses.RemoveRange(await db.CommentMissuses.Where(r => r.CommentId == commentId).ToListAsync());
        await db.SaveChangesAsync();
    }
}