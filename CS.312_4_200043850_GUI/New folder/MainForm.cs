using StudentManagementSystem.Services;
using StudentManagementSystem.UI;
using StudentManagementSystem.Views;

namespace StudentManagementSystem;

public class MainForm : Form
{
    private readonly string _username;
    private readonly StudentRepository _repository = new();
    private readonly Panel content = new() { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(28, 24, 28, 24) };
    private readonly Label lblPageTitle = new() { AutoSize = true, Font = Theme.Semibold(17f), ForeColor = Theme.Text, BackColor = Color.Transparent };
    private readonly Label lblPageSub = new() { AutoSize = true, Font = Theme.Regular(9.5f), ForeColor = Theme.Muted, BackColor = Color.Transparent };
    private readonly List<NavButton> _navButtons = new();

    public MainForm(string username)
    {
        _username = string.IsNullOrWhiteSpace(username) ? "admin" : username;
        Text = "Student Management System";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        Font = Theme.Regular();
        DoubleBuffered = true;
        BuildShell();
        _navButtons[0].PerformClick();
    }

    private void BuildShell()
    {
        // ── Sidebar ────────────────────────────────────────────────────────
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 256, BackColor = Theme.Ink };

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            BackColor = Theme.Ink, Padding = new Padding(14, 4, 14, 0), AutoScroll = false
        };

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 142, BackColor = Theme.Ink, Padding = new Padding(14, 6, 14, 14) };
        var userCard = new Panel { Bounds = new Rectangle(14, 6, 228, 62), BackColor = Theme.Ink };
        Theme.DoubleBuffer(userCard);
        userCard.Paint += (_, e) =>
        {
            var g = e.Graphics; Theme.HighQuality(g);
            using var path = Theme.RoundedRect(new Rectangle(0, 0, userCard.Width - 1, userCard.Height - 1), 12);
            using var fill = new SolidBrush(Theme.InkSoft); g.FillPath(fill, path);
            using var av = new SolidBrush(Theme.Accent); g.FillEllipse(av, 12, 12, 38, 38);
            const TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(g, _username[..1].ToUpperInvariant(), Theme.Semibold(12f), new Rectangle(12, 12, 38, 38), Color.White, f | TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, _username, Theme.Semibold(10f), new Rectangle(60, 11, 156, 22), Color.White, f | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, "Administrator", Theme.Regular(8.5f), new Rectangle(60, 31, 156, 20), Color.FromArgb(150, 166, 194), f);
        };
        var signOut = new NavButton { Text = "Sign out", Glyph = Theme.Glyph.SignOut, Bounds = new Rectangle(14, 80, 228, 46) };
        signOut.Click += (_, _) =>
        {
            if (MessageBox.Show("Are you sure you want to sign out?", "Sign out", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) Close();
        };
        bottom.Controls.Add(userCard); bottom.Controls.Add(signOut);

        var brand = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Theme.Ink };
        Theme.DoubleBuffer(brand);
        brand.Paint += (_, e) =>
        {
            var g = e.Graphics; Theme.HighQuality(g);
            using (var p = Theme.RoundedRect(new Rectangle(22, 26, 42, 42), 12))
            using (var b = new SolidBrush(Theme.Accent)) g.FillPath(b, p);
            const TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(g, Theme.Glyph.Education, Theme.Icon(16f), new Rectangle(22, 26, 42, 42), Color.White, f | TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, "Student Management", Theme.Semibold(11f), new Rectangle(76, 25, 170, 24), Color.White, f);
            TextRenderer.DrawText(g, "University portal", Theme.Regular(8.5f), new Rectangle(76, 47, 170, 20), Color.FromArgb(140, 158, 190), f);
        };

        var workspace = new Label
        {
            Text = "Workspace", AutoSize = false, Size = new Size(228, 28), ForeColor = Color.FromArgb(120, 139, 172),
            Font = Theme.Semibold(8.5f), BackColor = Theme.Ink, TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(8, 0, 0, 4), Margin = new Padding(0, 0, 0, 4)
        };
        nav.Controls.Add(workspace);

        // Fill first, then docked edges (WinForms dock order)
        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(bottom);
        sidebar.Controls.Add(brand);

        AddNav("Dashboard", Theme.Glyph.Dashboard, "Dashboard", "A live summary of your university today.", () => new DashboardView(_repository));
        AddNav("Students", Theme.Glyph.Students, "Student management", "Add, edit and search student records.", () => new StudentManagementView(_repository));
        AddNav("Departments", Theme.Glyph.Department, "Departments", "Manage university departments and their details.", () => Placeholder(Theme.Glyph.Department, "Departments", "Manage university departments, heads and capacity. This module is coming soon."));
        AddNav("Subjects", Theme.Glyph.Subjects, "Subjects", "Manage subjects and course information.", () => Placeholder(Theme.Glyph.Subjects, "Subjects", "Create subjects, assign them to departments and track credit hours. This module is coming soon."));
        AddNav("Reports", Theme.Glyph.Reports, "Reports & statistics", "Generate summaries and export them for review.", () => new ReportsView(_repository));

        nav.Resize += (_, _) => { foreach (var b in _navButtons) b.Width = Math.Max(120, nav.ClientSize.Width - nav.Padding.Horizontal); };

        // ── Right side: top bar + content ──────────────────────────────────
        var right = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
        var topbar = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Theme.Surface };
        Theme.DoubleBuffer(topbar);
        topbar.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.Border);
            e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
        };
        lblPageTitle.Location = new Point(28, 17);
        lblPageSub.Location = new Point(30, 52);

        var dateChip = new Label
        {
            Text = DateTime.Now.ToString("dddd, d MMMM yyyy"), AutoSize = false, Size = new Size(210, 36),
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = Theme.Muted, BackColor = Theme.Background,
            Font = Theme.Regular(9f), Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topbar.Controls.AddRange([lblPageTitle, lblPageSub, dateChip]);
        topbar.Resize += (_, _) => dateChip.Location = new Point(topbar.Width - dateChip.Width - 28, 26);

        right.Controls.Add(content);
        right.Controls.Add(topbar);
        Controls.Add(right);
        Controls.Add(sidebar);

        void AddNav(string text, string glyph, string title, string subtitle, Func<Control> factory)
        {
            var b = new NavButton { Text = text, Glyph = glyph, Width = 228 };
            b.Tag = (title, subtitle, factory);
            b.Click += (_, _) => Navigate(b);
            nav.Controls.Add(b);
            _navButtons.Add(b);
        }
    }

    private void Navigate(NavButton selected)
    {
        foreach (var b in _navButtons) b.Active = ReferenceEquals(b, selected);
        var (title, subtitle, factory) = ((string, string, Func<Control>))selected.Tag!;
        lblPageTitle.Text = title;
        lblPageSub.Text = subtitle;

        content.SuspendLayout();
        var old = content.Controls.Cast<Control>().ToList();
        content.Controls.Clear();
        foreach (var c in old) c.Dispose();
        var view = factory();
        view.Dock = DockStyle.Fill;
        content.Controls.Add(view);
        content.ResumeLayout(true);
    }

    private static Control Placeholder(string glyph, string title, string description)
    {
        var host = new Panel { BackColor = Theme.Background };
        var card = new RoundedPanel { Size = new Size(520, 300), Radius = 16 };
        var badge = new GlyphBadge { Glyph = glyph, Circle = true, Size = new Size(76, 76), GlyphSize = 26f, Location = new Point((520 - 76) / 2, 36) };
        var h = new Label { Text = title, AutoSize = false, Size = new Size(520, 36), Location = new Point(0, 126), TextAlign = ContentAlignment.MiddleCenter, Font = Theme.Semibold(16f), ForeColor = Theme.Text, BackColor = Color.Transparent };
        var p = new Label { Text = description, AutoSize = false, Size = new Size(400, 52), Location = new Point(60, 168), TextAlign = ContentAlignment.TopCenter, Font = Theme.Regular(10f), ForeColor = Theme.Muted, BackColor = Color.Transparent };
        var pill = new Label { Text = "Coming soon", AutoSize = false, Size = new Size(120, 30), Location = new Point((520 - 120) / 2, 234), TextAlign = ContentAlignment.MiddleCenter, Font = Theme.Semibold(9f), ForeColor = Color.FromArgb(160, 100, 10), BackColor = Theme.AmberSoft };
        card.Controls.AddRange([badge, h, p, pill]);
        host.Controls.Add(card);
        host.Resize += (_, _) => card.Location = new Point(Math.Max(0, (host.ClientSize.Width - card.Width) / 2), Math.Max(0, (host.ClientSize.Height - card.Height) / 2 - 20));
        return host;
    }
}
