namespace microblog_heads.Models;

public partial class CommunityJoinRequest
{
    public int Id { get; set; }
    public int CommunityId { get; set; }
    public int RegularUserId { get; set; }
    public DateTime DateRequested { get; set; }

    public virtual Community Community { get; set; } = null!;
    public virtual RegularUser RegularUser { get; set; } = null!;
}