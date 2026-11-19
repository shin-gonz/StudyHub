using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudyHub.Models;
using StudyHub.Services;
namespace StudyHub.Desktop;

public partial class Form1 : Form
{
    private readonly AuthenticationService authService = new();
    private readonly ResourceService resourceService = new();
    private readonly CommentService commentService = new();
    private readonly ModerationService moderationService;

    private User? currentUser;
    private readonly Panel content = new();
    private readonly Label heading = new();

    public Form1()
    {
        moderationService = new ModerationService(resourceService, commentService);

        Text = "StudyHub - Learning Resource Library";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1100, 700);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(245, 247, 250);

        ShowLogin();
    }

    private void BuildPage(string title)
    {
        Controls.Clear();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 82,
            BackColor = Color.FromArgb(30, 64, 120)
        };

        heading.Text = title;
        heading.ForeColor = Color.White;
        heading.Font = new Font("Segoe UI", 21, FontStyle.Bold);
        heading.AutoSize = true;
        heading.Location = new Point(28, 20);
        header.Controls.Add(heading);

        content.Dock = DockStyle.Fill;
        content.Padding = new Padding(30);
        content.BackColor = Color.FromArgb(245, 247, 250);
        content.Controls.Clear();

        Controls.Add(content);
        Controls.Add(header);
    }

    private Button MakeButton(string text, Action action, int width = 230)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 46,
            BackColor = Color.FromArgb(37, 99, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(8),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        return button;
    }

    private void ShowLogin()
    {
        currentUser = null;
        BuildPage("StudyHub");

        var panel = new Panel
        {
            Size = new Size(430, 350),
            BackColor = Color.White,
            Padding = new Padding(25),
            BorderStyle = BorderStyle.FixedSingle
        };

        var label = new Label
        {
            Text = "Welcome to StudyHub",
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(35, 25)
        };

        var email = AddTextBox(panel, "Email", 85);
        var password = AddTextBox(panel, "Password", 165, true);

        var login = MakeButton("Login", () =>
        {
            var user = authService.ValidateLogin(email.Text.Trim(), password.Text);
            if (user == null)
            {
                MessageBox.Show("Invalid email or password.", "Login failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentUser = user;
            ShowDashboard();
        }, 350);
        login.Location = new Point(35, 235);

        var register = MakeButton("Register as Student", ShowRegister, 350);
        register.Location = new Point(35, 285);
        register.BackColor = Color.FromArgb(75, 85, 99);

        panel.Controls.Add(label);
        panel.Controls.Add(login);
        panel.Controls.Add(register);

        content.Controls.Add(panel);
        panel.Location = new Point(
            Math.Max(20, (content.ClientSize.Width - panel.Width) / 2),
            35);
        content.Resize += (_, _) =>
        {
            if (panel.Parent == content)
                panel.Left = Math.Max(20, (content.ClientSize.Width - panel.Width) / 2);
        };
    }

    private TextBox AddTextBox(Control parent, string label, int top, bool password = false)
    {
        var caption = new Label
        {
            Text = label,
            Location = new Point(35, top),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        var box = new TextBox
        {
            Location = new Point(35, top + 22),
            Width = 350,
            Height = 30,
            UseSystemPasswordChar = password
        };

        parent.Controls.Add(caption);
        parent.Controls.Add(box);
        return box;
    }

    private void ShowRegister()
    {
        BuildPage("Student Registration");

        var panel = new Panel
        {
            Size = new Size(480, 500),
            BackColor = Color.White,
            Padding = new Padding(25),
            BorderStyle = BorderStyle.FixedSingle
        };

        var name = AddTextBox(panel, "Full Name", 20);
        var email = AddTextBox(panel, "Email", 95);
        var password = AddTextBox(panel, "Password", 170, true);
        var course = AddTextBox(panel, "Course", 245);
        var year = AddTextBox(panel, "Year Level", 320);

        var register = MakeButton("Create Account", () =>
        {
            if (new[] { name.Text, email.Text, password.Text, course.Text, year.Text }
                .Any(string.IsNullOrWhiteSpace))
            {
                MessageBox.Show("Please complete all fields.");
                return;
            }

            var result = authService.RegisterStudent(
                name.Text.Trim(), email.Text.Trim(), password.Text,
                course.Text.Trim(), year.Text.Trim());

            if (result == null)
            {
                MessageBox.Show("That email address is already registered.");
                return;
            }

            MessageBox.Show("Registration successful! You can now log in.",
                "StudyHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ShowLogin();
        });
        register.Location = new Point(35, 395);

        var back = MakeButton("Back to Login", ShowLogin, 350);
        back.Location = new Point(35, 450);
        back.BackColor = Color.FromArgb(75, 85, 99);

        panel.Controls.Add(register);
        panel.Controls.Add(back);
        content.Controls.Add(panel);
        panel.Location = new Point(
            Math.Max(20, (content.ClientSize.Width - panel.Width) / 2), 10);
    }

    private void ShowDashboard()
    {
        if (currentUser == null)
        {
            ShowLogin();
            return;
        }

        BuildPage("StudyHub Dashboard");

        var welcome = new Label
        {
            Text = $"Welcome, {currentUser.GetFullName()} ({currentUser.GetType().Name})",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 20)
        };
        content.Controls.Add(welcome);

        var menu = new FlowLayoutPanel
        {
            Location = new Point(20, 75),
            Size = new Size(850, 480),
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };
        content.Controls.Add(menu);

        if (currentUser is Student)
        {
            AddMenuButton(menu, "Browse Resources", BrowseResources);
            AddMenuButton(menu, "Search Resources", SearchResources);
            AddMenuButton(menu, "View Resource by ID", ViewResourceById);
            AddMenuButton(menu, "Upload Resource", UploadResource);
            AddMenuButton(menu, "My Uploaded Resources", ManageMyResources);
            AddMenuButton(menu, "My Bookmarks", ShowBookmarks);
            AddMenuButton(menu, "Recently Viewed", ShowRecentlyViewed);
            AddMenuButton(menu, "Submit a Report", SubmitReport);
            AddMenuButton(menu, "Update Profile", UpdateProfile);
        }
        else if (currentUser.CanModerateContent())
        {
            AddMenuButton(menu, "Pending Reports", ShowPendingReports);
            AddMenuButton(menu, "Remove Reported Resource", RemoveReportedResource);
            AddMenuButton(menu, "Remove Reported Comment", RemoveReportedComment);
            AddMenuButton(menu, "Dismiss Report", DismissReport);
            AddMenuButton(menu, "Browse All Resources", BrowseResources);
        }

        AddMenuButton(menu, "Logout", ShowLogin, Color.FromArgb(100, 100, 100));
    }

    private void AddMenuButton(FlowLayoutPanel panel, string text, Action action,
        Color? color = null)
    {
        var button = MakeButton(text, action, 250);
        if (color.HasValue)
            button.BackColor = color.Value;
        panel.Controls.Add(button);
    }

    private void BrowseResources()
    {
        ShowResourceList(resourceService.BrowseResources(), "All Learning Resources");
    }

    private void SearchResources()
    {
        string keyword = Ask("Search", "Enter a title or subject keyword:");
        if (keyword == null) return;

        ShowResourceList(resourceService.SearchResources(keyword), "Search Results");
    }

    private void ShowBookmarks()
    {
        if (currentUser is Student student)
            ShowResourceList(
                resourceService.GetBookmarkedResources(student.GetUserId()),
                "My Bookmarks");
    }

    private void ShowRecentlyViewed()
    {
        if (currentUser is Student student)
            ShowResourceList(
                resourceService.GetRecentlyViewed(student.GetUserId()),
                "Recently Viewed");
    }

    private void ShowResourceList(System.Collections.Generic.List<Resource> resources,
        string title)
    {
        BuildPage(title);

        var list = new ListBox
        {
            Dock = DockStyle.Top,
            Height = 390,
            Font = new Font("Segoe UI", 10)
        };

        foreach (var resource in resources)
            list.Items.Add($"{resource.ResourceId} - {resource.GetDetails().Replace(
                Environment.NewLine, " | ")}");

        var details = MakeButton("View Selected Resource", () =>
        {
            if (list.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a resource.");
                return;
            }

            ShowResourceDetails(resources[list.SelectedIndex]);
        }, 260);

        var back = MakeButton("Back to Dashboard", ShowDashboard, 260);
        back.BackColor = Color.FromArgb(75, 85, 99);

        content.Controls.Add(list);
        content.Controls.Add(details);
        content.Controls.Add(back);
        details.Location = new Point(30, 415);
        back.Location = new Point(310, 415);

        if (resources.Count == 0)
            MessageBox.Show("No resources found.");
    }

    private void ViewResourceById()
    {
        string input = Ask("View Resource", "Enter Resource ID:");
        if (!int.TryParse(input, out int id)) return;

        var resource = resourceService.GetById(id);
        if (resource == null)
        {
            MessageBox.Show("Resource not found.");
            return;
        }

        ShowResourceDetails(resource);
    }

    private void ShowResourceDetails(Resource resource)
    {
        if (currentUser is Student student)
            resourceService.RecordView(student.GetUserId(), resource.ResourceId);

        BuildPage("Resource Details");

        var details = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 11),
            Text = resource.GetDetails(),
            Location = new Point(25, 20),
            Size = new Size(800, 220)
        };
        content.Controls.Add(details);

        var comments = commentService.GetCommentsByResource(resource.ResourceId);
        var commentList = new ListBox
        {
            Location = new Point(25, 255),
            Size = new Size(800, 150)
        };

        foreach (var comment in comments)
            commentList.Items.Add(comment.GetDetails());

        content.Controls.Add(new Label
        {
            Text = "Comments",
            Location = new Point(25, 235),
            AutoSize = true,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        });
        content.Controls.Add(commentList);

        if (currentUser is Student currentStudent)
        {
            var bookmark = MakeButton("Toggle Bookmark", () =>
            {
                bool added = resourceService.ToggleBookmark(
                    currentStudent.GetUserId(), resource.ResourceId);
                MessageBox.Show(added ? "Bookmarked!" : "Bookmark removed.");
            });
            bookmark.Location = new Point(25, 425);

            var addComment = MakeButton("Add Comment", () =>
            {
                string text = Ask("Add Comment", "Write your comment:");
                if (string.IsNullOrWhiteSpace(text)) return;

                commentService.AddComment(resource.ResourceId,
                    currentStudent.GetUserId(), text);
                MessageBox.Show("Comment posted!");
                ShowResourceDetails(resource);
            });
            addComment.Location = new Point(275, 425);

            var editComment = MakeButton("Edit My Comment", () =>
                EditOrDeleteComment(currentStudent, false));
            editComment.Location = new Point(525, 425);

            var deleteComment = MakeButton("Delete My Comment", () =>
                EditOrDeleteComment(currentStudent, true));
            deleteComment.Location = new Point(25, 480);

            content.Controls.AddRange(new Control[]
                { bookmark, addComment, editComment, deleteComment });
        }

        var back = MakeButton("Back", ShowDashboard, 180);
        back.Location = new Point(275, 480);
        back.BackColor = Color.FromArgb(75, 85, 99);
        content.Controls.Add(back);
    }

    private void EditOrDeleteComment(Student student, bool delete)
    {
        string input = Ask("Comment", "Enter Comment ID:");
        if (!int.TryParse(input, out int commentId)) return;

        if (delete)
        {
            bool ok = commentService.DeleteComment(commentId, student.GetUserId());
            MessageBox.Show(ok ? "Comment deleted." :
                "Comment not found or you do not own it.");
        }
        else
        {
            string text = Ask("Edit Comment", "Enter the new comment:");
            if (text == null) return;

            bool ok = commentService.EditComment(
                commentId, student.GetUserId(), text);
            MessageBox.Show(ok ? "Comment updated." :
                "Comment not found or you do not own it.");
        }
    }

    private void UploadResource()
    {
        if (currentUser is not Student student) return;

        string title = Ask("Upload Resource", "Title:");
        if (title == null) return;
        string description = Ask("Upload Resource", "Description:");
        if (description == null) return;
        string subject = Ask("Upload Resource", "Subject:");
        if (subject == null) return;
        string author = Ask("Upload Resource", "Author Name:");
        if (author == null) return;
        string type = Ask("Upload Resource", "Type (PDF or Link):");
        if (type == null) return;

        type = type.Trim().Equals("PDF", StringComparison.OrdinalIgnoreCase)
            ? "PDF" : "Link";

        string path;
        if (type == "PDF")
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select a PDF resource"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            path = dialog.FileName;
        }
        else
        {
            path = Ask("Upload Resource", "External URL:");
            if (path == null) return;
        }

        resourceService.UploadResource(student.GetUserId(),
            title, description, subject, author, type, path);

        MessageBox.Show("Resource published successfully!");
    }

    private void ManageMyResources()
    {
        if (currentUser is not Student student) return;

        var mine = resourceService.BrowseResources()
            .Where(r => r.IsOwnedBy(student.GetUserId())).ToList();

        BuildPage("My Uploaded Resources");

        var list = new ListBox
        {
            Location = new Point(25, 20),
            Size = new Size(800, 380)
        };

        foreach (var resource in mine)
            list.Items.Add($"{resource.ResourceId} - {resource.GetDetails()}");

        var edit = MakeButton("Edit Selected", () =>
        {
            if (list.SelectedIndex < 0) return;
            EditResource(mine[list.SelectedIndex]);
        });
        edit.Location = new Point(25, 420);

        var delete = MakeButton("Delete Selected", () =>
        {
            if (list.SelectedIndex < 0) return;
            var resource = mine[list.SelectedIndex];

            if (MessageBox.Show("Delete this resource?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            resourceService.DeleteResource(resource.ResourceId);
            MessageBox.Show("Resource deleted.");
            ManageMyResources();
        });
        delete.Location = new Point(275, 420);

        var back = MakeButton("Back", ShowDashboard, 180);
        back.Location = new Point(525, 420);
        back.BackColor = Color.FromArgb(75, 85, 99);

        content.Controls.AddRange(new Control[] { list, edit, delete, back });

        if (mine.Count == 0)
            MessageBox.Show("You have not uploaded any resources yet.");
    }

    private void EditResource(Resource resource)
    {
        string title = Ask("Edit Resource", "New Title:");
        if (title == null) return;
        string description = Ask("Edit Resource", "New Description:");
        if (description == null) return;
        string subject = Ask("Edit Resource", "New Subject:");
        if (subject == null) return;
        string author = Ask("Edit Resource", "New Author:");
        if (author == null) return;
        string path = Ask("Edit Resource", "New Path/URL:");
        if (path == null) return;

        resource.Update(title, description, subject, author, path);
        resourceService.UpdateResource(resource);
        MessageBox.Show("Resource updated!");
        ManageMyResources();
    }

    private void UpdateProfile()
    {
        if (currentUser is not Student student) return;

        string name = Ask("Update Profile",
            $"Current name: {student.GetFullName()}\nNew Full Name (blank to keep):") ?? "";
        string course = Ask("Update Profile",
            $"Current course: {student.Course}\nNew Course (blank to keep):") ?? "";
        string year = Ask("Update Profile",
            $"Current year: {student.YearLevel}\nNew Year Level (blank to keep):") ?? "";

        student.UpdateProfile(name, course, year);
        authService.UpdateStudentProfile(student);
        MessageBox.Show("Profile updated!");
        ShowDashboard();
    }

    private void SubmitReport()
    {
        if (currentUser is not Student student) return;

        string typeInput = Ask("Submit Report", "Report [1] Resource or [2] Comment?");
        if (typeInput == null) return;

        string type = typeInput.Trim() == "2" ? "Comment" : "Resource";
        string idInput = Ask("Submit Report", $"Enter {type} ID:");
        if (!int.TryParse(idInput, out int targetId)) return;

        string reason = Ask("Submit Report", "Reason for report:");
        if (reason == null) return;

        moderationService.SubmitReport(
            student.GetUserId(), targetId, type, reason);
        MessageBox.Show("Report submitted for administrator review.");
    }

    private void ShowPendingReports()
    {
        var reports = moderationService.GetPendingReports();
        BuildPage("Pending Reports");

        var list = new ListBox
        {
            Location = new Point(25, 20),
            Size = new Size(820, 400)
        };

        foreach (var report in reports)
        {
            list.Items.Add(
                $"Report #{report.ReportId} | {report.TargetType} #{report.TargetId} | " +
                $"{report.Reason} | {report.DateReported:g}");
        }

        content.Controls.Add(list);
        var back = MakeButton("Back to Dashboard", ShowDashboard);
        back.Location = new Point(25, 440);
        content.Controls.Add(back);

        if (reports.Count == 0)
            MessageBox.Show("No pending reports.");
    }

    private void RemoveReportedResource()
    {
        string input = Ask("Moderation", "Enter Report ID to remove Resource:");
        if (!int.TryParse(input, out int id)) return;

        bool ok = moderationService.RemoveReportedResource(id);
        MessageBox.Show(ok ? "Resource removed and report resolved." :
            "Invalid Resource report ID.");
    }

    private void RemoveReportedComment()
    {
        string input = Ask("Moderation", "Enter Report ID to remove Comment:");
        if (!int.TryParse(input, out int id)) return;

        bool ok = moderationService.RemoveReportedComment(id);
        MessageBox.Show(ok ? "Comment removed and report resolved." :
            "Invalid Comment report ID.");
    }

    private void DismissReport()
    {
        string input = Ask("Moderation", "Enter Report ID to dismiss:");
        if (!int.TryParse(input, out int id)) return;

        bool ok = moderationService.DismissReport(id);
        MessageBox.Show(ok ? "Report dismissed." : "Report not found.");
    }

    private static string? Ask(string title, string prompt)
    {
        using var form = new Form
        {
            Text = title,
            Size = new Size(430, 190),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label
        {
            Text = prompt,
            Left = 15,
            Top = 15,
            Width = 380,
            Height = 45
        };

        var input = new TextBox
        {
            Left = 15,
            Top = 65,
            Width = 380
        };

        var ok = new Button
        {
            Text = "OK",
            Left = 230,
            Top = 105,
            Width = 75,
            DialogResult = DialogResult.OK
        };

        var cancel = new Button
        {
            Text = "Cancel",
            Left = 320,
            Top = 105,
            Width = 75,
            DialogResult = DialogResult.Cancel
        };

        form.Controls.AddRange(new Control[] { label, input, ok, cancel });
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        return form.ShowDialog() == DialogResult.OK ? input.Text : null;
    }
}
