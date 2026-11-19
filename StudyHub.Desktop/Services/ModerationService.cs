using MySql.Data.MySqlClient;
using StudyHub.Database;
using StudyHub.Models;

namespace StudyHub.Services;

public class ModerationService
{
    private readonly ResourceService _resourceService;
    private readonly CommentService _commentService;

    public ModerationService(ResourceService resourceService, CommentService commentService)
    {
        _resourceService = resourceService;
        _commentService = commentService;
    }

    public void SubmitReport(int reporterId, int targetId, string targetType, string reason)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"INSERT INTO reports (reporter_id, target_id, target_type, reason)
                       VALUES (@repId, @tId, @tType, @reason)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@repId", reporterId);
        cmd.Parameters.AddWithValue("@tId", targetId);
        cmd.Parameters.AddWithValue("@tType", targetType);
        cmd.Parameters.AddWithValue("@reason", reason);
        cmd.ExecuteNonQuery();
    }

    public List<Report> GetPendingReports()
    {
        var list = new List<Report>();
        using var conn = DbConnectionFactory.GetConnection();
        using var cmd = new MySqlCommand("SELECT * FROM reports WHERE status = 'Pending' ORDER BY date_reported ASC", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Report(
                reader.GetInt32("report_id"),
                reader.GetInt32("reporter_id"),
                reader.GetInt32("target_id"),
                reader.GetString("target_type"),
                reader.GetString("reason"),
                reader.GetString("status"),
                reader.GetDateTime("date_reported")
            ));
        }
        return list;
    }

    public bool DismissReport(int reportId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        using var cmd = new MySqlCommand("UPDATE reports SET status = 'Dismissed' WHERE report_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", reportId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool RemoveReportedResource(int reportId)
    {
        var report = GetPendingReports().FirstOrDefault(r => r.ReportId == reportId && r.TargetType == "Resource");
        if (report == null) return false;

        _resourceService.DeleteResource(report.TargetId);
        MarkReportResolved(reportId);
        return true;
    }

    public bool RemoveReportedComment(int reportId)
    {
        var report = GetPendingReports().FirstOrDefault(r => r.ReportId == reportId && r.TargetType == "Comment");
        if (report == null) return false;

        _commentService.DeleteComment(report.TargetId);
        MarkReportResolved(reportId);
        return true;
    }

    private void MarkReportResolved(int reportId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        using var cmd = new MySqlCommand("UPDATE reports SET status = 'Resolved' WHERE report_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", reportId);
        cmd.ExecuteNonQuery();
    }
}