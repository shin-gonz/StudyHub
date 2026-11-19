using MySql.Data.MySqlClient;
using StudyHub.Database;
using StudyHub.Models;

namespace StudyHub.Services;

public class CommentService
{
    public void AddComment(int resourceId, int studentId, string content)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = "INSERT INTO comments (resource_id, student_id, content) VALUES (@rid, @sid, @content)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rid", resourceId);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@content", content);
        cmd.ExecuteNonQuery();
    }

    public List<Comment> GetCommentsByResource(int resourceId)
    {
        var list = new List<Comment>();
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"SELECT c.*, u.full_name FROM comments c
                       INNER JOIN users u ON c.student_id = u.user_id
                       WHERE c.resource_id = @rid ORDER BY c.date_posted ASC";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rid", resourceId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Comment(
                reader.GetInt32("comment_id"),
                reader.GetInt32("resource_id"),
                reader.GetInt32("student_id"),
                reader.GetString("full_name"),
                reader.GetString("content"),
                reader.GetDateTime("date_posted")
            ));
        }
        return list;
    }

    public bool EditComment(int commentId, int studentId, string newContent)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = "UPDATE comments SET content = @content WHERE comment_id = @cid AND student_id = @sid";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@content", newContent);
        cmd.Parameters.AddWithValue("@cid", commentId);
        cmd.Parameters.AddWithValue("@sid", studentId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool DeleteComment(int commentId, int? studentId = null)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = studentId.HasValue
            ? "DELETE FROM comments WHERE comment_id = @cid AND student_id = @sid"
            : "DELETE FROM comments WHERE comment_id = @cid";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cid", commentId);
        if (studentId.HasValue) cmd.Parameters.AddWithValue("@sid", studentId.Value);
        return cmd.ExecuteNonQuery() > 0;
    }
}