using System;
using System.Collections.Generic;

namespace microblog_heads.Models;

public partial class Notification
{
    // Values for Type.
    public const string TypeMention = "mention";                            // mentioned in a post, repost or comment
    public const string TypeLike = "like";                                  // someone liked your post
    public const string TypeComment = "comment";                            // someone commented on your post
    public const string TypeReply = "reply";                                // someone replied to your comment
    public const string TypeCommentLike = "comment_like";                   // someone liked your comment
    public const string TypeFollowedUserPost = "followed_user_post";        // someone you follow posted
    public const string TypeCommunityPost = "community_post";               // someone posted in your community
    public const string TypeHashTagPost = "hashtag_post";                   // someone posted with a hashtag you follow
    public const string TypeFollowedPostComment = "followed_post_comment";  // someone commented on a post you follow
    public const string TypeFollow = "follow";                              // someone followed you (no post)

    public int Id { get; set; }

    public int NotifierUserId { get; set; }

    public int NotifiedUserId { get; set; }

    public string Type { get; set; } = null!;

    public int? PostId { get; set; }

    public int? CommentId { get; set; }

    public bool IsRead { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual Comment? Comment { get; set; }

    public virtual RegularUser NotifiedUser { get; set; } = null!;

    public virtual RegularUser NotifierUser { get; set; } = null!;

    public virtual Post? Post { get; set; }
}
