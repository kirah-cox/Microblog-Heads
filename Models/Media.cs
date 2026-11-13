using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class Media
{
    public int Id { get; set; }

    public string Path { get; set; } = null!;

    public virtual ICollection<PostMedia> PostMedia { get; set; } = new List<PostMedia>();
}
