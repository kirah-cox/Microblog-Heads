using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class HashTag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<HashTagFollow> HashTagFollows { get; set; } = new List<HashTagFollow>();

    public virtual ICollection<PostHashTag> PostHashTags { get; set; } = new List<PostHashTag>();
}
