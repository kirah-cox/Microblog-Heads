using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class CommunityRole
{
    public int Id { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<CommunityUserRole> CommunityUserRoles { get; set; } = new List<CommunityUserRole>();
}
