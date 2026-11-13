using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class Post
{
    public int Id { get; set; }

    public int RegularUserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime DatePosted { get; set; }

    public int? CommunityId { get; set; }

    public int? ParentPostId { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Community? Community { get; set; }

    public virtual ICollection<Post> InverseParentPost { get; set; } = new List<Post>();

    public virtual Post? ParentPost { get; set; }

    public virtual ICollection<PostFollow> PostFollows { get; set; } = new List<PostFollow>();

    public virtual ICollection<PostHashTag> PostHashTags { get; set; } = new List<PostHashTag>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<PostMedia> PostMedia { get; set; } = new List<PostMedia>();

    public virtual ICollection<PostMissuse> PostMissuses { get; set; } = new List<PostMissuse>();

    public virtual RegularUser RegularUser { get; set; } = null!;
}
