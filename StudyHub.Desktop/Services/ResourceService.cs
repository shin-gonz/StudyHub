using MySql.Data.MySqlClient;
using StudyHub.Database;
using StudyHub.Models;

namespace StudyHub.Services;

public class ResourceService
{
    public void UploadResource(int studentId, string title, string description, string subject,
                               string authorName, string resourceType, string filePathOrUrl)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"INSERT INTO resources (student_id, title, description, subject, author_name, resource_type, file_path_or_url)
                       VALUES (@sid, @title, @desc, @subj, @author, @type, @path)";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@desc", description);
        cmd.Parameters.AddWithValue("@subj", subject);
        cmd.Parameters.AddWithValue("@author", authorName);
        cmd.Parameters.AddWithValue("@type", resourceType);
        cmd.Parameters.AddWithValue("@path", filePathOrUrl);
        cmd.ExecuteNonQuery();
    }

    public List<Resource> BrowseResources()
    {
        using var conn = DbConnectionFactory.GetConnection();
        using var cmd = new MySqlCommand("SELECT * FROM resources ORDER BY date_uploaded DESC", conn);
        return ReadResourceList(cmd);
    }

    public Resource? GetById(int resourceId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        using var cmd = new MySqlCommand("SELECT * FROM resources WHERE resource_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", resourceId);
        return ReadResourceList(cmd).FirstOrDefault();
    }

    public List<Resource> SearchResources(string keyword)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = "SELECT * FROM resources WHERE title LIKE @kw OR subject LIKE @kw ORDER BY date_uploaded DESC";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@kw", $"%{keyword}%");
        return ReadResourceList(cmd);
    }

    public void UpdateResource(Resource resource)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"UPDATE resources 
                       SET title = @title, description = @desc, subject = @subj, 
                           author_name = @author, file_path_or_url = @path
                       WHERE resource_id = @id";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@title", resource.Title);
        cmd.Parameters.AddWithValue("@desc", resource.Description);
        cmd.Parameters.AddWithValue("@subj", resource.Subject);
        cmd.Parameters.AddWithValue("@author", resource.AuthorName);
        cmd.Parameters.AddWithValue("@path", resource.FilePathOrUrl);
        cmd.Parameters.AddWithValue("@id", resource.ResourceId);
        cmd.ExecuteNonQuery();
    }

    public bool DeleteResource(int resourceId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        using var cmd = new MySqlCommand("DELETE FROM resources WHERE resource_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", resourceId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool ToggleBookmark(int studentId, int resourceId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string checkSql = "SELECT COUNT(*) FROM bookmarks WHERE student_id = @sid AND resource_id = @rid";
        using var checkCmd = new MySqlCommand(checkSql, conn);
        checkCmd.Parameters.AddWithValue("@sid", studentId);
        checkCmd.Parameters.AddWithValue("@rid", resourceId);

        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
        {
            using var delCmd = new MySqlCommand("DELETE FROM bookmarks WHERE student_id = @sid AND resource_id = @rid", conn);
            delCmd.Parameters.AddWithValue("@sid", studentId);
            delCmd.Parameters.AddWithValue("@rid", resourceId);
            delCmd.ExecuteNonQuery();
            return false;
        }

        using var insCmd = new MySqlCommand("INSERT INTO bookmarks (student_id, resource_id) VALUES (@sid, @rid)", conn);
        insCmd.Parameters.AddWithValue("@sid", studentId);
        insCmd.Parameters.AddWithValue("@rid", resourceId);
        insCmd.ExecuteNonQuery();
        return true;
    }

    public List<Resource> GetBookmarkedResources(int studentId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"SELECT r.* FROM resources r
                       INNER JOIN bookmarks b ON r.resource_id = b.resource_id
                       WHERE b.student_id = @sid ORDER BY b.date_bookmarked DESC";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@sid", studentId);
        return ReadResourceList(cmd);
    }

    public void RecordView(int studentId, int resourceId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"INSERT INTO recently_viewed (student_id, resource_id, viewed_at)
                       VALUES (@sid, @rid, NOW())
                       ON DUPLICATE KEY UPDATE viewed_at = NOW()";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@rid", resourceId);
        cmd.ExecuteNonQuery();
    }

    public List<Resource> GetRecentlyViewed(int studentId)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"SELECT r.* FROM resources r
                       INNER JOIN recently_viewed rv ON r.resource_id = rv.resource_id
                       WHERE rv.student_id = @sid ORDER BY rv.viewed_at DESC LIMIT 5";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@sid", studentId);
        return ReadResourceList(cmd);
    }

    private List<Resource> ReadResourceList(MySqlCommand cmd)
    {
        var list = new List<Resource>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Resource(
                reader.GetInt32("resource_id"),
                reader.GetInt32("student_id"),
                reader.GetString("title"),
                reader.GetString("description"),
                reader.GetString("subject"),
                reader.GetString("author_name"),
                reader.GetString("resource_type"),
                reader.GetString("file_path_or_url"),
                reader.GetDateTime("date_uploaded")
            ));
        }
        return list;
    }
}