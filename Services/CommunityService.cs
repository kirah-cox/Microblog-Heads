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

    public async Task JoinCommunityAsync(int userId, int communityId)
    {
        // this treats all communities as public. V2 may allow private communities where admin/moderator must admit new users
        await using var db = await _dbFactory.CreateDbContextAsync();

        var alreadyMember = await db.CommunityUserRoles.AnyAsync(r => r.CommunityId == communityId && r.RegularUserId == userId);
        if (alreadyMember)
            throw new InvalidOperationException("You are already a member of this community.");

        var followRole = await db.CommunityRoles.FirstOrDefaultAsync(r => r.Role == "Follower"); 
        if (followRole is null)
            throw new InvalidOperationException("Follower role not found. Have the roles been seeded?");
 
        var communityUserRole = 
            new CommunityUserRole
            {
                CommunityId = communityId,
                RegularUserId = userId,
                CommunityRoleId = followRole.Id
            };
            
        try
        {
            db.CommunityUserRoles.Add(communityUserRole);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Could not join community. ", ex);
        }
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

}
