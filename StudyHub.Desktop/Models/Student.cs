namespace StudyHub.Models;

public class Student : User
{
    public string Course { get; private set; }
    public string YearLevel { get; private set; }
    public List<int> RecentlyViewedResourceIds { get; } = new();

    public Student(int userId, string fullName, string email, string password, string course, string yearLevel)
        : base(userId, fullName, email, password)
    {
        Course = course;
        YearLevel = yearLevel;
    }

    public void UpdateProfile(string fullName, string course, string yearLevel)
    {
        SetFullName(fullName);
        if (!string.IsNullOrWhiteSpace(course)) Course = course;
        if (!string.IsNullOrWhiteSpace(yearLevel)) YearLevel = yearLevel;
    }

    public void AddToRecentlyViewed(int resourceId)
    {
        RecentlyViewedResourceIds.Remove(resourceId);
        RecentlyViewedResourceIds.Insert(0, resourceId);
        if (RecentlyViewedResourceIds.Count > 5)
            RecentlyViewedResourceIds.RemoveAt(5);
    }

    public override List<string> GetDashboardOptions() => new()
    {
        "1. Browse All Resources",
        "2. Search Resources (Title/Subject)",
        "3. View Resource Details, Comment & Bookmark",
        "4. Upload New Resource (PDF/Link)",
        "5. Manage My Uploaded Resources",
        "6. View My Bookmarks",
        "7. View Recently Viewed History",
        "8. Report a Resource or Comment",
        "9. Update My Profile",
        "0. Logout"
    };

    public override bool CanModerateContent() => false;
}