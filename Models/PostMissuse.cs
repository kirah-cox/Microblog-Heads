using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class PostMissuse
{
    public int Id { get; set; }

    public int RegularUserId { get; set; }

    public int PostId { get; set; }

    public string MissuseName { get; set; } = null!;

    public string MissuseDescription { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;

    public virtual RegularUser RegularUser { get; set; } = null!;
}
