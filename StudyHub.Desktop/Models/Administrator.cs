namespace StudyHub.Models;

public class Administrator : User
{
    public Administrator(int userId, string fullName, string email, string password)
        : base(userId, fullName, email, password) { }

    public override List<string> GetDashboardOptions() => new()
    {
        "1. Review Pending Reports",
        "2. Remove Reported Resource",
        "3. Remove Reported Comment",
        "4. Dismiss a Report",
        "5. Browse All Resources",
        "0. Logout"
    };

    public override bool CanModerateContent() => true;
}