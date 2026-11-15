namespace microblog_heads.Models;

public class GroupedPostReport
{
    public Post Post { get; set; }
    public List<PostMissuse> Reports { get; set; } = new();
    public int ReportCount => Reports.Count;
    public List<string> ReportReasons => Reports.Select(r => r.MissuseName).Distinct().ToList();
}

public class GroupedCommentReport
{
    public Comment Comment { get; set; }
    public List<CommentMissuse> Reports { get; set; } = new();
    public int ReportCount => Reports.Count;
    public List<string> ReportReasons => Reports.Select(r => r.MissuseName).Distinct().ToList();
}
