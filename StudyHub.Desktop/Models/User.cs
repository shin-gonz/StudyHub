namespace StudyHub.Models;

public abstract class User
{
    private int _userId;
    private string _fullName;
    private string _email;
    private string _password;

    protected User(int userId, string fullName, string email, string password)
    {
        _userId = userId;
        _fullName = fullName;
        _email = email;
        _password = password;
    }

    public int GetUserId() => _userId;
    public string GetFullName() => _fullName;
    public string GetEmail() => _email;

    protected void SetFullName(string fullName)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            _fullName = fullName;
    }

    public bool VerifyPassword(string inputPassword) => _password == inputPassword;

    // Polymorphic methods implemented differently by Student and Administrator
    public abstract List<string> GetDashboardOptions();
    public abstract bool CanModerateContent();
}