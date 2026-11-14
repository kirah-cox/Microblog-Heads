using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class PostService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly NotificationService _notifications;
    private readonly MediaService _media;

    public PostService(IDbContextFactory<AppDbContext> dbFactory, NotificationService notifications, MediaService media)
    {
        _dbFactory = dbFactory;
        _notifications = notifications;
        _media = media;
    }

    /// <summary>
    /// Deletes one of the user's own posts. The database's cascade deletes remove its comments,
    /// likes, follows, hashtag links, reports and notifications; this also deletes its pictures
    /// (rows and files). Reposts of it are kept (every post has its own text) and become
    /// standalone posts, because fk_post_parent sets their parent_post_id to null.
    /// </summary>
    public async Task DeletePostAsync(int userId, int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var post = await db.Posts
            .Include(p => p.PostMedia)
                .ThenInclude(pm => pm.Media)
            .FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null)
            throw new InvalidOperationException("That post no longer exists.");
        if (post.RegularUserId != userId)
            throw new InvalidOperationException("You can only delete your own posts.");

        var media = post.PostMedia.Select(pm => pm.Media).ToList();

        await _notifications.NotifyForDeletedPostAsync(db, post.Id);
        db.Media.RemoveRange(media);
        db.Posts.Remove(post);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not delete the post.", ex);
        }

        // Only remove the files once the rows are gone, so a failed delete leaves the post intact.
        foreach (var m in media) _media.DeleteFile(m.Path);
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

    public async Task<Post> CreatePostAsync(int userId, string content, int? communityId = null, IEnumerable<string>? mediaFileNames = null, IEnumerable<string>? hashTags = null)
    {
        var files = mediaFileNames?.ToList() ?? new List<string>();
        var tagNames = NormalizeHashTags(hashTags);

        content = content?.Trim() ?? ""; // optional, we may wish to keep white space in later versions
        if (content.Length == 0 && files.Count == 0)
            throw new ArgumentException("A post cannot be empty");
        if (content.Length > 500)
            throw new ArgumentException("A post cannot exceed 500 characters");

        await using var db = await _dbFactory.CreateDbContextAsync();

        // Reuse hashtags that already exist; create the rest.
        var existingTags = await db.HashTags
            .Where(h => tagNames.Contains(h.Name.ToLower()))
            .ToListAsync();
        var tags = tagNames
            .Select(name => existingTags.FirstOrDefault(h => h.Name.ToLower() == name) ?? new HashTag { Name = name })
            .ToList();

        var post = new Post
        {
            RegularUserId = userId,
            Content = content,
            DatePosted = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), // column is "timestamp without time zone"; Npgsql rejects Kind=Utc
            CommunityId = communityId,
            PostMedia = files.Select(f => new PostMedia { Media = new Media { Path = f } }).ToList(),
            PostHashTags = tags.Select(t => new PostHashTag { HashTag = t }).ToList()
        };
        try 
        {
            db.Posts.Add(post);
            // New hashtags have no followers yet, so only existing ones matter.
            await _notifications.AddForNewPostAsync(db, post, existingTags.Select(t => t.Id));
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not save the post. Does user (or community) exist?", ex);
        }
        return post;
    }

    /// <summary>
    /// Trims, drops a leading '#', lowercases, removes blanks and duplicates, and checks each
    /// hashtag is 1-50 letters, digits or underscores.
    /// </summary>
    private static List<string> NormalizeHashTags(IEnumerable<string>? hashTags)
    {
        var names = (hashTags ?? Enumerable.Empty<string>())
            .Select(t => (t ?? "").Trim().TrimStart('#').ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();

        foreach (var name in names)
        {
            if (name.Length > 50)
                throw new ArgumentException($"Hashtag #{name} is longer than 50 characters");
            if (!name.All(c => char.IsLetterOrDigit(c) || c == '_'))
                throw new ArgumentException($"Hashtag #{name} can only contain letters, numbers and underscores");
        }

        return names;
    }

    public async Task<bool> ToggleLikePostAsync (int userId, int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var existing = await db.PostLikes.FirstOrDefaultAsync(l => l.RegularUserId == userId && l.PostId == postId);
        if (existing is not null)
        {
            db.PostLikes.Remove(existing);
            await _notifications.RemoveForPostUnlikeAsync(db, userId, postId);
            await db.SaveChangesAsync();

            return false; // if a like exists, we are 'unliking'the post
        }

        var like = new PostLike
        {
            RegularUserId = userId,
            PostId = postId
        };

        db.PostLikes.Add(like);
        await _notifications.AddForPostLikeAsync(db, userId, postId);

        await db.SaveChangesAsync();

        return true;
    }

    public async Task<List<int>> GetLikedPostIdsByUserIdAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PostLikes
            .Where(l => l.RegularUserId == userId)
            .Select(l => l.PostId)
            .ToListAsync();
    }

    public async Task<bool> ToggleFollowPostAsync (int userId, int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var existing = await db.PostFollows.FirstOrDefaultAsync(f => f.RegularUserId == userId && f.PostId == postId);
        if (existing is not null)
        {
            db.PostFollows.Remove(existing);
            await db.SaveChangesAsync();

            return false; // if a follow exists, we are 'unfollowing' the post
        }

        var follow = new PostFollow
        {
            RegularUserId = userId,
            PostId = postId
        };

        db.PostFollows.Add(follow);

        await db.SaveChangesAsync();

        return true;
    }

    public async Task<Post> CreateRepostAsync (int userId, int originalPostId, string content, int? communityId = null)
    {
        content = content?.Trim() ?? ""; // this is less optional for repost as we may have no comment, just a repost.
        
        if (content.Length > 500) 
            throw new ArgumentException("A post cannot exceed 500 characters");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var post = new Post
        {
            RegularUserId = userId,
            ParentPostId = originalPostId,
            Content = content,
            DatePosted = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), // column is "timestamp without time zone"; Npgsql rejects Kind=Utc
            CommunityId = communityId
        };
        try 
        {
            db.Posts.Add(post);
            await _notifications.AddForNewPostAsync(db, post);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not save the repost. Do the user, original post, and community exist?", ex);
        }
        return post;
    }
    
}
