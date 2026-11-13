using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class Community
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? ParentCommunityId { get; set; }

    public bool IsPrivate { get; set; }

    public virtual ICollection<CommunityJoinRequest> CommunityJoinRequests { get; set; } = new List<CommunityJoinRequest>();

    public virtual ICollection<CommunityUserRole> CommunityUserRoles { get; set; } = new List<CommunityUserRole>();

    public virtual ICollection<Community> InverseParentCommunity { get; set; } = new List<Community>();

    public virtual Community? ParentCommunity { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
