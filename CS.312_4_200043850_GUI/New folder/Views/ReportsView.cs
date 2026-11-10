using StudentManagementSystem.Services;
using StudentManagementSystem.UI;

namespace StudentManagementSystem.Views;

public class ReportsView : UserControl
{
    private readonly StudentRepository _repository;
    private readonly DataGridView dgvReport = new() { Name = "dgvReport", Dock = DockStyle.Fill };
    private readonly SlimProgress progress = new() { Name = "progressReport" };
    private readonly ModernButton btnGenerate = new() { Text = "Generate report", Kind = ButtonKind.Primary, Glyph = Theme.Glyph.Refresh, Size = new Size(172, 44) };
    private readonly ModernButton btnExport = new() { Text = "Export CSV", Kind = ButtonKind.Secondary, Glyph = Theme.Glyph.Download, Size = new Size(148, 44) };
    private readonly ModernButton btnSimulate = new() { Text = "Run async task", Kind = ButtonKind.Secondary, Glyph = Theme.Glyph.Play, Size = new Size(172, 44) };
    private readonly Label lblStatus = new() { Text = "Ready", AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = Theme.Regular(9f), ForeColor = Theme.Muted, BackColor = Color.Transparent };
    private readonly Label lblGridSub = new() { AutoSize = true, Font = Theme.Regular(9f), ForeColor = Theme.Muted, BackColor = Color.Transparent };
    private readonly ToastBanner toast = new();

