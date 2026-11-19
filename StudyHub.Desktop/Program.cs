using StudyHub.Models;
using StudyHub.Services;

var authService = new AuthenticationService();
var resourceService = new ResourceService();
var commentService = new CommentService();
var moderationService = new ModerationService(resourceService, commentService);

User? currentUser = null;

while (true)
{
    Console.Clear();
    Console.WriteLine("=========================================");
    Console.WriteLine("   STUDYHUB - Learning Resource Library  ");
    Console.WriteLine("=========================================");

    if (currentUser == null)
    {
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Register (Student)");
        Console.WriteLine("0. Exit");
        Console.Write("\nSelect option: ");
        var choice = Console.ReadLine();

        if (choice == "1")
        {
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string password = Console.ReadLine() ?? "";

            currentUser = authService.ValidateLogin(email, password);
            if (currentUser == null)
                Pause("Invalid email or password.");
        }
        else if (choice == "2")
        {
            Console.Write("Full Name: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string pass = Console.ReadLine() ?? "";
            Console.Write("Course: ");
            string course = Console.ReadLine() ?? "";
            Console.Write("Year Level: ");
            string year = Console.ReadLine() ?? "";

            var newStudent = authService.RegisterStudent(name, email, pass, course, year);
            Pause(newStudent != null ? "Registration successful! You can now log in." : "Email already exists.");
        }
        else if (choice == "0")
        {
            break;
        }
    }
    else
    {
        Console.WriteLine($"Welcome, {currentUser.GetFullName()} ({currentUser.GetType().Name})");
        Console.WriteLine("-----------------------------------------");

        foreach (var option in currentUser.GetDashboardOptions())
            Console.WriteLine(option);

        Console.Write("\nSelect option: ");
        var choice = Console.ReadLine();

        if (choice == "0")
        {
            currentUser = null;
            continue;
        }

        if (currentUser is Student student)
            HandleStudentMenu(choice, student);
        else if (currentUser.CanModerateContent())
            HandleAdminMenu(choice);
    }
}

void HandleStudentMenu(string? choice, Student student)
{
    switch (choice)
    {
        case "1":
            DisplayResources(resourceService.BrowseResources());
            Pause();
            break;

        case "2":
            Console.Write("Enter title or subject keyword: ");
            string kw = Console.ReadLine() ?? "";
            DisplayResources(resourceService.SearchResources(kw));
            Pause();
            break;

        case "3":
            Console.Write("Enter Resource ID: ");
            if (int.TryParse(Console.ReadLine(), out int resId))
            {
                var res = resourceService.GetById(resId);
                if (res == null)
                {
                    Pause("Resource not found.");
                    return;
                }

                resourceService.RecordView(student.GetUserId(), res.ResourceId);
                Console.WriteLine("\n" + res.GetDetails());
                Console.WriteLine("\n--- Comments ---");
                var comments = commentService.GetCommentsByResource(res.ResourceId);
                if (comments.Count == 0)
                    Console.WriteLine("  No comments yet.");
                foreach (var c in comments)
                    Console.WriteLine(c.GetDetails());

                Console.WriteLine("\nActions: [B] Toggle Bookmark | [C] Add Comment | [E] Edit My Comment | [D] Delete My Comment | [Enter] Back");
                var act = Console.ReadLine()?.ToUpper();
                if (act == "B")
                {
                    bool added = resourceService.ToggleBookmark(student.GetUserId(), res.ResourceId);
                    Pause(added ? "Bookmarked!" : "Bookmark removed.");
                }
                else if (act == "C")
                {
                    Console.Write("Write comment: ");
                    string text = Console.ReadLine() ?? "";
                    commentService.AddComment(res.ResourceId, student.GetUserId(), text);
                    Pause("Comment posted!");
                }
                else if (act == "E" || act == "D")
                {
                    Console.Write("Enter Comment ID: ");
                    if (int.TryParse(Console.ReadLine(), out int cid))
                    {
                        if (act == "E")
                        {
                            Console.Write("New comment text: ");
                            bool ok = commentService.EditComment(cid, student.GetUserId(), Console.ReadLine() ?? "");
                            Pause(ok ? "Comment updated." : "Comment not found or you do not own it.");
                        }
                        else
                        {
                            bool ok = commentService.DeleteComment(cid, student.GetUserId());
                            Pause(ok ? "Comment deleted." : "Comment not found or you do not own it.");
                        }
                    }
                }
            }
            break;

        case "4":
            Console.Write("Title: ");
            string title = Console.ReadLine() ?? "";
            Console.Write("Description: ");
            string desc = Console.ReadLine() ?? "";
            Console.Write("Subject: ");
            string subj = Console.ReadLine() ?? "";
            Console.Write("Author Name: ");
            string author = Console.ReadLine() ?? "";
            Console.Write("Type (PDF or Link): ");
            string type = Console.ReadLine()?.ToUpper() == "PDF" ? "PDF" : "Link";

            Console.Write(type == "PDF" ? "PDF File Path (must end in .pdf): " : "External URL: ");
            string path = Console.ReadLine() ?? "";

            if (type == "PDF" && !path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                Pause("Error: Only .pdf files are allowed.");
                return;
            }

            resourceService.UploadResource(student.GetUserId(), title, desc, subj, author, type, path);
            Pause("Resource published immediately!");
            break;

        case "5":
            var mine = resourceService.BrowseResources().Where(r => r.IsOwnedBy(student.GetUserId())).ToList();
            DisplayResources(mine);
            if (mine.Count > 0)
            {
                Console.Write("\nEnter Resource ID to Edit/Delete (or 0 to cancel): ");
                if (int.TryParse(Console.ReadLine(), out int myResId) && myResId != 0)
                {
                    var target = resourceService.GetById(myResId);
                    if (target != null && target.IsOwnedBy(student.GetUserId()))
                    {
                        Console.Write("[E] Edit or [D] Delete? ");
                        var op = Console.ReadLine()?.ToUpper();
                        if (op == "E")
                        {
                            Console.Write("New Title: ");
                            string t = Console.ReadLine() ?? "";
                            Console.Write("New Description: ");
                            string d = Console.ReadLine() ?? "";
                            Console.Write("New Subject: ");
                            string s = Console.ReadLine() ?? "";
                            Console.Write("New Author: ");
                            string a = Console.ReadLine() ?? "";
                            Console.Write("New Path/URL: ");
                            string p = Console.ReadLine() ?? "";
                            target.Update(t, d, s, a, p);
                            resourceService.UpdateResource(target);
                            Pause("Resource updated!");
                        }
                        else if (op == "D")
                        {
                            resourceService.DeleteResource(myResId);
                            Pause("Resource deleted!");
                        }
                    }
                }
            }
            else
            {
                Pause();
            }
            break;

        case "6":
            DisplayResources(resourceService.GetBookmarkedResources(student.GetUserId()));
            Pause();
            break;

        case "7":
            DisplayResources(resourceService.GetRecentlyViewed(student.GetUserId()));
            Pause();
            break;

        case "8":
            Console.Write("Report [1] Resource or [2] Comment? ");
            string rType = Console.ReadLine() == "2" ? "Comment" : "Resource";
            Console.Write($"Enter {rType} ID: ");
            if (int.TryParse(Console.ReadLine(), out int targetId))
            {
                Console.Write("Reason for report: ");
                string reason = Console.ReadLine() ?? "";
                moderationService.SubmitReport(student.GetUserId(), targetId, rType, reason);
                Pause("Report submitted for administrator review.");
            }
            break;

        case "9":
            Console.WriteLine($"Current: {student.GetFullName()} | {student.Course} | {student.YearLevel}");
            Console.Write("New Full Name (leave blank to keep): ");
            string newName = Console.ReadLine() ?? "";
            Console.Write("New Course: ");
            string newCourse = Console.ReadLine() ?? "";
            Console.Write("New Year Level: ");
            string newYear = Console.ReadLine() ?? "";
            student.UpdateProfile(newName, newCourse, newYear);
            authService.UpdateStudentProfile(student);
            Pause("Profile updated!");
            break;
    }
}

void HandleAdminMenu(string? choice)
{
    switch (choice)
    {
        case "1":
            var reports = moderationService.GetPendingReports();
            if (reports.Count == 0)
                Console.WriteLine("No pending reports.");
            foreach (var r in reports)
                Console.WriteLine($"Report #{r.ReportId} | Target: {r.TargetType} #{r.TargetId} | Reason: {r.Reason} | Date: {r.DateReported:g}");
            Pause();
            break;

        case "2":
            Console.Write("Enter Report ID to remove Resource: ");
            if (int.TryParse(Console.ReadLine(), out int rId))
                Pause(moderationService.RemoveReportedResource(rId) ? "Resource removed & report resolved." : "Invalid Resource report ID.");
            break;

        case "3":
            Console.Write("Enter Report ID to remove Comment: ");
            if (int.TryParse(Console.ReadLine(), out int cId))
                Pause(moderationService.RemoveReportedComment(cId) ? "Comment removed & report resolved." : "Invalid Comment report ID.");
            break;

        case "4":
            Console.Write("Enter Report ID to dismiss: ");
            if (int.TryParse(Console.ReadLine(), out int dId))
                Pause(moderationService.DismissReport(dId) ? "Report dismissed." : "Report not found.");
            break;

        case "5":
            DisplayResources(resourceService.BrowseResources());
            Pause();
            break;
    }
}

void DisplayResources(List<Resource> list)
{
    Console.WriteLine();
    if (list.Count == 0)
    {
        Console.WriteLine("No resources found.");
        return;
    }
    foreach (var r in list)
        Console.WriteLine(r.GetDetails() + "\n");
}

void Pause(string message = "")
{
    if (!string.IsNullOrEmpty(message))
        Console.WriteLine("\n" + message);
    Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}