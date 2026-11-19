namespace StudyHub.Models;

public class Bookmark
{
    public int BookmarkId { get; }
    public int StudentId { get; }
    public int ResourceId { get; }
    public DateTime DateBookmarked { get; }

    public Bookmark(int bookmarkId, int studentId, int resourceId)
    {
        BookmarkId = bookmarkId;
        StudentId = studentId;
        ResourceId = resourceId;
        DateBookmarked = DateTime.Now;
    }
}