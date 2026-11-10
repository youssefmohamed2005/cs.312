using System.Drawing.Drawing2D;
using System.Drawing.Text;
using StudentManagementSystem.UI;

namespace StudentManagementSystem;

public class LoginForm : Form
{
    private readonly TextBox txtUsername = new() { Name = "txtUsername", PlaceholderText = "Enter your username" };
    private readonly TextBox txtPassword = new() { Name = "txtPassword", PlaceholderText = "Enter your password", UseSystemPasswordChar = true };
    private readonly Label lblValidation = new() { AutoSize = false, ForeColor = Theme.Red, Font = Theme.Regular(9f), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ModernButton btnLogin = new() { Text = "Sign in", Kind = ButtonKind.Primary, Glyph = Theme.Glyph.Lock, Height = 46 };
    private readonly ModernButton btnCancel = new() { Text = "Clear", Kind = ButtonKind.Secondary, Height = 46 };
    private readonly CheckBox chkShow = new() { Text = "Show password", AutoSize = true, Font = Theme.Regular(9f), ForeColor = Theme.Muted, BackColor = Color.White, Cursor = Cursors.Hand };
    private FieldBox fbUser = null!;
    private FieldBox fbPass = null!;

    public LoginForm()
    {
        Text = "Student Management System | Sign in";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(860, 640);
        Size = new Size(1000, 680);
        BackColor = Theme.Background;
        Font = Theme.Regular();
        DoubleBuffered = true;
        BuildLayout();
    }

    private void BuildLayout()
    {
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Background, Margin = Padding.Empty, Padding = Padding.Empty };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var left = new BrandPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };

