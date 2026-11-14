using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

/// <summary>
/// Creates notification rows. Each method takes the caller's AppDbContext and only adds rows to
/// it, so the notifications are saved in the same SaveChangesAsync (same transaction) as the post,
/// comment or like that caused them.
///
/// Rules for every notification:
///   - Nobody is notified about their own action.
///   - Nobody is notified about someone they blocked or who blocked them.
///   - Each person gets at most one notification per post/comment. If several reasons apply, the
///     first in the method's priority list wins (e.g. a follower who is also mentioned gets "mention").
///
/// After a save that adds or removes notifications, NotificationNotifier is told which users were
/// affected so their bells update live. It also has the queries the bell uses.
/// </summary>
public class NotificationService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly NotificationNotifier _notifier;

    public NotificationService(IDbContextFactory<AppDbContext> dbFactory, NotificationNotifier notifier)
    {
        _dbFactory = dbFactory;
        _notifier = notifier;
    }

    // ---------- Reading (used by the bell) ----------

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Notifications.CountAsync(n => n.NotifiedUserId == userId && !n.IsRead);
    }

    /// <summary>The user's newest notifications, with everything needed to display and link them.</summary>
    public async Task<List<Notification>> GetRecentAsync(int userId, int take = 30)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Notifications
            .Where(n => n.NotifiedUserId == userId)
            .Include(n => n.NotifierUser)
            .Include(n => n.Post)
                .ThenInclude(p => p!.RegularUser)
            .Include(n => n.Post)
                .ThenInclude(p => p!.Community)
            .Include(n => n.Comment)
            .OrderByDescending(n => n.DateCreated)
            .ThenByDescending(n => n.Id)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var unread = await db.Notifications
            .Where(n => n.NotifiedUserId == userId && !n.IsRead)
            .ToListAsync();
        if (unread.Count == 0) return;

        foreach (var n in unread) n.IsRead = true;
        await db.SaveChangesAsync();
        _notifier.Notify(userId);   // other open tabs of this user clear their badge too
    }

    // ---------- Creating (called by PostService / CommentService before they save) ----------
    /// <summary>
    /// For a new post or repost (call after db.Posts.Add(post), before SaveChangesAsync).
    /// Priority: mentioned, follows the author, member of the post's community, follows one of its hashtags.
    /// </summary>
    public async Task AddForNewPostAsync(AppDbContext db, Post post, IEnumerable<int>? hashTagIds = null)
    {
        var authorId = post.RegularUserId;
        var reasons = new Dictionary<int, string>();

        AddReason(reasons, await FindMentionedUserIdsAsync(db, post.Content), Notification.TypeMention);

        AddReason(reasons, await db.Followers
            .Where(f => f.RegularUserFollowingId == authorId)
            .Select(f => f.RegularUserFollowerId)
            .ToListAsync(), Notification.TypeFollowedUserPost);

        if (post.CommunityId is int communityId)
        {
            AddReason(reasons, await db.CommunityUserRoles
                .Where(r => r.CommunityId == communityId)
                .Select(r => r.RegularUserId)
                .ToListAsync(), Notification.TypeCommunityPost);
        }

        var tagIds = hashTagIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
        if (tagIds.Count > 0)
        {
            AddReason(reasons, await db.HashTagFollows
                .Where(f => tagIds.Contains(f.HashTagId))
                .Select(f => f.RegularUserId)
                .ToListAsync(), Notification.TypeHashTagPost);
        }

        await AddNotificationsAsync(db, authorId, reasons, post.DatePosted,
            n => n.Post = post);
    }

    /// <summary>
    /// For a new comment (call after db.Comments.Add(comment), before SaveChangesAsync).
    /// Priority: mentioned, wrote the comment being replied to, owns the post, follows the post.
    /// </summary>
    public async Task AddForNewCommentAsync(AppDbContext db, Comment comment)
    {
        var postOwnerId = await db.Posts
            .Where(p => p.Id == comment.PostId)
            .Select(p => (int?)p.RegularUserId)
            .FirstOrDefaultAsync();
        if (postOwnerId is null) return;   // post doesn't exist; saving the comment will fail anyway

        var reasons = new Dictionary<int, string>();

        AddReason(reasons, await FindMentionedUserIdsAsync(db, comment.Content), Notification.TypeMention);

        if (comment.ParentCommentId is int parentId)
        {
            var parentAuthorId = await db.Comments
                .Where(c => c.Id == parentId)
                .Select(c => (int?)c.RegularUserId)
                .FirstOrDefaultAsync();
            if (parentAuthorId is int id)
                AddReason(reasons, new[] { id }, Notification.TypeReply);
        }

        AddReason(reasons, new[] { postOwnerId.Value }, Notification.TypeComment);

        AddReason(reasons, await db.PostFollows
            .Where(f => f.PostId == comment.PostId)
            .Select(f => f.RegularUserId)
            .ToListAsync(), Notification.TypeFollowedPostComment);

        await AddNotificationsAsync(db, comment.RegularUserId, reasons, comment.DatePosted, n =>
        {
            n.PostId = comment.PostId;
            n.Comment = comment;
        });
    }

    /// <summary>For a new like on a post (call before SaveChangesAsync). Notifies the post's owner.</summary>
    public async Task AddForPostLikeAsync(AppDbContext db, int likerId, int postId)
    {
        var ownerId = await db.Posts
            .Where(p => p.Id == postId)
            .Select(p => (int?)p.RegularUserId)
            .FirstOrDefaultAsync();
        if (ownerId is null) return;

        // Liking, unliking and liking again shouldn't stack up notifications.
        var alreadyNotified = await db.Notifications.AnyAsync(n =>
            n.Type == Notification.TypeLike && n.NotifierUserId == likerId && n.PostId == postId);
        if (alreadyNotified) return;

        var reasons = new Dictionary<int, string> { [ownerId.Value] = Notification.TypeLike };
        await AddNotificationsAsync(db, likerId, reasons, Now(), n => n.PostId = postId);
    }

    /// <summary>For an unlike (call before SaveChangesAsync). Removes that like's notification.</summary>
    public async Task RemoveForPostUnlikeAsync(AppDbContext db, int likerId, int postId)
    {
        var existing = await db.Notifications
            .Where(n => n.Type == Notification.TypeLike && n.NotifierUserId == likerId && n.PostId == postId)
            .ToListAsync();
        db.Notifications.RemoveRange(existing);
        NotifyAfterSave(db, existing.Select(n => n.NotifiedUserId));
    }

    /// <summary>For a new like on a comment (call before SaveChangesAsync). Notifies the comment's author.</summary>
    public async Task AddForCommentLikeAsync(AppDbContext db, int likerId, int commentId)
    {
        var comment = await db.Comments
            .Where(c => c.Id == commentId)
            .Select(c => new { c.RegularUserId, c.PostId })
            .FirstOrDefaultAsync();
        if (comment is null) return;

        // Liking, unliking and liking again shouldn't stack up notifications.
        var alreadyNotified = await db.Notifications.AnyAsync(n =>
            n.Type == Notification.TypeCommentLike && n.NotifierUserId == likerId && n.CommentId == commentId);
        if (alreadyNotified) return;

        var reasons = new Dictionary<int, string> { [comment.RegularUserId] = Notification.TypeCommentLike };
        await AddNotificationsAsync(db, likerId, reasons, Now(), n =>
        {
            n.PostId = comment.PostId;
            n.CommentId = commentId;
        });
    }

    /// <summary>For an unlike on a comment (call before SaveChangesAsync). Removes that like's notification.</summary>
    public async Task RemoveForCommentUnlikeAsync(AppDbContext db, int likerId, int commentId)
    {
        var existing = await db.Notifications
            .Where(n => n.Type == Notification.TypeCommentLike && n.NotifierUserId == likerId && n.CommentId == commentId)
            .ToListAsync();
        db.Notifications.RemoveRange(existing);
        NotifyAfterSave(db, existing.Select(n => n.NotifiedUserId));
    }

    /// <summary>For a new follow (call before SaveChangesAsync). Notifies the person being followed.</summary>
    public async Task AddForFollowAsync(AppDbContext db, int followerId, int followedId)
    {
        // Following, unfollowing and following again shouldn't stack up notifications.
        var alreadyNotified = await db.Notifications.AnyAsync(n =>
            n.Type == Notification.TypeFollow && n.NotifierUserId == followerId && n.NotifiedUserId == followedId);
        if (alreadyNotified) return;

        var reasons = new Dictionary<int, string> { [followedId] = Notification.TypeFollow };
        await AddNotificationsAsync(db, followerId, reasons, Now(), _ => { });
    }

    /// <summary>For an unfollow (call before SaveChangesAsync). Removes that follow's notification.</summary>
    public async Task RemoveForUnfollowAsync(AppDbContext db, int followerId, int followedId)
    {
        var existing = await db.Notifications
            .Where(n => n.Type == Notification.TypeFollow && n.NotifierUserId == followerId && n.NotifiedUserId == followedId)
            .ToListAsync();
        db.Notifications.RemoveRange(existing);
        NotifyAfterSave(db, existing.Select(n => n.NotifiedUserId));
    }

    /// <summary>
    /// For a post about to be deleted (call before SaveChangesAsync). Its notifications are
    /// removed by the database's cascade delete; this just refreshes the bells of anyone who had
    /// an unread one, so their count drops.
    /// </summary>
    public async Task NotifyForDeletedPostAsync(AppDbContext db, int postId)
    {
        var affected = await db.Notifications
            .Where(n => n.PostId == postId && !n.IsRead)
            .Select(n => n.NotifiedUserId)
            .Distinct()
            .ToListAsync();
        NotifyAfterSave(db, affected);
    }

    // Keeps the first reason given for each user (callers add reasons in priority order).
    private static void AddReason(Dictionary<int, string> reasons, IEnumerable<int> userIds, string type)
    {
        foreach (var userId in userIds)
            reasons.TryAdd(userId, type);
    }

    // Tells the notifier about these users once the caller's SaveChangesAsync succeeds
    // (SavedChanges doesn't fire if the save fails, so nobody's bell updates for a failed post).
    private void NotifyAfterSave(AppDbContext db, IEnumerable<int> userIds)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return;

        db.SavedChanges += (_, _) =>
        {
            foreach (var id in ids) _notifier.Notify(id);
        };
    }

    private async Task AddNotificationsAsync(AppDbContext db, int notifierId,
        Dictionary<int, string> reasons, DateTime dateCreated, Action<Notification> setTarget)
    {
        reasons.Remove(notifierId);
        if (reasons.Count == 0) return;

        var blocked = await db.BlockedUsers
            .Where(b => b.RegularUserBlockingId == notifierId || b.RegularUserBlockedId == notifierId)
            .Select(b => b.RegularUserBlockingId == notifierId ? b.RegularUserBlockedId : b.RegularUserBlockingId)
            .ToListAsync();

        var notifiedIds = new List<int>();
        foreach (var (notifiedId, type) in reasons)
        {
            if (blocked.Contains(notifiedId)) continue;
            notifiedIds.Add(notifiedId);

            var notification = new Notification
            {
                NotifierUserId = notifierId,
                NotifiedUserId = notifiedId,
                Type = type,
                DateCreated = dateCreated
            };
            setTarget(notification);
            db.Notifications.Add(notification);
        }

        NotifyAfterSave(db, notifiedIds);
    }

    private static async Task<List<int>> FindMentionedUserIdsAsync(AppDbContext db, string? content)
    {
        var lookupNames = MentionParser.GetLookupNames(content);
        if (lookupNames.Count == 0) return new List<int>();

        var users = await db.RegularUsers
            .Where(u => lookupNames.Contains(u.Username.ToLower()))
            .Select(u => new { u.Id, u.Username })
            .ToListAsync();

        var matched = MentionParser.MatchUsernames(content, users.Select(u => u.Username));
        return users.Where(u => matched.Contains(u.Username)).Select(u => u.Id).ToList();
    }

    // Columns are "timestamp without time zone"; Npgsql rejects Kind=Utc.
    private static DateTime Now() => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
}
