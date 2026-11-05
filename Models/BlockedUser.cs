using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class BlockedUser
{
    public int Id { get; set; }

    public int RegularUserBlockingId { get; set; }

    public int RegularUserBlockedId { get; set; }

    public virtual RegularUser RegularUserBlocked { get; set; } = null!;

    public virtual RegularUser RegularUserBlocking { get; set; } = null!;
}
