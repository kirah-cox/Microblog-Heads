using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class PostMedia
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int MediaId { get; set; }

    public virtual Media Media { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
