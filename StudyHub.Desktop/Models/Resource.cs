namespace StudyHub.Models;

public class Resource
{
    public int ResourceId { get; }
    public int StudentId { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Subject { get; private set; }
    public string AuthorName { get; private set; }
    public string ResourceType { get; private set; } // "PDF" or "Link"
    public string FilePathOrUrl { get; private set; }
    public DateTime DateUploaded { get; }

    public Resource(int resourceId, int studentId, string title, string description,
                string subject, string authorName, string resourceType, string filePathOrUrl, DateTime? dateUploaded = null)
{
    ResourceId = resourceId;
    StudentId = studentId;
    Title = title;
    Description = description;
    Subject = subject;
    AuthorName = authorName;
    ResourceType = resourceType;
    FilePathOrUrl = filePathOrUrl;
    DateUploaded = dateUploaded ?? DateTime.Now;
}

    public void Update(string title, string description, string subject, string authorName, string filePathOrUrl)
    {
        if (!string.IsNullOrWhiteSpace(title)) Title = title;
        if (!string.IsNullOrWhiteSpace(description)) Description = description;
        if (!string.IsNullOrWhiteSpace(subject)) Subject = subject;
        if (!string.IsNullOrWhiteSpace(authorName)) AuthorName = authorName;
        if (!string.IsNullOrWhiteSpace(filePathOrUrl)) FilePathOrUrl = filePathOrUrl;
    }

    public bool IsOwnedBy(int studentId) => StudentId == studentId;

    public string GetDetails() =>
        $"[{ResourceId}] {Title} ({ResourceType})\n" +
        $"    Subject: {Subject} | Author: {AuthorName} | Uploaded: {DateUploaded:yyyy-MM-dd}\n" +
        $"    Description: {Description}\n" +
        $"    Location: {FilePathOrUrl}";
}