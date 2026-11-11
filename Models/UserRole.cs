using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class UserRole
{
    // Ids of the seeded roles (see HasData in AppDbContext).
    public const int UserRoleId = 1;
    public const int AdminRoleId = 2;

    public int Id { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<RegularUser> RegularUsers { get; set; } = new List<RegularUser>();
}
