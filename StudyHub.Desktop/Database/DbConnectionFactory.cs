using MySql.Data.MySqlClient;

namespace StudyHub.Database;

public static class DbConnectionFactory
{
    private const string ConnectionString =
        "Server=localhost;Port=3306;Database=studyhub_db;Uid=root;Pwd=root123;";

    public static MySqlConnection GetConnection()
    {
        var conn = new MySqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
}