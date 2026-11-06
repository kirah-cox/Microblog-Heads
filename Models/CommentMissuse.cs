using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class CommentMissuse
{
    public int Id { get; set; }

    public int RegularUserId { get; set; }

    public int CommentId { get; set; }

    public string MissuseName { get; set; } = null!;

    public string MissuseDescription { get; set; } = null!;

    public virtual Comment Comment { get; set; } = null!;

    public virtual RegularUser RegularUser { get; set; } = null!;
}
