using microblog_heads.Data;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class DisplayService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DisplayService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Post>> GetPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var posts = await db.Posts
            .Where(x => x.RegularUserId == userID)
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetPostsCommunityIDAsync(int communityID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var posts = await db.Posts
            .Where(x => x.CommunityId == communityID)
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetPostsHashTagIDAsync(int hashTagID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var posts = await db.Posts
            .Where(x => x.PostHashTags.Any(pht => pht.HashTagId == hashTagID))
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetAllFollowedPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.PostFollows.Any(y => y.RegularUserId == userID))
            // Leave out reposts of a blocked user's post.
            .Where(x => x.ParentPost == null || !blockedIds.Contains(x.ParentPost.RegularUserId))
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetAllFollowedUserPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.RegularUser.FollowerRegularUserFollowings
                .Any(y => y.RegularUserFollowerId == userID))
            // Leave out reposts of a blocked user's post.
            .Where(x => x.ParentPost == null || !blockedIds.Contains(x.ParentPost.RegularUserId))
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetAllFollowedCommunityPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.Community != null && x.Community.CommunityUserRoles
                .Any(y => y.RegularUserId == userID))
            .Where(x => !blockedIds.Contains(x.RegularUserId))
            // Leave out reposts of a blocked user's post.
            .Where(x => x.ParentPost == null || !blockedIds.Contains(x.ParentPost.RegularUserId))
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetAllFollowedHashTagPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var followedTagIds = db.HashTagFollows
            .Where(f => f.RegularUserId == userID)
            .Select(f => f.HashTagId);

        var posts = await db.Posts
            .Where(x => x.PostHashTags.Any(pht => followedTagIds.Contains(pht.HashTagId)))
            .Where(x => !blockedIds.Contains(x.RegularUserId))
            // Leave out reposts of a blocked user's post.
            .Where(x => x.ParentPost == null || !blockedIds.Contains(x.ParentPost.RegularUserId))
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        await FillLikeCountsAsync(db, posts);
        return posts;
    }

    public async Task<List<Post>> GetAllFollowingPostsbyUserIDAsync(int userID)
    {
        var followedPosts = await GetAllFollowedPostsbyUserIDAsync(userID);
        var followedUserPosts = await GetAllFollowedUserPostsbyUserIDAsync(userID);
        var followedCommunityPosts = await GetAllFollowedCommunityPostsbyUserIDAsync(userID);
        var followedHashTagPosts = await GetAllFollowedHashTagPostsbyUserIDAsync(userID);

        return followedPosts
            .Concat(followedUserPosts)
            .Concat(followedCommunityPosts)
            .Concat(followedHashTagPosts)
            .DistinctBy(p => p.Id)
            .OrderByDescending(x => x.DatePosted)
            .ToList();
    }

    public async Task<List<Post>> GetForYouPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.RegularUserId != userID)
            .Where(x => !blockedIds.Contains(x.RegularUserId))
            // Leave out reposts of a blocked user's post.
            .Where(x => x.ParentPost == null || !blockedIds.Contains(x.ParentPost.RegularUserId))
            .Include(x => x.RegularUser)
            .Include(x => x.Community)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.RegularUser)
            .Include(x => x.ParentPost)
                .ThenInclude(pp => pp!.Community)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .Include(x => x.PostMedia)
                .ThenInclude(pm => pm.Media)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        await FillLikeCountsAsync(db, posts);

        // Shuffle here rather than with ORDER BY RANDOM() in SQL: the Includes above return one
        // row per post per hashtag/comment/like, and a random SQL order splits those rows up,
        // which makes EF return the same post more than once.
        return posts.OrderBy(_ => Random.Shared.Next()).ToList();
    }

    private static async Task<List<int>> GetBlockedUserIdsAsync(AppDbContext db, int userID)
    {
        return await db.BlockedUsers
            .Where(b => b.RegularUserBlockingId == userID || b.RegularUserBlockedId == userID)
            .Select(b => b.RegularUserBlockingId == userID
                ? b.RegularUserBlockedId
                : b.RegularUserBlockingId)
            .ToListAsync();
    }

    /// <summary>
    /// Sets Post.LikeCount and Comment.LikeCount with two COUNT queries (one row per post /
    /// comment), instead of loading every like row through Include.
    /// </summary>
    private static async Task FillLikeCountsAsync(AppDbContext db, List<Post> posts)
    {
        if (posts.Count == 0)
        {
            return;
        }

        var postIds = posts.Select(p => p.Id).ToList();
        var postLikeCounts = await db.PostLikes
            .Where(l => postIds.Contains(l.PostId))
            .GroupBy(l => l.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        var commentIds = posts.SelectMany(p => p.Comments).Select(c => c.Id).ToList();
        var commentLikeCounts = commentIds.Count == 0
            ? new Dictionary<int, int>()
            : await db.CommentLikes
                .Where(l => commentIds.Contains(l.CommentId))
                .GroupBy(l => l.CommentId)
                .Select(g => new { CommentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CommentId, x => x.Count);

        foreach (var post in posts)
        {
            post.LikeCount = postLikeCounts.GetValueOrDefault(post.Id);
            foreach (var comment in post.Comments)
            {
                comment.LikeCount = commentLikeCounts.GetValueOrDefault(comment.Id);
            }
        }
    }

    private static void RemoveBlockedComments(List<Post> posts, List<int> blockedIds)
    {
        var blocked = blockedIds.ToHashSet();

        foreach (var post in posts)
        {
            var byId = post.Comments.ToDictionary(c => c.Id);
            var known = new Dictionary<int, bool>();

            bool IsVisible(Comment c)
            {
                if (known.TryGetValue(c.Id, out var cached)) return cached;

                bool visible = !blocked.Contains(c.RegularUserId)
                    && (c.ParentCommentId is not int parentId
                        || (byId.TryGetValue(parentId, out var parent) && IsVisible(parent)));

                known[c.Id] = visible;
                return visible;
            }

            post.Comments = post.Comments.Where(IsVisible).ToList();

            foreach (var comment in post.Comments)
            {
                comment.InverseCommentNavigation = comment.InverseCommentNavigation.Where(IsVisible).ToList();
            }
        }
    }
}
