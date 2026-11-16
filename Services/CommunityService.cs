using System.Linq.Expressions;
using microblog_heads.Data;
using microblog_heads.Migrations;
using microblog_heads.Models;
using Microsoft.EntityFrameworkCore;

namespace microblog_heads.Services;

public class CommunityService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CommunityService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Community?> GetCommunityByIdAsync(int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Communities.FindAsync(communityId);
    }

    public async Task<List<Community>> GetAllCommunitiesAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Communities.ToListAsync();
    }

    public async Task<Community> CreateCommunityAsync(int userId, string name, string description, 
                                int? parentCommunityId = null, bool isPrivate = false )
    {
        description = description?.Trim() ?? "";
        name = name?.Trim() ??"";

        if (name.Length <= 0)
            throw new ArgumentException("A name must have characters in it.");
        if (name.Length > 50)
            throw new ArgumentException("A name cannot exceed 50 characters");

        if (description.Length <=0) /// A description is required, this may be revisited in V2
            throw new ArgumentException("A description must have characters in it.");
        if (description.Length > 200)
            throw new ArgumentException("A description cannot exceed 200 characters");
        

        await using var db = await _dbFactory.CreateDbContextAsync();

        var nameTaken = await db.Communities.AnyAsync(c => c.Name == name);
        if(nameTaken)
        {
            throw new ArgumentException("A community with that name already exists");           
        }

        var adminRole = await db.CommunityRoles.FirstOrDefaultAsync(r => r.Role == "Admin");
        if (adminRole is null)
            throw new InvalidOperationException("Admin role not found. Have the roles been seeded?");

        var community = new Community {
            Name = name,
            Description = description,
            ParentCommunityId = parentCommunityId,
            IsPrivate = isPrivate,
            CommunityUserRoles = new List<CommunityUserRole>
            {
                new CommunityUserRole
                {
                    RegularUserId = userId,
                    CommunityRoleId = adminRole.Id
                }
            }
        };
        try
        {
            db.Communities.Add(community);
            await db.SaveChangesAsync(); 
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not save the community", ex);
        }
        return community;
    }

    public async Task<bool> JoinCommunityAsync(int userId, int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var community = await db.Communities.FindAsync(communityId);
        if (community is null)
            throw new InvalidOperationException("That community doesn't exist.");

        if (await db.CommunityUserRoles.AnyAsync(r => r.CommunityId == communityId && r.RegularUserId == userId))
            throw new InvalidOperationException("You are already a member of this community.");

        if (community.IsPrivate)
        {
            if (await db.CommunityJoinRequests.AnyAsync(r => r.CommunityId == communityId && r.RegularUserId == userId))
                throw new InvalidOperationException("Your request is already waiting for approval.");

            try
            {
                db.CommunityJoinRequests.Add(new CommunityJoinRequest
                {
                    CommunityId = communityId,
                    RegularUserId = userId,
                    DateRequested = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Could not send the join request.", ex);
            }
            return false;   // request sent, not a member yet
        }

        var followRole = await db.CommunityRoles.FirstOrDefaultAsync(r => r.Role == "Follower");
        if (followRole is null)
            throw new InvalidOperationException("Follower role not found. Have the roles been seeded?");

        try
        {
            db.CommunityUserRoles.Add(new CommunityUserRole
            {
                CommunityId = communityId,
                RegularUserId = userId,
                CommunityRoleId = followRole.Id
            });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not join the community.", ex);
        }
        return true;   // joined
    }

    public async Task AssignRoleAsync(int actingUserId, int communityId, int targetUserId, string roleName)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var actorIsAdmin = await db.CommunityUserRoles.AnyAsync(r => r.CommunityId == communityId && r.RegularUserId == actingUserId && r.CommunityRole.Role == "Admin");
        if (!actorIsAdmin)
            throw new InvalidOperationException("Must be an admin to change roles");


        var newRole = await db.CommunityRoles.FirstOrDefaultAsync(r => r.Role == roleName);
        if (newRole is null)
            throw new InvalidOperationException("This is not a valid role");

        var membership = await db.CommunityUserRoles.FirstOrDefaultAsync(r => r.CommunityId == communityId && r.RegularUserId == targetUserId);
        if (membership is null)
            throw new InvalidOperationException("User is not a member of the community");

        var targetIsAdmin = await db.CommunityUserRoles.AnyAsync(r => r.Id == membership.Id && r.CommunityRole.Role == "Admin");

        if (targetIsAdmin && newRole.Role != "Admin")
        {
            var adminCount = await db.CommunityUserRoles
                .CountAsync(r => r.CommunityId == communityId && r.CommunityRole.Role == "Admin");
            if (adminCount <= 1)
                throw new InvalidOperationException("A community must keep at least one admin.");
        }
        try
        {
            membership.CommunityRoleId = newRole.Id;
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not update users role. ", ex);
        }

    }

    public async Task<List<CommunityUserRole>> GetCommunityMembersAsync(int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CommunityUserRoles
            .Where(r => r.CommunityId == communityId)
            .Include(r => r.RegularUser)
            .Include(r => r.CommunityRole)
            .OrderBy(r => r.RegularUser.Username)
            .ToListAsync();
    }
    public async Task<List<CommunityUserRole>> GetCommunityRolesByUserIdAsync(int userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CommunityUserRoles
            .Where(r => r.RegularUserId == userId)
            .Include(r => r.Community)
            .Include(r => r.CommunityRole)
            .OrderBy(r => r.Community.Name)
            .ToListAsync();
    }
    private static Task<bool> IsStaffAsync(AppDbContext db, int userId, int communityId) =>
    db.CommunityUserRoles.AnyAsync(r => r.CommunityId == communityId
        && r.RegularUserId == userId
        && (r.CommunityRole.Role == "Admin" || r.CommunityRole.Role == "Moderator"));

    public async Task<bool> HasPendingRequestAsync(int userId, int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CommunityJoinRequests.AnyAsync(r => r.CommunityId == communityId && r.RegularUserId == userId);
    }

    public async Task<List<CommunityJoinRequest>> GetPendingRequestsAsync(int actingUserId, int communityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        if (!await IsStaffAsync(db, actingUserId, communityId))
            throw new InvalidOperationException("Only an admin or moderator can see join requests.");

        return await db.CommunityJoinRequests
            .Where(r => r.CommunityId == communityId)
            .Include(r => r.RegularUser)
            .OrderBy(r => r.DateRequested)
            .ToListAsync();
    }

    public async Task ApproveRequestAsync(int actingUserId, int requestId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var request = await db.CommunityJoinRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request is null)
            throw new InvalidOperationException("That request no longer exists.");

        if (!await IsStaffAsync(db, actingUserId, request.CommunityId))
            throw new InvalidOperationException("Only an admin or moderator can approve requests.");

        var followRole = await db.CommunityRoles.FirstOrDefaultAsync(r => r.Role == "Follower");
        if (followRole is null)
            throw new InvalidOperationException("Follower role not found. Have the roles been seeded?");

        try
        {
            db.CommunityUserRoles.Add(new CommunityUserRole
            {
                CommunityId = request.CommunityId,
                RegularUserId = request.RegularUserId,
                CommunityRoleId = followRole.Id
            });
            db.CommunityJoinRequests.Remove(request);
            await db.SaveChangesAsync();   // both changes succeed or neither does
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not approve the request.", ex);
        }
    }

    public async Task RejectRequestAsync(int actingUserId, int requestId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var request = await db.CommunityJoinRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request is null)
            throw new InvalidOperationException("That request no longer exists.");

        if (!await IsStaffAsync(db, actingUserId, request.CommunityId))
            throw new InvalidOperationException("Only an admin or moderator can reject requests.");

        db.CommunityJoinRequests.Remove(request);
        await db.SaveChangesAsync();
    }
    public async Task RemoveMemberAsync(int actingUserId, int communityId, int targetUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var target = await db.CommunityUserRoles
            .Include(r => r.CommunityRole)
            .FirstOrDefaultAsync(r => r.CommunityId == communityId && r.RegularUserId == targetUserId);
        if (target is null)
            throw new InvalidOperationException("That user is not a member of this community.");

        if (actingUserId != targetUserId)
        {
            var actor = await db.CommunityUserRoles
                .Include(r => r.CommunityRole)
                .FirstOrDefaultAsync(r => r.CommunityId == communityId && r.RegularUserId == actingUserId);

            if (actor is null || (actor.CommunityRole.Role != "Admin" && actor.CommunityRole.Role != "Moderator"))
                throw new InvalidOperationException("Only an admin or moderator can remove members.");

            if (target.CommunityRole.Role == "Admin" && actor.CommunityRole.Role != "Admin")
                throw new InvalidOperationException("Only an admin can remove an admin.");
        }

        if (target.CommunityRole.Role == "Admin")
        {
            var adminCount = await db.CommunityUserRoles
                .CountAsync(r => r.CommunityId == communityId && r.CommunityRole.Role == "Admin");
            if (adminCount <= 1)
                throw new InvalidOperationException("A community must keep at least one admin. Promote someone else first.");
        }

        try
        {
            db.CommunityUserRoles.Remove(target);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not remove the member.", ex);
        }
    }
}
