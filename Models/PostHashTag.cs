using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class PostHashTag
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int HashTagId { get; set; }

    public virtual HashTag HashTag { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
