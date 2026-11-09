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
        return await db.Posts
            .Where(x => x.RegularUserId == userID)
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();
    }

    public async Task<List<Post>> GetPostsCommunityIDAsync(int communityID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Posts
            .Where(x => x.CommunityId == communityID)
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();
    }

    public async Task<List<Post>> GetAllFollowedPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.PostFollows.Any(y => y.RegularUserId == userID))
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        return posts;
    }

    public async Task<List<Post>> GetAllFollowedUserPostsbyUserIDAsync(int userID)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.RegularUser.FollowerRegularUserFollowings
                .Any(y => y.RegularUserFollowerId == userID))
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
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
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
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
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .OrderByDescending(x => x.DatePosted)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
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

    public async Task<List<Post>> GetForYouPostsbyUserIDAsync(int userID, int count = 20)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var blockedIds = await GetBlockedUserIdsAsync(db, userID);

        var posts = await db.Posts
            .Where(x => x.RegularUserId != userID)
            .Where(x => !blockedIds.Contains(x.RegularUserId))
            .OrderBy(x => EF.Functions.Random())
            .Take(count)
            .Include(x => x.RegularUser)
            .Include(x => x.PostLikes)
            .Include(x => x.PostFollows)
            .Include(x => x.PostHashTags)
                .ThenInclude(pht => pht.HashTag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.RegularUser)
            .ToListAsync();

        RemoveBlockedComments(posts, blockedIds);
        return posts;
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
