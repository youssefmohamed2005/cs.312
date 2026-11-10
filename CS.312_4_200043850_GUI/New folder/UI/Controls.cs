using System.Drawing.Drawing2D;

namespace StudentManagementSystem.UI;

// ════════════════════════════════════════════════════════════════════════════
//  RoundedPanel — white card with rounded corners, hairline border, soft shadow
// ════════════════════════════════════════════════════════════════════════════
public class RoundedPanel : Panel
{
    public int Radius { get; set; } = Theme.Radius;
    public bool Shadow { get; set; } = true;
    public Color Fill { get; set; } = Theme.Surface;
    public Color Stroke { get; set; } = Theme.Border;

    public RoundedPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Theme.Surface;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var parentColor = Parent?.BackColor ?? Theme.Background;
        if (parentColor.A < 255) parentColor = Theme.Background;
        e.Graphics.Clear(parentColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        Theme.HighQuality(g);
        var rect = new Rectangle(0, 0, Width - 1, Height - (Shadow ? 3 : 1));
        if (rect.Width < 4 || rect.Height < 4) { base.OnPaint(e); return; }

        if (Shadow)
        {
            // layered translucent outlines fake a soft ambient shadow
            for (int i = 1; i <= 3; i++)
            {
                var r = new Rectangle(rect.X, rect.Y + i, rect.Width, rect.Height);
                using var p = Theme.RoundedRect(r, Radius);
                using var pen = new Pen(Color.FromArgb(14 - i * 3, 20, 40, 90), 1.5f);
                g.DrawPath(pen, p);
            }
        }
        using var path = Theme.RoundedRect(rect, Radius);
        using var brush = new SolidBrush(Fill);
        g.FillPath(brush, path);
        using var border = new Pen(Stroke, 1f);
        g.DrawPath(border, path);
        base.OnPaint(e); // raises Paint so subscribers draw on top of the card surface
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  ModernButton — owner-drawn, icon + text, hover / pressed / focus states
// ════════════════════════════════════════════════════════════════════════════
public enum ButtonKind { Primary, Secondary, Danger, Ghost }

public class ModernButton : Control, IButtonControl
{
    private bool _hover, _down, _isDefault;
    private ButtonKind _kind = ButtonKind.Secondary;
    private string _glyph = "";

    public ButtonKind Kind { get => _kind; set { _kind = value; Invalidate(); } }
    public string Glyph { get => _glyph; set { _glyph = value; Invalidate(); } }
    public int Radius { get; set; } = 10;
    public DialogResult DialogResult { get; set; }

    public ModernButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
        Cursor = Cursors.Hand;
        Font = Theme.Semibold(9.5f);
        Size = new Size(120, 40);
        Margin = new Padding(0, 0, 10, 0);
    }

    public void NotifyDefault(bool value) { _isDefault = value; Invalidate(); }
    public void PerformClick() { if (Enabled && Visible) OnClick(EventArgs.Empty); }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _down = true; Focus(); Invalidate(); } base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }
    protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || base.IsInputKey(keyData);
    protected override void OnKeyUp(KeyEventArgs e) { if (e.KeyCode == Keys.Space) PerformClick(); base.OnKeyUp(e); }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var parentColor = Parent?.BackColor ?? Theme.Background;
        if (parentColor.A < 255) parentColor = Theme.Background;
        e.Graphics.Clear(parentColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        Theme.HighQuality(g);

        Color bg, fg, border;
        switch (_kind)
        {
            case ButtonKind.Primary:
                bg = _down ? Theme.Blend(Theme.AccentDark, Color.Black, .12f) : _hover ? Theme.AccentDark : Theme.Accent;
                fg = Color.White; border = bg; break;
            case ButtonKind.Danger:
                bg = _down ? Color.FromArgb(250, 214, 218) : _hover ? Theme.RedSoft : Theme.Surface;
                fg = Theme.Red; border = Theme.Blend(Theme.Red, Color.White, .55f); break;
            case ButtonKind.Ghost:
                bg = _down ? Theme.Blend(Theme.AccentSoft, Theme.Accent, .15f) : _hover ? Theme.AccentSoft : Color.Transparent;
                fg = _hover ? Theme.Accent : Theme.Muted; border = Color.Transparent; break;
            default:
                bg = _down ? Theme.Blend(Theme.AccentSoft, Theme.Accent, .15f) : _hover ? Theme.AccentSoft : Theme.Surface;
                fg = _hover ? Theme.AccentDark : Theme.Text; border = _hover ? Theme.Accent : Theme.BorderStrong; break;
        }
        if (!Enabled)
        {
            bg = _kind == ButtonKind.Primary ? Theme.Blend(Theme.Accent, Color.White, .6f) : Theme.SurfaceAlt;
            fg = _kind == ButtonKind.Primary ? Color.White : Color.FromArgb(160, 170, 188);
            border = _kind == ButtonKind.Primary ? bg : Theme.Border;
        }

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.RoundedRect(rect, Radius);
        if (bg != Color.Transparent) { using var b = new SolidBrush(bg); g.FillPath(b, path); }
        if (border != Color.Transparent) { using var p = new Pen(border, 1f); g.DrawPath(p, path); }

        if (Focused && ShowFocusCues && Enabled)
        {
            var ring = Rectangle.Inflate(rect, -3, -3);
            using var rp = Theme.RoundedRect(ring, Math.Max(2, Radius - 3));
            using var pen = new Pen(Color.FromArgb(_kind == ButtonKind.Primary ? 170 : 110, Theme.Accent), 1.5f);
            g.DrawPath(pen, rp);
        }

        // content: [glyph] [text] centred together
        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
        var iconFont = Theme.Icon(Font.Size + 1.5f);
        bool hasIcon = !string.IsNullOrEmpty(_glyph);
        var textSize = string.IsNullOrEmpty(Text) ? Size.Empty : TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), flags);
        var iconSize = hasIcon ? TextRenderer.MeasureText(_glyph, iconFont, new Size(int.MaxValue, int.MaxValue), flags) : Size.Empty;
        int gap = hasIcon && textSize.Width > 0 ? 8 : 0;
        int total = iconSize.Width + gap + textSize.Width;
        int x = Math.Max(6, (Width - total) / 2);
        if (hasIcon)
        {
            TextRenderer.DrawText(g, _glyph, iconFont, new Rectangle(x, 0, iconSize.Width + 2, Height), fg, flags);
            x += iconSize.Width + gap;
        }
        if (textSize.Width > 0)
            TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, Width - x - 4, Height), fg, flags | TextFormatFlags.EndEllipsis);
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  FieldBox — rounded input shell hosting a TextBox / ComboBox / DateTimePicker
// ════════════════════════════════════════════════════════════════════════════
public class FieldBox : Control
{
    private bool _error, _focused;
    private readonly Control _inner;
    private readonly string _glyph;

