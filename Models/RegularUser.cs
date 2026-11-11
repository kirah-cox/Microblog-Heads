using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class RegularUser
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int UserRoleID { get; set; }

    public virtual UserRole UserRole { get; set; } = null!;

    public virtual ICollection<BlockedUser> BlockedUserRegularUserBlockeds { get; set; } = new List<BlockedUser>();

    public virtual ICollection<BlockedUser> BlockedUserRegularUserBlockings { get; set; } = new List<BlockedUser>();

    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    public virtual ICollection<CommentMissuse> CommentMissuses { get; set; } = new List<CommentMissuse>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<CommunityUserRole> CommunityUserRoles { get; set; } = new List<CommunityUserRole>();

    public virtual ICollection<Follower> FollowerRegularUserFollowers { get; set; } = new List<Follower>();

    public virtual ICollection<Follower> FollowerRegularUserFollowings { get; set; } = new List<Follower>();

    public virtual ICollection<HashTagFollow> HashTagFollows { get; set; } = new List<HashTagFollow>();

    public virtual ICollection<PostFollow> PostFollows { get; set; } = new List<PostFollow>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<PostMissuse> PostMissuses { get; set; } = new List<PostMissuse>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
