using System.Drawing;
using System.Windows.Forms;

namespace StudyHub.Desktop;

public static class StudyHubTheme
{
    // Exact palette from colors.jpg
    public static readonly Color PhthaloGreen = Color.FromArgb(25, 53, 12);     // #19350C
    public static readonly Color MustardGreen = Color.FromArgb(104, 125, 49);   // #687D31
    public static readonly Color LightGray = Color.FromArgb(213, 211, 204);    // #D5D3CC
    public static readonly Color DeepSpaceSparkle = Color.FromArgb(64, 103, 104);// #406768
    public static readonly Color MoonstoneBlue = Color.FromArgb(111, 169, 187); // #6FA9BB

    // Clean surfaces & text
    public static readonly Color CanvasBg = Color.FromArgb(242, 241, 237);
    public static readonly Color CardWhite = Color.White;
    public static readonly Color SoftFieldBg = Color.FromArgb(245, 246, 244);
    public static readonly Color TextDark = Color.FromArgb(22, 28, 22);
    public static readonly Color TextMuted = Color.FromArgb(92, 102, 94);
    public static readonly Color BorderSubtle = Color.FromArgb(216, 214, 207);
    public static readonly Color DangerBrick = Color.FromArgb(153, 48, 42);

    public static void StyleDarkButton(Button btn, Color? bg = null)
    {
        btn.BackColor = bg ?? PhthaloGreen;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleAccentButton(Button btn)
    {
        btn.BackColor = MustardGreen;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleOutlineButton(Button btn)
    {
        btn.BackColor = CardWhite;
        btn.ForeColor = TextDark;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = BorderSubtle;
        btn.FlatAppearance.BorderSize = 1;
        btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleCard(Panel panel)
    {
        panel.BackColor = CardWhite;
        panel.BorderStyle = BorderStyle.FixedSingle;
    }
}