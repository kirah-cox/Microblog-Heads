using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int RegularUserId { get; set; }

    public int PostId { get; set; }

    public int? ParentCommentId { get; set; }

    public string? Content { get; set; }

    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    public virtual ICollection<CommentMissuse> CommentMissuses { get; set; } = new List<CommentMissuse>();

    public virtual Comment? CommentNavigation { get; set; }

    public virtual ICollection<Comment> InverseCommentNavigation { get; set; } = new List<Comment>();

    public virtual Post Post { get; set; } = null!;

    public virtual RegularUser RegularUser { get; set; } = null!;
}