        var right = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Margin = Padding.Empty };
        var card = new RoundedPanel { Size = new Size(400, 484), Radius = 16 };

        var welcome = Theme.MakeLabel("Welcome back", 22, true); welcome.Location = new Point(30, 30);
        var intro = Theme.MakeLabel("Sign in to access the administration dashboard.", 9.5f, false, Theme.Muted); intro.Location = new Point(32, 78);

        var userLabel = Theme.MakeLabel("Username", 9, true, Theme.Text); userLabel.Location = new Point(32, 124);
        fbUser = new FieldBox(txtUsername, Theme.Glyph.Contact) { Bounds = new Rectangle(30, 148, 340, 44) };
        var passLabel = Theme.MakeLabel("Password", 9, true, Theme.Text); passLabel.Location = new Point(32, 208);
        fbPass = new FieldBox(txtPassword, Theme.Glyph.Lock) { Bounds = new Rectangle(30, 232, 340, 44) };

        chkShow.Location = new Point(32, 288);
        chkShow.CheckedChanged += (_, _) => txtPassword.UseSystemPasswordChar = !chkShow.Checked;

        lblValidation.SetBounds(32, 316, 336, 22);
        btnLogin.SetBounds(30, 346, 226, 46);
        btnCancel.SetBounds(264, 346, 106, 46);

        var hint = new RoundedPanel { Bounds = new Rectangle(30, 412, 340, 44), Shadow = false, Radius = 10, Fill = Theme.AccentSoft, Stroke = Theme.AccentSoft };
        var hintText = Theme.MakeLabel("Demo access:   admin  /  admin123", 9, false, Theme.AccentDark);
        hintText.Location = new Point(16, 13);
        hint.Controls.Add(hintText);

        card.Controls.AddRange([welcome, intro, userLabel, fbUser, passLabel, fbPass, chkShow, lblValidation, btnLogin, btnCancel, hint]);
        right.Controls.Add(card);
        void Center() => card.Location = new Point(Math.Max(10, (right.ClientSize.Width - card.Width) / 2), Math.Max(10, (right.ClientSize.Height - card.Height) / 2));
        right.Resize += (_, _) => Center();

        split.Controls.Add(left, 0, 0);
        split.Controls.Add(right, 1, 0);
        Controls.Add(split);
        Shown += (_, _) => { Center(); txtUsername.Focus(); };

        btnLogin.Click += (_, _) => Login();
        btnCancel.Click += (_, _) =>
        {
            txtUsername.Clear(); txtPassword.Clear(); lblValidation.Text = "";
            fbUser.SetError(false); fbPass.SetError(false); txtUsername.Focus();
        };
        AcceptButton = btnLogin;
    }

    private void Login()
    {
        bool userMissing = string.IsNullOrWhiteSpace(txtUsername.Text);
        bool passMissing = string.IsNullOrWhiteSpace(txtPassword.Text);
        fbUser.SetError(userMissing);
        fbPass.SetError(passMissing);
        if (userMissing || passMissing)
        {
            lblValidation.Text = "Please enter your username and password.";
            (userMissing ? txtUsername : txtPassword).Focus();
            return;
        }

        if (txtUsername.Text.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase) && txtPassword.Text == "admin123")
        {
            Hide();
            using var dashboard = new MainForm(txtUsername.Text.Trim());
            dashboard.ShowDialog();
            Close();
        }
        else
        {
            lblValidation.Text = "Incorrect username or password. Please try again.";
            fbUser.SetError(true); fbPass.SetError(true);
            txtPassword.SelectAll(); txtPassword.Focus();
        }
    }

    // ── Left-hand brand panel: gradient, decorative rings, value props ─────
    private sealed class BrandPanel : Panel
    {
        public BrandPanel() { Theme.DoubleBuffer(this); ResizeRedraw = true; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            if (Width < 10 || Height < 10) return;

            using (var bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(9, 18, 38), Color.FromArgb(22, 52, 112), 60f))
                g.FillRectangle(bg, ClientRectangle);

            // concentric rings, bottom-right
            for (int i = 1; i <= 5; i++)
            {
                int r = 90 + i * 70;
                using var pen = new Pen(Color.FromArgb(26 - i * 3, 120, 160, 255), 1.4f);
                g.DrawEllipse(pen, Width - 40 - r, Height + 20 - r, r * 2, r * 2);
            }
            using (var glow = new SolidBrush(Color.FromArgb(18, Theme.Accent)))
                g.FillEllipse(glow, -120, -140, 360, 360);

            using var subBrush = new SolidBrush(Color.FromArgb(190, 204, 228));
            using var bodyBrush = new SolidBrush(Color.FromArgb(170, 188, 216));
            using var goldBrush = new SolidBrush(Theme.Gold);
            using var footBrush = new SolidBrush(Color.FromArgb(120, 140, 172));

            // logo mark
            using (var path = Theme.RoundedRect(new Rectangle(56, 56, 52, 52), 14))
            using (var b = new SolidBrush(Theme.Accent)) g.FillPath(b, path);
            var logoFont = Theme.Icon(20f);
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(Theme.Glyph.Education, logoFont, Brushes.White, new RectangleF(56, 56, 52, 52), sf);
            using (var f = new Font("Segoe UI Semibold", 10f))
                g.DrawString("University Portal", f, subBrush, 120, 72);

            // headline block, vertically centred
            float y = Math.Max(150, Height * 0.30f);
            using (var f = new Font("Segoe UI Light", 30f))
                g.DrawString("Student\nManagement\nSystem", f, Brushes.White, 54, y);
            using (var f = new Font("Segoe UI", 11f))
                g.DrawString("Records, enrollment and reporting\nfor your whole university, in one place.", f,
                    bodyBrush, 58, y + 150);

            string[] points = ["Centralised student records", "Live enrollment insights", "One-click CSV reports"];
            float py = y + 232;
            foreach (var p in points)
            {
                using (var dot = new SolidBrush(Color.FromArgb(40, Theme.Gold))) g.FillEllipse(dot, 58, py - 2, 24, 24);
                var tickFont = Theme.Icon(9f);
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("\uE73E", tickFont, goldBrush, new RectangleF(58, py - 2, 24, 24), sf);
                using (var f = new Font("Segoe UI", 10f))
                    g.DrawString(p, f, Brushes.White, 92, py + 1);
                py += 36;
            }

            using (var f = new Font("Segoe UI", 8.5f))
                g.DrawString("© 2026 University Administration", f, footBrush, 58, Height - 52);
        }
    }
}