    public ReportsView(StudentRepository repository)
    {
        _repository = repository;
        BackColor = Theme.Background;
        DoubleBuffered = true;
        Theme.StyleGrid(dgvReport);
        BuildLayout();
        GenerateReport();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Background, Margin = Padding.Empty, Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 196));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // ── Generator card ─────────────────────────────────────────────
        var options = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 20) };
        var title = Theme.MakeLabel("Report generator", 11.5f, true); title.Location = new Point(22, 16);
        var description = Theme.MakeLabel("Generate a department-wise student summary, then export it for review.", 9f, false, Theme.Muted); description.Location = new Point(23, 42);

        var buttons = new FlowLayoutPanel { Location = new Point(20, 76), Size = new Size(560, 46), WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Surface };
        btnGenerate.Click += (_, _) => { GenerateReport(); toast.Display("Report updated.", ToastKind.Success); };
        btnExport.Click += (_, _) => ExportReport();
        btnSimulate.Click += async (_, _) => await SimulateAsync();
        buttons.Controls.AddRange([btnGenerate, btnExport, btnSimulate]);

        var progressLabel = Theme.MakeLabel("Task progress", 9f, true, Theme.Text); progressLabel.Location = new Point(23, 138);
        progress.Location = new Point(22, 164);
        progress.Height = 10;
        progress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Size = new Size(320, 22);
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        options.Controls.AddRange([title, description, buttons, progressLabel, progress, lblStatus]);
        options.Resize += (_, _) =>
        {
            progress.Width = Math.Max(120, options.ClientSize.Width - 44);
            lblStatus.Location = new Point(options.ClientSize.Width - lblStatus.Width - 22, 134);
        };

        // ── Report grid card ───────────────────────────────────────────
        var reportCard = new RoundedPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(2, 80, 2, 14) };
        var reportTitle = Theme.MakeLabel("Student distribution by department", 11.5f, true); reportTitle.Location = new Point(22, 16);
        lblGridSub.Location = new Point(23, 42);
        reportCard.Controls.Add(dgvReport);
        reportCard.Controls.Add(reportTitle);
        reportCard.Controls.Add(lblGridSub);

        dgvReport.Columns.Add("Department", "Department");
        dgvReport.Columns.Add("Count", "Students");
        dgvReport.Columns.Add("Share", "Distribution");
        dgvReport.Columns["Department"]!.FillWeight = 140;
        dgvReport.Columns["Count"]!.FillWeight = 60;
        dgvReport.Columns["Share"]!.FillWeight = 110;
        dgvReport.CellPainting += Grid_CellPainting;
        dgvReport.SelectionChanged += (_, _) => dgvReport.ClearSelection();   // read-only report: no highlight
        dgvReport.Paint += (_, e) =>
        {
            if (dgvReport.Rows.Count > 0) return;
            TextRenderer.DrawText(e.Graphics, "No data to report yet.", Theme.Regular(10f), new Rectangle(0, 60, dgvReport.Width, 40),
                Theme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        root.Controls.Add(options, 0, 0);
        root.Controls.Add(reportCard, 0, 1);
        Controls.Add(root);
        Controls.Add(toast);
        toast.BringToFront();
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is null) return;
        var col = dgvReport.Columns[e.ColumnIndex].Name;
        if (col is not ("Share" or "Department" or "Count")) return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
        var g = e.Graphics;
        var state = g.Save();
        g.SetClip(e.CellBounds);
        Theme.HighQuality(g);
        var b = e.CellBounds;
        var text = Convert.ToString(e.Value) ?? "";
        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

        if (col == "Department")
            TextRenderer.DrawText(g, text, Theme.Semibold(9.5f), new Rectangle(b.X + 12, b.Y, b.Width - 16, b.Height), Theme.Text, flags);
        else if (col == "Count")
            TextRenderer.DrawText(g, text, Theme.Semibold(10f), new Rectangle(b.X + 12, b.Y, b.Width - 16, b.Height), Theme.Text, flags);
        else
        {
            double pct = double.TryParse(text.TrimEnd('%'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
            const int labelW = 58;
            var track = new Rectangle(b.X + 12, b.Y + (b.Height - 8) / 2, Math.Max(20, b.Width - labelW - 28), 8);
            using (var tp = Theme.RoundedRect(track, 4)) using (var tb = new SolidBrush(Theme.Background)) g.FillPath(tb, tp);
            int fw = Math.Max(8, (int)(track.Width * pct / 100.0));
            using (var fp = Theme.RoundedRect(new Rectangle(track.X, track.Y, fw, track.Height), 4)) using (var fb = new SolidBrush(Theme.Accent)) g.FillPath(fb, fp);
            TextRenderer.DrawText(g, text, Theme.Semibold(9f), new Rectangle(track.Right + 10, b.Y, labelW, b.Height), Theme.Muted, flags);
        }
        g.Restore(state);
        e.Handled = true;
    }

    private void GenerateReport()
    {
        dgvReport.Rows.Clear();
        var students = _repository.GetAll();
        foreach (var group in students.GroupBy(s => s.Department).OrderBy(g => g.Key))
        {
            double share = students.Count == 0 ? 0 : group.Count() * 100.0 / students.Count;
            dgvReport.Rows.Add(group.Key, group.Count(), $"{share.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}%");
        }
        dgvReport.ClearSelection();
        lblStatus.Text = $"Report updated · {students.Count} students";
        lblGridSub.Text = $"{dgvReport.Rows.Count} department{(dgvReport.Rows.Count == 1 ? "" : "s")} · {students.Count} student{(students.Count == 1 ? "" : "s")} in total";
    }

    private void ExportReport()
    {
        using var dialog = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = "student-report.csv" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            var lines = new List<string> { "Department,Student Count,Distribution" };
            foreach (DataGridViewRow row in dgvReport.Rows)
                if (!row.IsNewRow)
                    lines.Add($"\"{Convert.ToString(row.Cells[0].Value)?.Replace("\"", "\"\"")}\",{row.Cells[1].Value},{row.Cells[2].Value}");
            File.WriteAllLines(dialog.FileName, lines, new System.Text.UTF8Encoding(true));
            toast.Display("Report exported successfully.", ToastKind.Success);
        }
        catch (Exception ex)
        {
            toast.Display($"Export failed: {ex.Message}", ToastKind.Error);
        }
    }

    private async Task SimulateAsync()
    {
        btnSimulate.Enabled = false; btnGenerate.Enabled = false; btnExport.Enabled = false;
        progress.Value = 0; lblStatus.Text = "Processing...";
        for (int value = 0; value <= 100; value += 4)
        {
            await Task.Delay(65);
            if (IsDisposed) return;
            progress.Value = value;
            lblStatus.Text = $"Processing... {value}%";
        }
        if (IsDisposed) return;
        lblStatus.Text = "Task completed successfully";
        toast.Display("Task completed successfully.", ToastKind.Success);
        btnSimulate.Enabled = true; btnGenerate.Enabled = true; btnExport.Enabled = true;
    }

    // ── Slim rounded progress bar ──────────────────────────────────────────
    private sealed class SlimProgress : Control
    {
        private int _value;
        public int Value { get => _value; set { _value = Math.Clamp(value, 0, 100); Invalidate(); } }

        public SlimProgress()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 10;
        }

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Theme.Surface);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; Theme.HighQuality(g);
            var track = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var tp = Theme.RoundedRect(track, Height / 2)) using (var tb = new SolidBrush(Theme.Background)) g.FillPath(tb, tp);
            if (_value <= 0) return;
            int w = Math.Max(Height, (int)(track.Width * _value / 100.0));
            using var fp = Theme.RoundedRect(new Rectangle(0, 0, w, track.Height), Height / 2);
            using var fb = new SolidBrush(_value >= 100 ? Theme.Green : Theme.Accent);
            g.FillPath(fb, fp);
        }
    }
}
