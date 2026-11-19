namespace StudyHub.Models;

public class Report
{
    public int ReportId { get; }
    public int ReporterId { get; }
    public int TargetId { get; }
    public string TargetType { get; } // "Resource" or "Comment"
    public string Reason { get; }
    public string Status { get; private set; } // "Pending", "Resolved", "Dismissed"
    public DateTime DateReported { get; }

    public Report(int reportId, int reporterId, int targetId, string targetType, string reason,
              string status = "Pending", DateTime? dateReported = null)
{
    ReportId = reportId;
    ReporterId = reporterId;
    TargetId = targetId;
    TargetType = targetType;
    Reason = reason;
    Status = status;
    DateReported = dateReported ?? DateTime.Now;
}

    public void MarkResolved() => Status = "Resolved";
    public void Dismiss() => Status = "Dismissed";
}