using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class Follower
{
    public int Id { get; set; }

    public int RegularUserFollowerId { get; set; }

    public int RegularUserFollowingId { get; set; }

    public virtual RegularUser RegularUserFollower { get; set; } = null!;

    public virtual RegularUser RegularUserFollowing { get; set; } = null!;
}
