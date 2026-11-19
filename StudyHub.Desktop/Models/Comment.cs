namespace StudyHub.Models;

public class Comment
{
    public int CommentId { get; }
    public int ResourceId { get; }
    public int StudentId { get; }
    public string StudentName { get; }
    public string Content { get; private set; }
    public DateTime DatePosted { get; }

    public Comment(int commentId, int resourceId, int studentId, string studentName, string content, DateTime? datePosted = null)
{
    CommentId = commentId;
    ResourceId = resourceId;
    StudentId = studentId;
    StudentName = studentName;
    Content = content;
    DatePosted = datePosted ?? DateTime.Now;
}

    public void Edit(string newContent)
    {
        if (!string.IsNullOrWhiteSpace(newContent))
            Content = newContent;
    }

    public string GetDetails() =>
        $"  (Comment #{CommentId}) {StudentName} [{DatePosted:g}]: {Content}";
}