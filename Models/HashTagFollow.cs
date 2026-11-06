using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class HashTagFollow
{
    public int Id { get; set; }

    public int RegularUserId { get; set; }

    public int HashTagId { get; set; }

    public virtual HashTag HashTag { get; set; } = null!;

    public virtual RegularUser RegularUser { get; set; } = null!;
}
