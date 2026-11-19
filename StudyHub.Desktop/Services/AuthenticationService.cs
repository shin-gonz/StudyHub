using MySql.Data.MySqlClient;
using StudyHub.Database;
using StudyHub.Models;

namespace StudyHub.Services;

public class AuthenticationService
{
    public Student? RegisterStudent(string fullName, string email, string password, string course, string yearLevel)
    {
        using var conn = DbConnectionFactory.GetConnection();

        // Check if email already exists
        using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", conn);
        checkCmd.Parameters.AddWithValue("@email", email);
        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            return null;

        string insertSql = @"INSERT INTO users (full_name, email, password, role, course, year_level)
                             VALUES (@name, @email, @pass, 'Student', @course, @year);
                             SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(insertSql, conn);
        cmd.Parameters.AddWithValue("@name", fullName);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@pass", password);
        cmd.Parameters.AddWithValue("@course", course);
        cmd.Parameters.AddWithValue("@year", yearLevel);

        int newId = Convert.ToInt32(cmd.ExecuteScalar());
        return new Student(newId, fullName, email, password, course, yearLevel);
    }

    public User? ValidateLogin(string email, string password)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = "SELECT * FROM users WHERE email = @email AND password = @pass LIMIT 1";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@pass", password);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        int id = reader.GetInt32("user_id");
        string name = reader.GetString("full_name");
        string role = reader.GetString("role");

        if (role == "Administrator")
            return new Administrator(id, name, email, password);

        string course = reader.IsDBNull(reader.GetOrdinal("course")) ? "" : reader.GetString("course");
        string year = reader.IsDBNull(reader.GetOrdinal("year_level")) ? "" : reader.GetString("year_level");
        return new Student(id, name, email, password, course, year);
    }

    public void UpdateStudentProfile(Student student)
    {
        using var conn = DbConnectionFactory.GetConnection();
        string sql = @"UPDATE users 
                       SET full_name = @name, course = @course, year_level = @year 
                       WHERE user_id = @id";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", student.GetFullName());
        cmd.Parameters.AddWithValue("@course", student.Course);
        cmd.Parameters.AddWithValue("@year", student.YearLevel);
        cmd.Parameters.AddWithValue("@id", student.GetUserId());
        cmd.ExecuteNonQuery();
    }
}