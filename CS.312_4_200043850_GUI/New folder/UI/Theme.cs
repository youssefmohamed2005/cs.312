using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace StudentManagementSystem.UI;

/// <summary>Central design tokens + small GDI+ helpers shared by every screen.</summary>
public static class Theme
{
    // ── Palette ────────────────────────────────────────────────────────────
    public static readonly Color Ink          = Color.FromArgb(11, 22, 43);     // sidebar / login panel
    public static readonly Color InkSoft      = Color.FromArgb(21, 36, 66);
    public static readonly Color InkHover     = Color.FromArgb(30, 49, 87);
    public static readonly Color Accent       = Color.FromArgb(47, 107, 255);
    public static readonly Color AccentDark   = Color.FromArgb(33, 84, 214);
    public static readonly Color AccentSoft   = Color.FromArgb(233, 240, 255);
    public static readonly Color Gold         = Color.FromArgb(245, 176, 65);
    public static readonly Color Background   = Color.FromArgb(243, 245, 250);
    public static readonly Color Surface      = Color.White;
    public static readonly Color SurfaceAlt   = Color.FromArgb(248, 250, 253);
    public static readonly Color Text         = Color.FromArgb(24, 36, 58);
    public static readonly Color Muted        = Color.FromArgb(111, 124, 146);
    public static readonly Color Border       = Color.FromArgb(226, 231, 241);
    public static readonly Color BorderStrong = Color.FromArgb(200, 208, 224);
    public static readonly Color Green        = Color.FromArgb(22, 163, 112);
    public static readonly Color GreenSoft    = Color.FromArgb(226, 247, 238);
    public static readonly Color Red          = Color.FromArgb(220, 69, 82);
    public static readonly Color RedSoft      = Color.FromArgb(253, 233, 235);
    public static readonly Color Violet       = Color.FromArgb(124, 92, 230);
    public static readonly Color VioletSoft   = Color.FromArgb(240, 235, 255);
    public static readonly Color AmberSoft    = Color.FromArgb(255, 244, 222);

    public const int Radius = 12;

    // ── Fonts (cached so we never leak a Font per control) ─────────────────
    private static readonly Dictionary<(string, float, FontStyle), Font> _fonts = new();

    private static Font Get(string family, float size, FontStyle style)
    {
        var key = (family, size, style);
        if (!_fonts.TryGetValue(key, out var f))
            _fonts[key] = f = new Font(family, size, style, GraphicsUnit.Point);
        return f;
    }

    public static Font Regular(float size = 9.5f)  => Get("Segoe UI", size, FontStyle.Regular);
    public static Font Semibold(float size = 9.5f) => Get("Segoe UI Semibold", size, FontStyle.Regular);
    public static Font Bold(float size = 9.5f)     => Get("Segoe UI", size, FontStyle.Bold);
    public static Font Display(float size = 24f)   => Get("Segoe UI Light", size, FontStyle.Regular);
    public static Font Icon(float size = 12f)      => Get("Segoe MDL2 Assets", size, FontStyle.Regular);

    // ── Segoe MDL2 glyphs (built into Windows 10/11) ───────────────────────
    public static class Glyph
    {
        public const string Dashboard   = "\uE80F";
        public const string Students    = "\uE716";
        public const string Department  = "\uE731";
        public const string Subjects    = "\uE8F1";
        public const string Reports     = "\uE9D9";
        public const string SignOut     = "\uE7E8";
        public const string Add         = "\uE710";
        public const string Edit        = "\uE70F";
        public const string Delete      = "\uE74D";
        public const string Clear       = "\uE894";
        public const string Search      = "\uE721";
        public const string Download    = "\uE896";
        public const string Play        = "\uE768";
        public const string Refresh     = "\uE72C";
        public const string Mail        = "\uE715";
        public const string Calendar    = "\uE787";
        public const string Contact     = "\uE77B";
        public const string Lock        = "\uE72E";
        public const string Education   = "\uE7BE";
    }

    // ── Geometry helpers ───────────────────────────────────────────────────
    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = Math.Max(1, radius * 2);
        d = Math.Min(d, Math.Min(r.Width, r.Height));
        if (d <= 1) { path.AddRectangle(r); return path; }
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void HighQuality(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
    }

    public static Color Blend(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    /// <summary>Turns on double buffering for controls that hide it (Panel, DataGridView, ...).</summary>
    public static void DoubleBuffer(Control c)
    {
        typeof(Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(c, true);
    }

    // ── Factories kept for convenience ─────────────────────────────────────
    public static Label MakeLabel(string text, float size = 9.5f, bool bold = false, Color? color = null) => new()
    {
        Text = text, AutoSize = true, Font = bold ? Semibold(size) : Regular(size),
        ForeColor = color ?? Text, BackColor = Color.Transparent
    };

    public static ModernButton MakeButton(string text, bool primary = false, int height = 40) =>
        new() { Text = text, Height = height, Kind = primary ? ButtonKind.Primary : ButtonKind.Secondary };

    public static RoundedPanel Card() => new() { Padding = new Padding(20) };

    // ── DataGridView styling ───────────────────────────────────────────────
    public static void StyleGrid(DataGridView grid)
    {
        DoubleBuffer(grid);
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SurfaceAlt, ForeColor = Muted, Font = Semibold(8.5f),
            Padding = new Padding(12, 0, 8, 0), SelectionBackColor = SurfaceAlt,
            SelectionForeColor = Muted, Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface, ForeColor = Text, Font = Regular(9.5f),
            Padding = new Padding(12, 0, 8, 0), SelectionBackColor = AccentSoft,
            SelectionForeColor = Text, Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle(grid.DefaultCellStyle) { BackColor = Color.FromArgb(251, 252, 254) };
        grid.RowTemplate.Height = 46;
        grid.RowHeadersVisible = false;
        grid.AllowUserToResizeRows = false;
        grid.AllowUserToResizeColumns = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ScrollBars = ScrollBars.Vertical;
    }
}
