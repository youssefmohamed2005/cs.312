using System.Drawing.Drawing2D;
using StudentManagementSystem.Services;
using StudentManagementSystem.UI;

namespace StudentManagementSystem.Views;

public class DashboardView : UserControl
{
    public DashboardView(StudentRepository repository)
    {
        BackColor = Theme.Background;
        DoubleBuffered = true;
        var all = repository.GetAll();
        var byDept = all.GroupBy(s => s.Department).Select(g => (Name: g.Key, Count: g.Count())).OrderByDescending(x => x.Count).ToList();
        int thisYear = all.Count(s => s.EnrollmentDate.Year == DateTime.Now.Year);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Theme.Background, Padding = Padding.Empty, Margin = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // ── Greeting ───────────────────────────────────────────────────────
        var hour = DateTime.Now.Hour;
        var greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        var intro = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
        var hello = Theme.MakeLabel($"{greeting}, Administrator", 15, true); hello.Location = new Point(2, 4);
        var desc = Theme.MakeLabel("Here is what's happening across your university today.", 9.5f, false, Theme.Muted); desc.Location = new Point(3, 36);
        intro.Controls.AddRange([hello, desc]);

        // ── KPI cards ──────────────────────────────────────────────────────
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Theme.Background, Margin = Padding.Empty, Padding = Padding.Empty };
        for (int i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cards.Controls.Add(Metric("Total students", all.Count.ToString(), "Registered student records", Theme.Glyph.Students, Theme.Accent, Theme.AccentSoft, first: true), 0, 0);
        cards.Controls.Add(Metric("Departments", byDept.Count.ToString(), "Departments represented", Theme.Glyph.Department, Theme.Green, Theme.GreenSoft), 1, 0);
        cards.Controls.Add(Metric("New this year", thisYear.ToString(), $"Enrolled in {DateTime.Now.Year}", Theme.Glyph.Add, Color.FromArgb(205, 130, 10), Theme.AmberSoft), 2, 0);
        cards.Controls.Add(Metric("Subjects", "18", "Available subjects (sample)", Theme.Glyph.Subjects, Theme.Violet, Theme.VioletSoft, last: true), 3, 0);

        // ── Charts row ─────────────────────────────────────────────────────
        var lower = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Background, Margin = Padding.Empty, Padding = new Padding(0, 20, 0, 0) };
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
        lower.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var chartCard = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0), Padding = new Padding(14, 76, 14, 14) };
        var chartTitle = Theme.MakeLabel("Enrollment statistics", 11.5f, true); chartTitle.Location = new Point(22, 18);
        var chartSub = Theme.MakeLabel("Yearly enrollment trend (sample data)", 9f, false, Theme.Muted); chartSub.Location = new Point(23, 44);
        chartCard.Controls.Add(new EnrollmentChart { Dock = DockStyle.Fill });
        chartCard.Controls.Add(chartTitle); chartCard.Controls.Add(chartSub);

        var deptCard = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), Padding = new Padding(14, 76, 14, 14) };
        var deptTitle = Theme.MakeLabel("Students by department", 11.5f, true); deptTitle.Location = new Point(22, 18);
        var deptSub = Theme.MakeLabel("Share of all registered students", 9f, false, Theme.Muted); deptSub.Location = new Point(23, 44);
        deptCard.Controls.Add(new DepartmentBars(byDept) { Dock = DockStyle.Fill });
        deptCard.Controls.Add(deptTitle); deptCard.Controls.Add(deptSub);

        lower.Controls.Add(chartCard, 0, 0);
        lower.Controls.Add(deptCard, 1, 0);

        layout.Controls.Add(intro, 0, 0);
        layout.Controls.Add(cards, 0, 1);
        layout.Controls.Add(lower, 0, 2);
        Controls.Add(layout);
    }

    private static Control Metric(string title, string value, string note, string glyph, Color tint, Color soft, bool first = false, bool last = false)
    {
        var card = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(first ? 0 : 8, 0, last ? 0 : 8, 0) };
        var badge = new GlyphBadge { Glyph = glyph, Tint = tint, Soft = soft, Location = new Point(20, 20), Size = new Size(46, 46), GlyphSize = 14f };
        var v = new Label { Text = value, AutoSize = true, Font = Theme.Semibold(24f), ForeColor = Theme.Text, BackColor = Color.Transparent, Location = new Point(78, 12) };
        var t = Theme.MakeLabel(title, 9.5f, false, Theme.Muted); t.Location = new Point(81, 54);
        var n = Theme.MakeLabel(note, 8.5f, false, Theme.Muted); n.Location = new Point(21, 100);
        card.Controls.AddRange([badge, v, t, n]);
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.Border);
            e.Graphics.DrawLine(pen, 20, 90, card.Width - 21, 90);
        };
        return card;
    }

    // ── Smooth area/line chart with hover tooltip ──────────────────────────
    private sealed class EnrollmentChart : Control
    {
        private static readonly string[] Years = ["2022", "2023", "2024", "2025", "2026"];
        private static readonly int[] Values = [32, 48, 61, 78, 94];
        private int _hot = Values.Length - 1;

        public EnrollmentChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
        }

        private PointF[] Points(int left, int right, int top, int bottom)
        {
            float slot = (right - left) / (float)Years.Length;
            var pts = new PointF[Values.Length];
            for (int i = 0; i < Values.Length; i++)
                pts[i] = new PointF(left + slot * (i + 0.5f), bottom - (bottom - top) * Values[i] / 100f);
            return pts;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pts = Points(46, Width - 16, 14, Height - 34);
            int nearest = 0; float best = float.MaxValue;
            for (int i = 0; i < pts.Length; i++) { float d = Math.Abs(pts[i].X - e.X); if (d < best) { best = d; nearest = i; } }
            if (nearest != _hot) { _hot = nearest; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; Theme.HighQuality(g);
            int left = 46, right = Width - 16, top = 14, bottom = Height - 34;
            if (right - left < 60 || bottom - top < 60) return;

            using var gridPen = new Pen(Theme.Border, 1f) { DashStyle = DashStyle.Dash };
            for (int i = 0; i <= 4; i++)
            {
                int y = bottom - (bottom - top) * i / 4;
                g.DrawLine(gridPen, left, y, right, y);
                TextRenderer.DrawText(g, (i * 25).ToString(), Theme.Regular(8.5f), new Rectangle(0, y - 9, left - 8, 18), Theme.Muted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            var pts = Points(left, right, top, bottom);

            // area under curve
            using (var area = new GraphicsPath())
            {
                area.AddCurve(pts, 0.35f);
                area.AddLine(pts[^1], new PointF(pts[^1].X, bottom));
                area.AddLine(new PointF(pts[^1].X, bottom), new PointF(pts[0].X, bottom));
                area.CloseFigure();
                using var fill = new LinearGradientBrush(new Rectangle(left, top, right - left, bottom - top + 1),
                    Color.FromArgb(80, Theme.Accent), Color.FromArgb(0, Theme.Accent), 90f);
                g.FillPath(fill, area);
            }
            using (var line = new Pen(Theme.Accent, 3f) { LineJoin = LineJoin.Round })
                g.DrawCurve(line, pts, 0.35f);

            // markers + x labels
            for (int i = 0; i < pts.Length; i++)
            {
                bool hot = i == _hot;
                float r = hot ? 6.5f : 4.5f;
                using (var ring = new SolidBrush(Color.White)) g.FillEllipse(ring, pts[i].X - r - 1.5f, pts[i].Y - r - 1.5f, (r + 1.5f) * 2, (r + 1.5f) * 2);
                using (var dot = new SolidBrush(Theme.Accent)) g.FillEllipse(dot, pts[i].X - r, pts[i].Y - r, r * 2, r * 2);
                TextRenderer.DrawText(g, Years[i], hot ? Theme.Semibold(9f) : Theme.Regular(9f),
                    new Rectangle((int)pts[i].X - 30, bottom + 8, 60, 20), hot ? Theme.Text : Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }

            // tooltip for the active point
            var p = pts[_hot];
            string label = $"{Years[_hot]}  ·  {Values[_hot]} students";
            var size = TextRenderer.MeasureText(label, Theme.Semibold(9f));
            var tip = new Rectangle((int)p.X - (size.Width + 24) / 2, (int)p.Y - 46, size.Width + 24, 28);
            tip.X = Math.Clamp(tip.X, left - 30, Width - tip.Width - 2);
            if (tip.Y < 0) tip.Y = (int)p.Y + 14;
            using (var tp = Theme.RoundedRect(tip, 8))
            using (var tb = new SolidBrush(Theme.Ink)) g.FillPath(tb, tp);
            TextRenderer.DrawText(g, label, Theme.Semibold(9f), tip, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    // ── Horizontal share bars per department ───────────────────────────────
    private sealed class DepartmentBars : Control
    {
        private readonly List<(string Name, int Count)> _rows;
        private static readonly Color[] Palette = [Theme.Accent, Theme.Green, Theme.Violet, Color.FromArgb(235, 160, 40), Theme.Red];

        public DepartmentBars(List<(string Name, int Count)> rows)
        {
            _rows = rows;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; Theme.HighQuality(g);
            if (_rows.Count == 0)
            {
                TextRenderer.DrawText(g, "No students yet", Theme.Regular(10f), ClientRectangle, Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            int total = _rows.Sum(r => r.Count);
            int rowH = Math.Min(62, Math.Max(46, Height / Math.Max(1, _rows.Count)));
            int y = 4;
            for (int i = 0; i < _rows.Count && y + rowH <= Height + 4; i++)
            {
                var (name, count) = _rows[i];
                var color = Palette[i % Palette.Length];
                double share = total == 0 ? 0 : count * 100.0 / total;
                const TextFormatFlags f = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
                TextRenderer.DrawText(g, name, Theme.Semibold(9.5f), new Rectangle(8, y, Width - 130, 22), Theme.Text, f);
                TextRenderer.DrawText(g, $"{count}  ·  {share:0}%", Theme.Regular(9f), new Rectangle(Width - 128, y, 120, 22), Theme.Muted, f | TextFormatFlags.Right);
                var track = new Rectangle(8, y + 28, Width - 16, 8);
                using (var tp = Theme.RoundedRect(track, 4)) using (var tb = new SolidBrush(Theme.Background)) g.FillPath(tb, tp);
                int fw = Math.Max(8, (int)(track.Width * share / 100.0));
                using (var fp = Theme.RoundedRect(new Rectangle(track.X, track.Y, fw, track.Height), 4)) using (var fb = new SolidBrush(color)) g.FillPath(fb, fp);
                y += rowH;
            }
        }
    }
}
