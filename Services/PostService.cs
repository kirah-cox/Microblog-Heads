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

    public async Task<Post> CreatePostAsync(int userId, string content, int? communityId = null, IEnumerable<string>? mediaFileNames = null)
    {
        var files = mediaFileNames?.ToList() ?? new List<string>();
        
        content = content?.Trim() ?? ""; // optional, we may wish to keep white space in later versions
        if (content.Length == 0 && files.Count == 0) 
            throw new ArgumentException("A post cannot be empty");
        if (content.Length > 500) 
            throw new ArgumentException("A post cannot exceed 500 characters");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var post = new Post
        {
            RegularUserId = userId,
            Content = content,
            DatePosted = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), // column is "timestamp without time zone"; Npgsql rejects Kind=Utc
            CommunityId = communityId,
            PostMedia = files.Select(f => new PostMedia { Media = new Media { Path = f } }).ToList()
        };
        try 
        {
            db.Posts.Add(post);
            await db.SaveChangesAsync();    
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not save the post. Does user (or community) exist?", ex);
        }
        return post;
    }

    public async Task<bool> ToggleLikePostAsync (int userId, int postId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var existing = await db.PostLikes.FirstOrDefaultAsync(l => l.RegularUserId == userId && l.PostId == postId);
        if (existing is not null)
        {
            db.PostLikes.Remove(existing);
            await db.SaveChangesAsync();

            return false; // if a like exists, we are 'unliking'the post
        }

        var like = new PostLike
        {
            RegularUserId = userId,
            PostId = postId
        };

        db.PostLikes.Add(like);

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
            await db.SaveChangesAsync();    
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not save the repost. Do the user, original post, and community exist?", ex);
        }
        return post;
    }
    
}