    public Control Inner => _inner;
    public string Value => _inner.Text;

    public FieldBox(Control inner, string glyph = "")
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.ContainerControl, true);
        _inner = inner;
        _glyph = glyph;
        Height = 42;
        Margin = new Padding(0);
        BackColor = Theme.Surface;

        _inner.Font = Theme.Regular(10f);
        _inner.BackColor = Theme.Surface;
        _inner.ForeColor = Theme.Text;
        if (_inner is TextBox tb) tb.BorderStyle = BorderStyle.None;
        if (_inner is ComboBox cb) { cb.FlatStyle = FlatStyle.Flat; cb.IntegralHeight = false; }
        _inner.Dock = DockStyle.None;
        _inner.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _inner.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _inner.TextChanged += (_, _) => { if (_error) SetError(false); };
        Controls.Add(_inner);
        LayoutInner();
    }

    public void SetError(bool on) { _error = on; Invalidate(); }

    private int LeftInset => string.IsNullOrEmpty(_glyph) ? 14 : 40;

    private void LayoutInner()
    {
        if (_inner == null) return;
        int w = Math.Max(20, Width - LeftInset - 14);
        _inner.Width = w;
        _inner.Location = new Point(LeftInset, Math.Max(0, (Height - _inner.Height) / 2));
    }

    protected override void OnResize(EventArgs e) { base.OnResize(e); LayoutInner(); }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); LayoutInner(); }
    protected override void OnMouseDown(MouseEventArgs e) { _inner.Focus(); base.OnMouseDown(e); }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var parentColor = Parent?.BackColor ?? Theme.Background;
        if (parentColor.A < 255) parentColor = Theme.Background;
        e.Graphics.Clear(parentColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        Theme.HighQuality(g);
        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = Theme.RoundedRect(rect, 9);
        using (var fill = new SolidBrush(Theme.Surface)) g.FillPath(fill, path);

        if (_focused && !_error)
        {
            using var halo = new Pen(Color.FromArgb(55, Theme.Accent), 4f);
            g.DrawPath(halo, path);
        }
        var color = _error ? Theme.Red : _focused ? Theme.Accent : Theme.BorderStrong;
        using var pen = new Pen(color, _focused || _error ? 1.6f : 1f);
        g.DrawPath(pen, path);

        if (!string.IsNullOrEmpty(_glyph))
            TextRenderer.DrawText(g, _glyph, Theme.Icon(11f), new Rectangle(12, 0, 24, Height),
                _error ? Theme.Red : _focused ? Theme.Accent : Theme.Muted,
                TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  NavButton — sidebar entry with glyph, label and active indicator
// ════════════════════════════════════════════════════════════════════════════
public class NavButton : Control
{
    private bool _hover, _active;
    public string Glyph { get; set; } = "";
    public bool Active { get => _active; set { _active = value; Invalidate(); } }

    public NavButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable, true);
        Cursor = Cursors.Hand;
        Height = 46;
        Margin = new Padding(0, 2, 0, 2);
    }

    public void PerformClick() => OnClick(EventArgs.Empty);

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }
    protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || keyData == Keys.Enter || base.IsInputKey(keyData);
    protected override void OnKeyUp(KeyEventArgs e) { if (e.KeyCode is Keys.Space or Keys.Enter) OnClick(EventArgs.Empty); base.OnKeyUp(e); }
    protected override void OnMouseDown(MouseEventArgs e) { Focus(); base.OnMouseDown(e); }

    protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Theme.Ink);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        Theme.HighQuality(g);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        if (_active || _hover)
        {
            using var path = Theme.RoundedRect(rect, 10);
            using var b = new SolidBrush(_active ? Theme.InkHover : Theme.InkSoft);
            g.FillPath(b, path);
        }
        if (_active)
        {
            using var bar = new SolidBrush(Theme.Gold);
            using var p = Theme.RoundedRect(new Rectangle(0, 11, 4, Height - 22), 2);
            g.FillPath(bar, p);
        }
        if (Focused && ShowFocusCues)
        {
            using var pen = new Pen(Color.FromArgb(120, Theme.Accent), 1.5f);
            using var p = Theme.RoundedRect(Rectangle.Inflate(rect, -1, -1), 10);
            g.DrawPath(pen, p);
        }

        var fg = _active ? Color.White : _hover ? Color.FromArgb(232, 238, 248) : Color.FromArgb(168, 182, 206);
        const TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix;
        TextRenderer.DrawText(g, Glyph, Theme.Icon(12f), new Rectangle(18, 0, 26, Height), _active ? Theme.Gold : fg, f);
        TextRenderer.DrawText(g, Text, _active ? Theme.Semibold(10f) : Theme.Regular(10f), new Rectangle(52, 0, Width - 60, Height), fg, f | TextFormatFlags.EndEllipsis);
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  ToastBanner — non-blocking feedback that replaces most MessageBox popups
// ════════════════════════════════════════════════════════════════════════════
public enum ToastKind { Success, Error, Info }

