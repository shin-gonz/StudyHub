using System;
using System.Collections.Generic;
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
    private string activeNav = "Browse & Search";
    private string searchFilter = "";

    public Form1()
    {
        moderationService = new ModerationService(resourceService, commentService);

        Text = "StudyHub - Learning Resource Library";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1024, 680);
        Size = new Size(1340, 820);
        Font = new Font("Segoe UI", 10);
        BackColor = StudyHubTheme.CanvasBg;

        ShowAuthScreen(registerMode: false);
    }

    // =========================================================
    // 1. SPLIT-SCREEN LOGIN & REGISTRATION
    // =========================================================

    private void ShowAuthScreen(bool registerMode)
    {
        currentUser = null;
        Controls.Clear();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
        Controls.Add(root);

        // Left Brand Panel (Phthalo Green + Deep Space Sparkle + Mustard Green)
        var leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudyHubTheme.PhthaloGreen,
            Padding = new Padding(36)
        };
        root.Controls.Add(leftPanel, 0, 0);

        var badge = new Panel
        {
            Size = new Size(44, 44),
            Location = new Point(36, 36),
            BackColor = StudyHubTheme.MustardGreen
        };
        var badgeIcon = new Label
        {
            Text = "📖",
            Font = new Font("Segoe UI", 16),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(7, 6)
        };
        badge.Controls.Add(badgeIcon);

        var brandName = new Label
        {
            Text = "StudyHub",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(90, 32)
        };

        var sdgLabel = new Label
        {
            Text = "SDG 4 • QUALITY EDUCATION",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.MoonstoneBlue,
            AutoSize = true,
            Location = new Point(92, 62)
        };

        var heroTitle = new Label
        {
            Text = "A Learning\nCommunity Platform",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(36, 124)
        };

        var heroDesc = new Label
        {
            Text = "Discover, share, organize, and discuss free\neducational resources in one centralized student\nlibrary.",
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = StudyHubTheme.LightGray,
            AutoSize = true,
            Location = new Point(36, 214)
        };

        leftPanel.Controls.AddRange(new Control[]
        {
            badge, brandName, sdgLabel, heroTitle, heroDesc
        });

        // Right Auth Form Panel
        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            AutoScroll = true
        };
        root.Controls.Add(rightPanel, 1, 0);

        var formWrap = new Panel
        {
            Size = new Size(460, registerMode ? 650 : 470),
            Location = new Point(64, 48),
            BackColor = Color.White
        };
        rightPanel.Controls.Add(formWrap);

        // Mode switcher pill
        var switcherBg = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(440, 44),
            BackColor = StudyHubTheme.SoftFieldBg,
            BorderStyle = BorderStyle.FixedSingle
        };

        var loginTab = new Button
        {
            Text = "Log In",
            Location = new Point(4, 4),
            Size = new Size(214, 34)
        };
        var regTab = new Button
        {
            Text = "Student Registration",
            Location = new Point(220, 4),
            Size = new Size(214, 34)
        };

        if (!registerMode)
        {
            StudyHubTheme.StyleDarkButton(loginTab);
            regTab.BackColor = Color.Transparent;
            regTab.ForeColor = StudyHubTheme.TextMuted;
            regTab.FlatStyle = FlatStyle.Flat;
            regTab.FlatAppearance.BorderSize = 0;
            regTab.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            regTab.Cursor = Cursors.Hand;
        }
        else
        {
            StudyHubTheme.StyleDarkButton(regTab);
            loginTab.BackColor = Color.Transparent;
            loginTab.ForeColor = StudyHubTheme.TextMuted;
            loginTab.FlatStyle = FlatStyle.Flat;
            loginTab.FlatAppearance.BorderSize = 0;
            loginTab.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            loginTab.Cursor = Cursors.Hand;
        }

        loginTab.Click += (_, _) => ShowAuthScreen(registerMode: false);
        regTab.Click += (_, _) => ShowAuthScreen(registerMode: true);
        switcherBg.Controls.AddRange(new Control[] { loginTab, regTab });
        formWrap.Controls.Add(switcherBg);

        if (!registerMode)
        {
            var heading = new Label
            {
                Text = "Sign in to your account",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 68)
            };
            var sub = new Label
            {
                Text = "Users must log in to browse, share, or moderate resources.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = StudyHubTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 102)
            };

            var emailBox = MakeLabeledInput(formWrap, "EMAIL ADDRESS", "student@studyhub.edu", 138, 440, false);
            var passBox = MakeLabeledInput(formWrap, "PASSWORD", "••••••••", 224, 440, true);

            var submitBtn = new Button
            {
                Text = "Log In",
                Location = new Point(0, 322),
                Size = new Size(440, 44)
            };
            StudyHubTheme.StyleDarkButton(submitBtn);
            submitBtn.Click += (_, _) => AttemptLogin(emailBox.Text.Trim(), passBox.Text);

            formWrap.Controls.AddRange(new Control[] { heading, sub, submitBtn });
        }
        else
        {
            var heading = new Label
            {
                Text = "Create a student account",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                AutoSize = true,
                Location = new Point(0, 64)
            };
            var sub = new Label
            {
                Text = "Register with your academic details to share and bookmark study notes.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = StudyHubTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 96)
            };

            var nameBox = MakeLabeledInput(formWrap, "FULL NAME", "Juan Dela Cruz", 128, 440, false);
            var emailBox = MakeLabeledInput(formWrap, "EMAIL ADDRESS", "juan@student.edu", 198, 440, false);
            var passBox = MakeLabeledInput(formWrap, "PASSWORD", "Create a password", 268, 440, true);
            var courseBox = MakeLabeledInput(formWrap, "COURSE", "BS Computer Science", 338, 212, false);
            var yearBox = MakeLabeledInput(formWrap, "YEAR LEVEL", "2nd Year", 338, 212, false, 228);

            var regBtn = new Button
            {
                Text = "Create Student Account",
                Location = new Point(0, 424),
                Size = new Size(440, 44)
            };
            StudyHubTheme.StyleAccentButton(regBtn);
            regBtn.Click += (_, _) =>
            {
                if (new[] { nameBox.Text, emailBox.Text, passBox.Text, courseBox.Text, yearBox.Text }
                    .Any(string.IsNullOrWhiteSpace))
                {
                    MessageBox.Show("Please complete all registration fields.");
                    return;
                }

                var created = authService.RegisterStudent(
                    nameBox.Text.Trim(), emailBox.Text.Trim(), passBox.Text,
                    courseBox.Text.Trim(), yearBox.Text.Trim());

                if (created == null)
                {
                    MessageBox.Show("That email is already registered.");
                    return;
                }

                MessageBox.Show("Registration successful! You can now log in.");
                ShowAuthScreen(registerMode: false);
            };

            formWrap.Controls.AddRange(new Control[] { heading, sub, regBtn });
        }
    }

    private TextBox MakeLabeledInput(Control parent, string captionText, string placeholder,
        int top, int width, bool isPassword, int left = 0)
    {
        var lbl = new Label
        {
            Text = captionText,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextMuted,
            AutoSize = true,
            Location = new Point(left, top)
        };

        var box = new TextBox
        {
            Location = new Point(left, top + 20),
            Size = new Size(width, 36),
            BackColor = StudyHubTheme.SoftFieldBg,
            ForeColor = StudyHubTheme.TextDark,
            BorderStyle = BorderStyle.FixedSingle,
            UseSystemPasswordChar = isPassword,
            PlaceholderText = placeholder
        };

        parent.Controls.AddRange(new Control[] { lbl, box });
        return box;
    }

    private void AttemptLogin(string email, string password)
    {
        var user = authService.ValidateLogin(email, password);
        if (user == null)
        {
            MessageBox.Show("Invalid email or password.", "Login Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        currentUser = user;
        searchFilter = "";
        activeNav = user is Administrator ? "Reported Content Queue" : "Browse & Search";
        ShowWorkspace();
    }

    // =========================================================
    // 2. SIDEBAR + MAIN WORKSPACE SHELL
    // =========================================================

    private void ShowWorkspace()
    {
        if (currentUser == null)
        {
            ShowAuthScreen(false);
            return;
        }

        Controls.Clear();

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230f));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        Controls.Add(shell);

        // Left Sidebar
        var sidebar = BuildSidebar();
        shell.Controls.Add(sidebar, 0, 0);

        // Right Main Column (Top Bar + Scrollable Content)
        var rightCol = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudyHubTheme.CanvasBg
        };
        shell.Controls.Add(rightCol, 1, 0);

        var topBar = BuildTopBar();
        rightCol.Controls.Add(topBar);

        var bodyScroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(24),
            BackColor = StudyHubTheme.CanvasBg
        };
        rightCol.Controls.Add(bodyScroll);
        bodyScroll.BringToFront();

        if (currentUser is Administrator)
            RenderAdminBody(bodyScroll);
        else if (currentUser is Student student)
            RenderStudentBody(bodyScroll, student);
    }

    private Panel BuildSidebar()
    {
        var side = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = StudyHubTheme.PhthaloGreen
        };

        // Brand Header
        var brandBox = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(230, 68),
            BackColor = StudyHubTheme.PhthaloGreen
        };
        var logoSquare = new Panel
        {
            Location = new Point(16, 16),
            Size = new Size(34, 34),
            BackColor = StudyHubTheme.MustardGreen
        };
        var logoIcon = new Label
        {
            Text = "📖",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(6, 5)
        };
        logoSquare.Controls.Add(logoIcon);

        var titleLbl = new Label
        {
            Text = "StudyHub",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(58, 13)
        };
        var portalLbl = new Label
        {
            Text = currentUser is Administrator ? "ADMIN CONSOLE" : "STUDENT PORTAL",
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.MoonstoneBlue,
            AutoSize = true,
            Location = new Point(59, 35)
        };
        brandBox.Controls.AddRange(new Control[] { logoSquare, titleLbl, portalLbl });
        side.Controls.Add(brandBox);

        // Nav Items
        var navSectionLabel = new Label
        {
            Text = currentUser is Administrator ? "MODERATION MENU" : "STUDENT NAVIGATION",
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.MoonstoneBlue,
            AutoSize = true,
            Location = new Point(18, 84)
        };
        side.Controls.Add(navSectionLabel);

        string[] navItems = currentUser is Administrator
            ? new[] { "Reported Content Queue", "All Shared Resources" }
            : new[]
            {
                "Browse & Search",
                "Bookmarked Resources",
                "Recently Viewed",
                "My Uploaded Resources",
                "My Profile"
            };

        int y = 110;
        foreach (var item in navItems)
        {
            bool isSelected = activeNav == item;
            var btn = new Button
            {
                Text = "  " + item,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(12, y),
                Size = new Size(206, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                BackColor = isSelected ? StudyHubTheme.DeepSpaceSparkle : Color.Transparent,
                ForeColor = isSelected ? Color.White : StudyHubTheme.LightGray,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            string captured = item;
            btn.Click += (_, _) =>
            {
                activeNav = captured;
                ShowWorkspace();
            };
            side.Controls.Add(btn);
            y += 44;
        }

        // Bottom User Footer
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 104,
            BackColor = StudyHubTheme.PhthaloGreen,
            Padding = new Padding(12)
        };

        string fullName = currentUser?.GetFullName() ?? "User";
        string initials = string.Concat(fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2).Select(w => char.ToUpper(w[0])));

        var avatar = new Label
        {
            Text = initials,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = StudyHubTheme.DeepSpaceSparkle,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(34, 34),
            Location = new Point(12, 10)
        };

        var userNameLbl = new Label
        {
            Text = fullName,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(54, 10)
        };

        var userSubLbl = new Label
        {
            Text = currentUser is Student s
                ? $"{s.Course} • {s.YearLevel}"
                : "Restricted Admin Access",
            Font = new Font("Segoe UI", 7.8f),
            ForeColor = StudyHubTheme.LightGray,
            AutoSize = true,
            Location = new Point(54, 28)
        };

        var logoutBtn = new Button
        {
            Text = "↪  Log Out",
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(12, 54),
            Size = new Size(206, 36),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.Transparent,
            ForeColor = StudyHubTheme.LightGray,
            Cursor = Cursors.Hand
        };
        logoutBtn.FlatAppearance.BorderColor = StudyHubTheme.DeepSpaceSparkle;
        logoutBtn.FlatAppearance.BorderSize = 1;
        logoutBtn.Click += (_, _) => ShowAuthScreen(registerMode: false);

        footer.Controls.AddRange(new Control[] { avatar, userNameLbl, userSubLbl, logoutBtn });
        side.Controls.Add(footer);

        return side;
    }

    private Panel BuildTopBar()
    {
        var bar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.White
        };

        var bottomLine = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = StudyHubTheme.BorderSubtle
        };
        bar.Controls.Add(bottomLine);

        if (currentUser is Student)
        {
            var searchBox = new TextBox
            {
                Location = new Point(24, 12),
                Size = new Size(320, 32),
                BackColor = StudyHubTheme.SoftFieldBg,
                ForeColor = StudyHubTheme.TextDark,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Search resources by title or subject...",
                Text = searchFilter
            };
            searchBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    searchFilter = searchBox.Text.Trim();
                    activeNav = "Browse & Search";
                    ShowWorkspace();
                }
            };
            bar.Controls.Add(searchBox);

            var uploadBtn = new Button
            {
                Text = "+ Upload Resource",
                Location = new Point(1140, 10),
                Size = new Size(148, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            StudyHubTheme.StyleDarkButton(uploadBtn);
            uploadBtn.Click += (_, _) => ShowUploadDialog();
            bar.Controls.Add(uploadBtn);
        }

        var rolePill = new Label
        {
            Text = currentUser is Administrator ? "Administrator" : "Student",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextDark,
            BackColor = StudyHubTheme.SoftFieldBg,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(92, 28),
            Location = new Point(currentUser is Administrator ? 1190 : 1036, 14),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        bar.Controls.Add(rolePill);

        return bar;
    }

    // =========================================================
    // 3. STUDENT PORTAL BODY
    // =========================================================

    private void RenderStudentBody(Panel container, Student student)
    {
        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = StudyHubTheme.CanvasBg
        };
        container.Controls.Add(stack);

        // Hero Banner
        var hero = new Panel
        {
            Size = new Size(1020, 128),
            BackColor = StudyHubTheme.PhthaloGreen,
            Margin = new Padding(0, 0, 0, 18),
            Padding = new Padding(24)
        };

        var tag = new Label
        {
            Text = "• Centralized Learning Resource Library",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.MoonstoneBlue,
            BackColor = StudyHubTheme.DeepSpaceSparkle,
            AutoSize = true,
            Padding = new Padding(8, 3, 8, 3),
            Location = new Point(22, 16)
        };

        var welcome = new Label
        {
            Text = $"Welcome back, {student.GetFullName()}!",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(22, 46)
        };

        var desc = new Label
        {
            Text = "Browse and search shared PDF materials and external learning links, bookmark resources, and join resource discussions.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = StudyHubTheme.LightGray,
            AutoSize = true,
            Location = new Point(22, 84)
        };

        var shareBtn = new Button
        {
            Text = "+ Share Study Resource",
            Size = new Size(176, 36),
            Location = new Point(820, 48)
        };
        StudyHubTheme.StyleAccentButton(shareBtn);
        shareBtn.Click += (_, _) => ShowUploadDialog();

        hero.Controls.AddRange(new Control[] { tag, welcome, desc, shareBtn });
        stack.Controls.Add(hero);

        // Stat Cards Row
        var allResources = resourceService.BrowseResources();
        var bookmarked = resourceService.GetBookmarkedResources(student.GetUserId());
        var mine = allResources.Where(r => r.IsOwnedBy(student.GetUserId())).ToList();

        var statsRow = new FlowLayoutPanel
        {
            Size = new Size(1020, 104),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 18)
        };
        statsRow.Controls.Add(MakeStatCard("AVAILABLE RESOURCES", allResources.Count.ToString(), "PDF files & external links", 328));
        statsRow.Controls.Add(MakeStatCard("BOOKMARKED RESOURCES", bookmarked.Count.ToString(), "Saved for later review", 328));
        statsRow.Controls.Add(MakeStatCard("MY UPLOADED RESOURCES", mine.Count.ToString(), "Shared by you", 328));
        stack.Controls.Add(statsRow);

        if (activeNav == "My Profile")
        {
            stack.Controls.Add(BuildProfileCard(student));
        }
        else
        {
            List<Resource> shown = activeNav switch
            {
                "Bookmarked Resources" => bookmarked,
                "Recently Viewed" => resourceService.GetRecentlyViewed(student.GetUserId()),
                "My Uploaded Resources" => mine,
                _ => string.IsNullOrWhiteSpace(searchFilter)
                    ? allResources
                    : resourceService.SearchResources(searchFilter)
            };

            string sectionTitle = activeNav == "Browse & Search" && !string.IsNullOrWhiteSpace(searchFilter)
                ? $"Search Results for \"{searchFilter}\""
                : activeNav;

            stack.Controls.Add(BuildResourceGridSection(sectionTitle, shown, student, bookmarked));
        }

        stack.Controls.Add(BuildPaletteFooter());
    }

    private Panel BuildResourceGridSection(string title, List<Resource> resources,
        Student student, List<Resource> bookmarked)
    {
        var wrapper = new Panel
        {
            Size = new Size(1020, Math.Max(260, ((resources.Count + 2) / 3) * 235 + 80)),
            BackColor = StudyHubTheme.CardWhite,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 0, 18),
            Padding = new Padding(18)
        };

        var secTitle = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextDark,
            AutoSize = true,
            Location = new Point(18, 14)
        };

        var countBadge = new Label
        {
            Text = $"{resources.Count} resources",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextDark,
            BackColor = StudyHubTheme.SoftFieldBg,
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(912, 14)
        };

        var sub = new Label
        {
            Text = "Browse and search shared PDF materials and external learning links by title or subject",
            Font = new Font("Segoe UI", 9),
            ForeColor = StudyHubTheme.TextMuted,
            AutoSize = true,
            Location = new Point(18, 38)
        };

        wrapper.Controls.AddRange(new Control[] { secTitle, countBadge, sub });

        if (resources.Count == 0)
        {
            var emptyLbl = new Label
            {
                Text = "No study resources match this view yet.",
                Font = new Font("Segoe UI", 10),
                ForeColor = StudyHubTheme.TextMuted,
                AutoSize = true,
                Location = new Point(18, 86)
            };
            wrapper.Controls.Add(emptyLbl);
            return wrapper;
        }

        var grid = new FlowLayoutPanel
        {
            Location = new Point(16, 68),
            Size = new Size(984, wrapper.Height - 80),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };
        wrapper.Controls.Add(grid);

        var bookmarkedIds = bookmarked.Select(b => b.ResourceId).ToHashSet();

        foreach (var res in resources)
        {
            int commentCount = commentService.GetCommentsByResource(res.ResourceId).Count;
            bool isBookmarked = bookmarkedIds.Contains(res.ResourceId);
            bool isOwner = res.IsOwnedBy(student.GetUserId());

            var card = new Panel
            {
                Size = new Size(314, 212),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 12, 12),
                Padding = new Padding(14)
            };

            var typeBadge = new Label
            {
                Text = res.ResourceType == "PDF" ? "PDF File" : "External Link",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = res.ResourceType == "PDF" ? StudyHubTheme.PhthaloGreen : StudyHubTheme.DeepSpaceSparkle,
                BackColor = StudyHubTheme.SoftFieldBg,
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Location = new Point(14, 14)
            };

            var subjBadge = new Label
            {
                Text = res.Subject,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextMuted,
                AutoSize = true,
                Location = new Point(92, 16)
            };

            var bmBtn = new Button
            {
                Text = isBookmarked ? "🔖" : "📑",
                Size = new Size(30, 28),
                Location = new Point(268, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = isBookmarked ? StudyHubTheme.MoonstoneBlue : Color.White,
                Cursor = Cursors.Hand
            };
            bmBtn.FlatAppearance.BorderSize = 0;
            bmBtn.Click += (_, _) =>
            {
                resourceService.ToggleBookmark(student.GetUserId(), res.ResourceId);
                ShowWorkspace();
            };

            var rTitle = new Label
            {
                Text = res.Title,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                Size = new Size(284, 42),
                Location = new Point(14, 44)
            };

            var rDesc = new Label
            {
                Text = res.Description,
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = StudyHubTheme.TextMuted,
                Size = new Size(284, 42),
                Location = new Point(14, 88)
            };

            var metaLbl = new Label
            {
                Text = $"By {res.AuthorName} • {res.DateUploaded:MMM dd, yyyy}",
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = StudyHubTheme.TextMuted,
                AutoSize = true,
                Location = new Point(14, 142)
            };

            var cmtPill = new Label
            {
                Text = $"{commentCount} {(commentCount == 1 ? "comment" : "comments")}",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                BackColor = StudyHubTheme.SoftFieldBg,
                AutoSize = true,
                Padding = new Padding(6, 3, 6, 3),
                Location = new Point(14, 168)
            };

            var viewBtn = new Button
            {
                Text = "View",
                Size = new Size(64, 30),
                Location = new Point(234, 166)
            };
            StudyHubTheme.StyleDarkButton(viewBtn);
            viewBtn.Click += (_, _) => OpenResourceDetailsModal(res, student);

            card.Controls.AddRange(new Control[] { typeBadge, subjBadge, bmBtn, rTitle, rDesc, metaLbl, cmtPill, viewBtn });

            if (isOwner)
            {
                var editBtn = new Button
                {
                    Text = "Edit",
                    Size = new Size(52, 30),
                    Location = new Point(176, 166)
                };
                StudyHubTheme.StyleOutlineButton(editBtn);
                editBtn.Click += (_, _) => ShowEditResourceDialog(res);
                card.Controls.Add(editBtn);
            }

            grid.Controls.Add(card);
        }

        return wrapper;
    }

    private Panel BuildProfileCard(Student student)
    {
        var card = new Panel
        {
            Size = new Size(520, 320),
            BackColor = StudyHubTheme.CardWhite,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 0, 18),
            Padding = new Padding(20)
        };

        var lbl = new Label
        {
            Text = "Update Student Profile",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextDark,
            AutoSize = true,
            Location = new Point(20, 16)
        };
        card.Controls.Add(lbl);

        var nameBox = MakeLabeledInput(card, "FULL NAME", "", 54, 460, false, 20);
        nameBox.Text = student.GetFullName();

        var courseBox = MakeLabeledInput(card, "COURSE", "", 120, 460, false, 20);
        courseBox.Text = student.Course;

        var yearBox = MakeLabeledInput(card, "YEAR LEVEL", "", 186, 460, false, 20);
        yearBox.Text = student.YearLevel;

        var saveBtn = new Button
        {
            Text = "Save Profile",
            Size = new Size(150, 38),
            Location = new Point(20, 256)
        };
        StudyHubTheme.StyleAccentButton(saveBtn);
        saveBtn.Click += (_, _) =>
        {
            student.UpdateProfile(nameBox.Text.Trim(), courseBox.Text.Trim(), yearBox.Text.Trim());
            authService.UpdateStudentProfile(student);
            MessageBox.Show("Profile updated.");
            ShowWorkspace();
        };

        card.Controls.Add(saveBtn);
        return card;
    }

    // =========================================================
    // 4. ADMIN MODERATION BODY
    // =========================================================

    private void RenderAdminBody(Panel container)
    {
        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = StudyHubTheme.CanvasBg
        };
        container.Controls.Add(stack);

        var hero = new Panel
        {
            Size = new Size(1020, 122),
            BackColor = StudyHubTheme.PhthaloGreen,
            Margin = new Padding(0, 0, 0, 18),
            Padding = new Padding(24)
        };

        var tag = new Label
        {
            Text = "• Restricted Administrative Access",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = StudyHubTheme.MoonstoneBlue,
            BackColor = StudyHubTheme.DeepSpaceSparkle,
            AutoSize = true,
            Padding = new Padding(8, 3, 8, 3),
            Location = new Point(22, 16)
        };
        var welcome = new Label
        {
            Text = $"Welcome, {currentUser?.GetFullName() ?? "System Administrator"}",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(22, 46)
        };
        var desc = new Label
        {
            Text = "Review reported study resources and comments, and remove inappropriate content to maintain community standards.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = StudyHubTheme.LightGray,
            AutoSize = true,
            Location = new Point(22, 82)
        };
        hero.Controls.AddRange(new Control[] { tag, welcome, desc });
        stack.Controls.Add(hero);

        var resources = resourceService.BrowseResources();
        var pending = moderationService.GetPendingReports();

        var statsRow = new FlowLayoutPanel
        {
            Size = new Size(1020, 104),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 18)
        };
        statsRow.Controls.Add(MakeStatCard("TOTAL PLATFORM RESOURCES", resources.Count.ToString(), "Active shared items", 328));
        statsRow.Controls.Add(MakeStatCard("PENDING REPORTS", pending.Count.ToString(), "Awaiting admin review", 328));
        statsRow.Controls.Add(MakeStatCard("ACTIVE MODERATION QUEUE", pending.Count.ToString(), "Resources & comments", 328));
        stack.Controls.Add(statsRow);

        if (activeNav == "All Shared Resources")
        {
            var listPanel = new Panel
            {
                Size = new Size(1020, Math.Max(200, resources.Count * 68 + 80)),
                BackColor = StudyHubTheme.CardWhite,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 18),
                Padding = new Padding(18)
            };
            listPanel.Controls.Add(new Label
            {
                Text = "All Shared Resources",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                AutoSize = true,
                Location = new Point(18, 16)
            });

            int ry = 54;
            foreach (var r in resources)
            {
                var row = new Panel
                {
                    Location = new Point(18, ry),
                    Size = new Size(980, 56),
                    BackColor = StudyHubTheme.SoftFieldBg,
                    BorderStyle = BorderStyle.FixedSingle
                };
                row.Controls.Add(new Label
                {
                    Text = $"#{r.ResourceId}  [{r.ResourceType}] {r.Title} — {r.Subject} (by {r.AuthorName})",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = StudyHubTheme.TextDark,
                    AutoSize = true,
                    Location = new Point(14, 18)
                });
                var delBtn = new Button
                {
                    Text = "Delete",
                    Size = new Size(80, 32),
                    Location = new Point(884, 11),
                    BackColor = StudyHubTheme.DangerBrick,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                delBtn.FlatAppearance.BorderSize = 0;
                int targetId = r.ResourceId;
                delBtn.Click += (_, _) =>
                {
                    resourceService.DeleteResource(targetId);
                    ShowWorkspace();
                };
                row.Controls.Add(delBtn);
                listPanel.Controls.Add(row);
                ry += 64;
            }
            stack.Controls.Add(listPanel);
        }
        else
        {
            var queueBox = new Panel
            {
                Size = new Size(1020, Math.Max(220, pending.Count * 116 + 96)),
                BackColor = StudyHubTheme.CardWhite,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 18),
                Padding = new Padding(18)
            };

            queueBox.Controls.Add(new Label
            {
                Text = "Reported Content Moderation Queue",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                AutoSize = true,
                Location = new Point(18, 14)
            });
            queueBox.Controls.Add(new Label
            {
                Text = "Review submitted reports and remove inappropriate resources or comments",
                Font = new Font("Segoe UI", 9),
                ForeColor = StudyHubTheme.TextMuted,
                AutoSize = true,
                Location = new Point(18, 38)
            });
            queueBox.Controls.Add(new Label
            {
                Text = $"{pending.Count} Pending {(pending.Count == 1 ? "Report" : "Reports")}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                BackColor = StudyHubTheme.SoftFieldBg,
                AutoSize = true,
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(900, 16)
            });

            if (pending.Count == 0)
            {
                queueBox.Controls.Add(new Label
                {
                    Text = "No pending reports in the queue.",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = StudyHubTheme.TextMuted,
                    AutoSize = true,
                    Location = new Point(18, 76)
                });
            }
            else
            {
                int qy = 70;
                foreach (var rep in pending)
                {
                    var itemCard = new Panel
                    {
                        Location = new Point(18, qy),
                        Size = new Size(980, 100),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    var typeTag = new Label
                    {
                        Text = $"Reported {rep.TargetType}",
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        ForeColor = StudyHubTheme.DeepSpaceSparkle,
                        BackColor = StudyHubTheme.SoftFieldBg,
                        AutoSize = true,
                        Padding = new Padding(6, 2, 6, 2),
                        Location = new Point(14, 14)
                    };

                    var statusTag = new Label
                    {
                        Text = rep.Status.ToUpper(),
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                        ForeColor = StudyHubTheme.MustardGreen,
                        BackColor = StudyHubTheme.SoftFieldBg,
                        AutoSize = true,
                        Padding = new Padding(6, 2, 6, 2),
                        Location = new Point(122, 14)
                    };

                    var meta = new Label
                    {
                        Text = $"Report #{rep.ReportId} • Target #{rep.TargetId} • {rep.DateReported:MMM dd, yyyy}",
                        Font = new Font("Segoe UI", 8.5f),
                        ForeColor = StudyHubTheme.TextMuted,
                        AutoSize = true,
                        Location = new Point(195, 16)
                    };

                    var reasonBox = new Panel
                    {
                        Location = new Point(14, 42),
                        Size = new Size(680, 44),
                        BackColor = StudyHubTheme.SoftFieldBg
                    };
                    reasonBox.Controls.Add(new Label
                    {
                        Text = $"Reason: {rep.Reason}",
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = StudyHubTheme.TextDark,
                        AutoSize = true,
                        Location = new Point(10, 12)
                    });

                    var dismissBtn = new Button
                    {
                        Text = "Dismiss Report",
                        Size = new Size(124, 34),
                        Location = new Point(712, 32)
                    };
                    StudyHubTheme.StyleOutlineButton(dismissBtn);
                    int rId = rep.ReportId;
                    dismissBtn.Click += (_, _) =>
                    {
                        moderationService.DismissReport(rId);
                        ShowWorkspace();
                    };

                    var removeBtn = new Button
                    {
                        Text = rep.TargetType == "Comment" ? "Remove Comment" : "Remove Resource",
                        Size = new Size(138, 34),
                        Location = new Point(842, 32),
                        BackColor = StudyHubTheme.PhthaloGreen,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    removeBtn.FlatAppearance.BorderSize = 0;
                    string tType = rep.TargetType;
                    removeBtn.Click += (_, _) =>
                    {
                        if (tType == "Comment")
                            moderationService.RemoveReportedComment(rId);
                        else
                            moderationService.RemoveReportedResource(rId);
                        ShowWorkspace();
                    };

                    itemCard.Controls.AddRange(new Control[] { typeTag, statusTag, meta, reasonBox, dismissBtn, removeBtn });
                    queueBox.Controls.Add(itemCard);
                    qy += 110;
                }
            }

            stack.Controls.Add(queueBox);
        }

        stack.Controls.Add(BuildPaletteFooter());
    }

    // =========================================================
    // 5. SHARED COMPONENTS & MODALS
    // =========================================================

    private Panel MakeStatCard(string title, string value, string caption, int width)
    {
        var card = new Panel
        {
            Size = new Size(width, 94),
            BackColor = StudyHubTheme.CardWhite,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(16)
        };

        card.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextMuted,
            AutoSize = true,
            Location = new Point(16, 14)
        });
        card.Controls.Add(new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextDark,
            AutoSize = true,
            Location = new Point(16, 34)
        });
        card.Controls.Add(new Label
        {
            Text = caption,
            Font = new Font("Segoe UI", 8.2f),
            ForeColor = StudyHubTheme.TextMuted,
            AutoSize = true,
            Location = new Point(16, 66)
        });

        var iconBox = new Label
        {
            Text = "📄",
            Font = new Font("Segoe UI", 12),
            BackColor = StudyHubTheme.SoftFieldBg,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(34, 34),
            Location = new Point(width - 54, 28)
        };
        card.Controls.Add(iconBox);

        return card;
    }

    private Panel BuildPaletteFooter()
    {
        var bar = new Panel
        {
            Size = new Size(1020, 56),
            BackColor = StudyHubTheme.CardWhite,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(16)
        };

        bar.Controls.Add(new Label
        {
            Text = "STUDYHUB • LEARNING COMMUNITY PLATFORM",
            Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
            ForeColor = StudyHubTheme.TextDark,
            AutoSize = true,
            Location = new Point(16, 10)
        });
        bar.Controls.Add(new Label
        {
            Text = "Supporting Sustainable Development Goal 4 (Quality Education) through shared academic resources.",
            Font = new Font("Segoe UI", 8.2f),
            ForeColor = StudyHubTheme.TextMuted,
            AutoSize = true,
            Location = new Point(16, 28)
        });

        string[] hexes = { "#19350C", "#687D31", "#D5D3CC", "#406768", "#6FA9BB" };
        Color[] swatches =
        {
            StudyHubTheme.PhthaloGreen,
            StudyHubTheme.MustardGreen,
            StudyHubTheme.LightGray,
            StudyHubTheme.DeepSpaceSparkle,
            StudyHubTheme.MoonstoneBlue
        };

        int sx = 635;
        for (int i = 0; i < hexes.Length; i++)
        {
            var chip = new Panel
            {
                Location = new Point(sx, 14),
                Size = new Size(72, 26),
                BackColor = StudyHubTheme.SoftFieldBg,
                BorderStyle = BorderStyle.FixedSingle
            };
            chip.Controls.Add(new Panel
            {
                Location = new Point(6, 7),
                Size = new Size(10, 10),
                BackColor = swatches[i]
            });
            chip.Controls.Add(new Label
            {
                Text = hexes[i],
                Font = new Font("Segoe UI", 7.2f, FontStyle.Bold),
                ForeColor = StudyHubTheme.TextDark,
                AutoSize = true,
                Location = new Point(20, 6)
            });
            bar.Controls.Add(chip);
            sx += 76;
        }

        return bar;
    }

    private void OpenResourceDetailsModal(Resource resource, Student student)
    {
        resourceService.RecordView(student.GetUserId(), resource.ResourceId);

        using var modal = new Form
        {
            Text = resource.Title,
            Size = new Size(640, 560),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = StudyHubTheme.CanvasBg
        };

        var infoBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Location = new Point(18, 18),
            Size = new Size(588, 150),
            BackColor = Color.White,
            Text = resource.GetDetails()
        };

        var cmtBox = new ListBox
        {
            Location = new Point(18, 180),
            Size = new Size(588, 150),
            BackColor = Color.White
        };
        foreach (var c in commentService.GetCommentsByResource(resource.ResourceId))
            cmtBox.Items.Add(c.GetDetails());

        var newCmtInput = new TextBox
        {
            Location = new Point(18, 344),
            Size = new Size(460, 34),
            BackColor = Color.White,
            PlaceholderText = "Write a helpful comment..."
        };

        var postBtn = new Button
        {
            Text = "Post Comment",
            Location = new Point(486, 343),
            Size = new Size(120, 36)
        };
        StudyHubTheme.StyleAccentButton(postBtn);
        postBtn.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(newCmtInput.Text)) return;
            commentService.AddComment(resource.ResourceId, student.GetUserId(), newCmtInput.Text.Trim());
            modal.Close();
            OpenResourceDetailsModal(resource, student);
        };

        var reportBtn = new Button
        {
            Text = "Report Resource",
            Location = new Point(18, 396),
            Size = new Size(140, 36),
            BackColor = StudyHubTheme.DangerBrick,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        reportBtn.FlatAppearance.BorderSize = 0;
        reportBtn.Click += (_, _) =>
        {
            string reason = PromptText("Report Resource", "Reason for reporting:");
            if (!string.IsNullOrWhiteSpace(reason))
            {
                moderationService.SubmitReport(student.GetUserId(), resource.ResourceId, "Resource", reason.Trim());
                MessageBox.Show("Report submitted.");
            }
        };

        var closeBtn = new Button
        {
            Text = "Close",
            Location = new Point(486, 396),
            Size = new Size(120, 36)
        };
        StudyHubTheme.StyleOutlineButton(closeBtn);
        closeBtn.Click += (_, _) => modal.Close();

        modal.Controls.AddRange(new Control[] { infoBox, cmtBox, newCmtInput, postBtn, reportBtn, closeBtn });
        modal.ShowDialog(this);
        ShowWorkspace();
    }

    private void ShowUploadDialog()
    {
        if (currentUser is not Student student) return;

        using var dlg = new Form
        {
            Text = "Share Study Resource",
            Size = new Size(480, 480),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };

        var tBox = MakeLabeledInput(dlg, "TITLE", "e.g., OOP Principles Notes", 16, 424, false, 20);
        var dBox = MakeLabeledInput(dlg, "DESCRIPTION", "Short summary of the material", 82, 424, false, 20);
        var sBox = MakeLabeledInput(dlg, "SUBJECT", "e.g., Computer Science", 148, 424, false, 20);
        var aBox = MakeLabeledInput(dlg, "AUTHOR NAME", student.GetFullName(), 214, 424, false, 20);

        var typeCombo = new ComboBox
        {
            Location = new Point(20, 286),
            Size = new Size(120, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        typeCombo.Items.AddRange(new object[] { "PDF", "Link" });
        typeCombo.SelectedIndex = 0;

        var pBox = MakeLabeledInput(dlg, "FILE PATH OR URL", "docs/notes.pdf or https://...", 316, 310, false, 20);

        var pickBtn = new Button
        {
            Text = "Pick PDF",
            Location = new Point(338, 336),
            Size = new Size(106, 34)
        };
        StudyHubTheme.StyleOutlineButton(pickBtn);
        pickBtn.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                typeCombo.SelectedItem = "PDF";
                pBox.Text = ofd.FileName;
            }
        };

        var saveBtn = new Button
        {
            Text = "Publish Resource",
            Location = new Point(20, 392),
            Size = new Size(424, 38)
        };
        StudyHubTheme.StyleAccentButton(saveBtn);
        saveBtn.Click += (_, _) =>
        {
            if (new[] { tBox.Text, dBox.Text, sBox.Text, aBox.Text, pBox.Text }.Any(string.IsNullOrWhiteSpace))
            {
                MessageBox.Show("Please fill out all fields.");
                return;
            }
            resourceService.UploadResource(
                student.GetUserId(), tBox.Text.Trim(), dBox.Text.Trim(),
                sBox.Text.Trim(), aBox.Text.Trim(),
                typeCombo.SelectedItem?.ToString() ?? "Link", pBox.Text.Trim());
            dlg.Close();
            ShowWorkspace();
        };

        dlg.Controls.AddRange(new Control[] { typeCombo, pickBtn, saveBtn });
        dlg.ShowDialog(this);
    }

    private void ShowEditResourceDialog(Resource resource)
    {
        string title = PromptText("Edit Title", "New Title:", resource.Title);
        if (string.IsNullOrWhiteSpace(title)) return;
        string desc = PromptText("Edit Description", "New Description:", resource.Description);
        if (string.IsNullOrWhiteSpace(desc)) return;
        string subj = PromptText("Edit Subject", "New Subject:", resource.Subject);
        if (string.IsNullOrWhiteSpace(subj)) return;
        string author = PromptText("Edit Author", "New Author:", resource.AuthorName);
        if (string.IsNullOrWhiteSpace(author)) return;
        string path = PromptText("Edit Path/URL", "New Path or URL:", resource.FilePathOrUrl);
        if (string.IsNullOrWhiteSpace(path)) return;

        resource.Update(title.Trim(), desc.Trim(), subj.Trim(), author.Trim(), path.Trim());
        resourceService.UpdateResource(resource);
        ShowWorkspace();
    }

    private string PromptText(string title, string prompt, string defaultValue = "")
    {
        using var f = new Form
        {
            Text = title,
            Size = new Size(400, 175),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };
        var lbl = new Label { Text = prompt, Location = new Point(16, 14), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
        var tb = new TextBox { Text = defaultValue, Location = new Point(16, 42), Size = new Size(350, 30) };
        var ok = new Button { Text = "OK", Location = new Point(286, 86), Size = new Size(80, 34), DialogResult = DialogResult.OK };
        StudyHubTheme.StyleDarkButton(ok);
        f.Controls.AddRange(new Control[] { lbl, tb, ok });
        f.AcceptButton = ok;
        return f.ShowDialog(this) == DialogResult.OK ? tb.Text : "";
    }
}