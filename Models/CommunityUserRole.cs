using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class CommunityUserRole
{
    public int Id { get; set; }

    public int CommunityId { get; set; }

    public int RegularUserId { get; set; }

    public int CommunityRoleId { get; set; }

    public virtual Community Community { get; set; } = null!;

    public virtual CommunityRole CommunityRole { get; set; } = null!;

    public virtual RegularUser RegularUser { get; set; } = null!;
}