public class ToastBanner : Control
{
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 3200 };
    private ToastKind _kind = ToastKind.Info;

    public ToastBanner()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        Visible = false;
        Height = 48;
        Font = Theme.Semibold(9.5f);
        _timer.Tick += (_, _) => { _timer.Stop(); Visible = false; };
    }

    public void Display(string message, ToastKind kind = ToastKind.Info)
    {
        _kind = kind;
        Text = message;
        var w = TextRenderer.MeasureText(message, Font).Width + 86;
        Width = Math.Clamp(w, 240, 560);
        if (Parent != null) Location = new Point(Parent.ClientSize.Width - Width - 8, Parent.ClientSize.Height - Height - 8);
        Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        BringToFront();
        Visible = true;
        Invalidate();
        _timer.Stop(); _timer.Start();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var parentColor = Parent?.BackColor ?? Theme.Background;
        if (parentColor.A < 255) parentColor = Theme.Background;
        e.Graphics.Clear(parentColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        Theme.HighQuality(g);
        var (accent, soft, glyph) = _kind switch
        {
            ToastKind.Success => (Theme.Green, Theme.GreenSoft, "\uE73E"),
            ToastKind.Error   => (Theme.Red, Theme.RedSoft, "\uE783"),
            _                 => (Theme.Accent, Theme.AccentSoft, "\uE946")
        };
        var rect = new Rectangle(0, 0, Width - 1, Height - 3);
        using (var shadow = Theme.RoundedRect(new Rectangle(1, 3, Width - 3, Height - 4), 12))
        using (var sp = new Pen(Color.FromArgb(28, 10, 25, 60), 3f)) g.DrawPath(sp, shadow);
        using var path = Theme.RoundedRect(rect, 12);
        using (var b = new SolidBrush(Theme.Surface)) g.FillPath(b, path);
        using (var p = new Pen(Theme.Border)) g.DrawPath(p, path);
        using (var strip = new SolidBrush(accent))
        {
            g.SetClip(path);
            g.FillRectangle(strip, 0, 0, 5, Height);
            g.ResetClip();
        }
        using (var chip = new SolidBrush(soft)) g.FillEllipse(chip, 16, 8, 30, 30);
        const TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;
        TextRenderer.DrawText(g, glyph, Theme.Icon(11f), new Rectangle(16, 8, 30, 30), accent, f | TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g, Text, Font, new Rectangle(58, 0, Width - 70, Height - 3), Theme.Text, f | TextFormatFlags.EndEllipsis);
    }

    protected override void Dispose(bool disposing) { if (disposing) _timer.Dispose(); base.Dispose(disposing); }
}

// ════════════════════════════════════════════════════════════════════════════
//  GlyphBadge — soft-tinted rounded square / circle with an icon glyph
// ════════════════════════════════════════════════════════════════════════════
public class GlyphBadge : Control
{
    public string Glyph { get; set; } = "";
    public Color Tint { get; set; } = Theme.Accent;
    public Color Soft { get; set; } = Theme.AccentSoft;
    public bool Circle { get; set; }
    public float GlyphSize { get; set; } = 13f;

    public GlyphBadge()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(44, 44);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var c = Parent?.BackColor ?? Theme.Surface;
        if (c.A < 255) c = Theme.Surface;
        e.Graphics.Clear(c);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        Theme.HighQuality(g);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var b = new SolidBrush(Soft);
        if (Circle) g.FillEllipse(b, rect);
        else { using var p = Theme.RoundedRect(rect, Math.Max(6, Width / 4)); g.FillPath(b, p); }
        TextRenderer.DrawText(g, Glyph, Theme.Icon(GlyphSize), rect, Tint,
            TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
