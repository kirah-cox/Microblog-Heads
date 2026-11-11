using System.ComponentModel.DataAnnotations.Schema;

namespace microblog_heads.Models;

// Like counts filled in by DisplayService with a COUNT query, so feeds don't have to
// load every PostLike/CommentLike row just to show a number. Not database columns.

public partial class Post
{
    [NotMapped]
    public int LikeCount { get; set; }
}

public partial class Comment
{
    [NotMapped]
    public int LikeCount { get; set; }
}
