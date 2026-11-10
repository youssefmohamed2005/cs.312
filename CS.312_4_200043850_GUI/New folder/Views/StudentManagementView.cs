using System.Text.RegularExpressions;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using StudentManagementSystem.UI;

namespace StudentManagementSystem.Views;

public class StudentManagementView : UserControl
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly (Color Tint, Color Soft)[] Tones =
    [
        (Theme.Accent, Theme.AccentSoft),
        (Theme.Green, Theme.GreenSoft),
        (Theme.Violet, Theme.VioletSoft),
        (Color.FromArgb(190, 115, 10), Theme.AmberSoft),
        (Theme.Red, Theme.RedSoft)
    ];

    private readonly StudentRepository _repository;
    private readonly DataGridView dgvStudents = new() { Name = "dgvStudents", Dock = DockStyle.Fill };
    private readonly TextBox txtName = new() { Name = "txtName", PlaceholderText = "Student full name" };
    private readonly TextBox txtEmail = new() { Name = "txtEmail", PlaceholderText = "name@university.edu" };
    private readonly ComboBox cmbDepartment = new() { Name = "cmbDepartment", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtpEnrollment = new() { Name = "dtpEnrollment", Format = DateTimePickerFormat.Short };
    private readonly TextBox txtSearch = new() { Name = "txtSearch", PlaceholderText = "Search by name, email or ID" };
    private readonly Label lblFormTitle = new() { AutoSize = true, Font = Theme.Semibold(11.5f), ForeColor = Theme.Text, BackColor = Color.Transparent };
    private readonly Label lblFormSub = new() { AutoSize = true, Font = Theme.Regular(9f), ForeColor = Theme.Muted, BackColor = Color.Transparent };
    private readonly Label lblDirSub = new() { AutoSize = true, Font = Theme.Regular(9f), ForeColor = Theme.Muted, BackColor = Color.Transparent };
    private readonly ToastBanner toast = new();

    private FieldBox fbName = null!, fbEmail = null!, fbDept = null!, fbDate = null!, fbSearch = null!;
    private ModernButton btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnClear = null!;
    private int? _selectedId;
    private bool _suppress;

    public StudentManagementView(StudentRepository repository)
    {
        _repository = repository;
        BackColor = Theme.Background;
        Font = Theme.Regular();
        DoubleBuffered = true;
        cmbDepartment.Items.AddRange(["Computer Science", "Information Systems", "Information Technology", "Business Information Systems"]);
        Theme.StyleGrid(dgvStudents);
        BuildLayout();
        RefreshGrid();
        RefreshMode();
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Layout
    // ═════════════════════════════════════════════════════════════════════
    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Background, Margin = Padding.Empty, Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 218));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // ── Form card ──────────────────────────────────────────────────
        var formCard = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 20) };
        lblFormTitle.Location = new Point(22, 16);
        lblFormSub.Location = new Point(23, 42);

        fbName = new FieldBox(txtName, Theme.Glyph.Contact);
        fbEmail = new FieldBox(txtEmail, Theme.Glyph.Mail);
        fbDept = new FieldBox(cmbDepartment, Theme.Glyph.Department);
        fbDate = new FieldBox(dtpEnrollment, Theme.Glyph.Calendar);

        var fields = new TableLayoutPanel
        {
            Location = new Point(20, 70), Size = new Size(900, 70), ColumnCount = 4, RowCount = 2,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Theme.Surface
        };
        for (int i = 0; i < 4; i++) fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        AddField("Full name", fbName, 0);
        AddField("Email address", fbEmail, 1);
        AddField("Department", fbDept, 2);
        AddField("Enrollment date", fbDate, 3);

        var actions = new FlowLayoutPanel { Location = new Point(20, 152), Size = new Size(620, 46), WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Surface };
        btnAdd = new ModernButton { Text = "Add student", Kind = ButtonKind.Primary, Glyph = Theme.Glyph.Add, Size = new Size(150, 44) };
        btnUpdate = new ModernButton { Text = "Update", Kind = ButtonKind.Secondary, Glyph = Theme.Glyph.Edit, Size = new Size(116, 44) };
        btnDelete = new ModernButton { Text = "Delete", Kind = ButtonKind.Danger, Glyph = Theme.Glyph.Delete, Size = new Size(112, 44) };
        btnClear = new ModernButton { Text = "Clear", Kind = ButtonKind.Ghost, Glyph = Theme.Glyph.Clear, Size = new Size(100, 44) };
        btnAdd.Click += (_, _) => AddStudent();
        btnUpdate.Click += (_, _) => UpdateStudent();
        btnDelete.Click += (_, _) => DeleteStudent();
        btnClear.Click += (_, _) => { ClearForm(); txtName.Focus(); };
        actions.Controls.AddRange([btnAdd, btnUpdate, btnDelete, btnClear]);

        formCard.Controls.AddRange([lblFormTitle, lblFormSub, fields, actions]);
        formCard.Resize += (_, _) => fields.Width = Math.Max(520, formCard.ClientSize.Width - 40);

        // ── Directory card ─────────────────────────────────────────────
        var gridCard = new RoundedPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(2, 80, 2, 14) };
        var dirTitle = Theme.MakeLabel("Student directory", 11.5f, true); dirTitle.Location = new Point(22, 16);
        lblDirSub.Location = new Point(23, 42);

        fbSearch = new FieldBox(txtSearch, Theme.Glyph.Search) { Size = new Size(340, 44), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        txtSearch.TextChanged += (_, _) => RefreshGrid(txtSearch.Text);
        gridCard.Resize += (_, _) => fbSearch.Location = new Point(gridCard.ClientSize.Width - fbSearch.Width - 22, 18);

        dgvStudents.Columns.Add("Id", "Student ID");
        dgvStudents.Columns.Add("Name", "Student");
        dgvStudents.Columns.Add("Email", "Email address");
        dgvStudents.Columns.Add("Department", "Department");
        dgvStudents.Columns.Add("EnrollmentDate", "Enrollment date");
        dgvStudents.Columns["Id"]!.FillWeight = 62;
        dgvStudents.Columns["Name"]!.FillWeight = 125;
        dgvStudents.Columns["Email"]!.FillWeight = 130;
        dgvStudents.Columns["Department"]!.FillWeight = 135;
        dgvStudents.Columns["EnrollmentDate"]!.FillWeight = 92;
        dgvStudents.SelectionChanged += (_, _) => LoadSelectedStudent();
        dgvStudents.CellPainting += Grid_CellPainting;
        dgvStudents.Paint += (_, e) =>
        {
            if (dgvStudents.Rows.Count > 0) return;
            TextRenderer.DrawText(e.Graphics, "No students match your search.", Theme.Regular(10f),
                new Rectangle(0, 60, dgvStudents.Width, 40), Theme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        gridCard.Controls.Add(dgvStudents);
        gridCard.Controls.Add(dirTitle);
        gridCard.Controls.Add(lblDirSub);
        gridCard.Controls.Add(fbSearch);

        root.Controls.Add(formCard, 0, 0);
        root.Controls.Add(gridCard, 0, 1);
        Controls.Add(root);
        Controls.Add(toast);
        toast.BringToFront();

        void AddField(string caption, FieldBox box, int col)
        {
            var label = new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Font = Theme.Semibold(9f), ForeColor = Theme.Text, BackColor = Color.Transparent, Margin = new Padding(2, 0, 0, 3) };
            box.Dock = DockStyle.Fill;
            box.Margin = new Padding(0, 0, col == 3 ? 0 : 14, 0);
            fields.Controls.Add(label, col, 0);
            fields.Controls.Add(box, col, 1);
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Grid rendering: avatar + name, department pill, muted ID
    // ═════════════════════════════════════════════════════════════════════
    private static (Color Tint, Color Soft) ToneFor(string key)
    {
        uint hash = 17;
        foreach (var c in key) hash = unchecked(hash * 31 + c);
        return Tones[(int)(hash % (uint)Tones.Length)];
    }

    private static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        return parts.Length == 1 ? parts[0][..1].ToUpperInvariant() : (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is null) return;
        var col = dgvStudents.Columns[e.ColumnIndex].Name;
        if (col is not ("Name" or "Department" or "Id")) return;

        var text = Convert.ToString(e.Value) ?? "";
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);

        var g = e.Graphics;
        var state = g.Save();
        g.SetClip(e.CellBounds);
        Theme.HighQuality(g);
        var b = e.CellBounds;
        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

        switch (col)
        {
            case "Name":
            {
                var (tint, soft) = ToneFor(text);
                var av = new Rectangle(b.X + 12, b.Y + (b.Height - 32) / 2, 32, 32);
                using (var br = new SolidBrush(soft)) g.FillEllipse(br, av);
                TextRenderer.DrawText(g, Initials(text), Theme.Semibold(8.5f), av, tint, TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, text, Theme.Semibold(9.5f), new Rectangle(b.X + 54, b.Y, b.Width - 60, b.Height), Theme.Text, flags);
                break;
            }
            case "Department":
            {
                var (tint, soft) = ToneFor(text);
                var font = Theme.Semibold(8.5f);
                var size = TextRenderer.MeasureText(g, text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                int w = Math.Min(b.Width - 20, size.Width + 24);
                var pill = new Rectangle(b.X + 12, b.Y + (b.Height - 26) / 2, w, 26);
                using (var path = Theme.RoundedRect(pill, 13)) using (var br = new SolidBrush(soft)) g.FillPath(br, path);
                TextRenderer.DrawText(g, text, font, pill, tint, TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                break;
            }
            default:
                TextRenderer.DrawText(g, "#" + text, Theme.Semibold(9f), new Rectangle(b.X + 12, b.Y, b.Width - 16, b.Height), Theme.Muted, flags);
                break;
        }
        g.Restore(state);
        e.Handled = true;
    }

    // ═════════════════════════════════════════════════════════════════════
    //  Data + behaviour
    // ═════════════════════════════════════════════════════════════════════
    private void RefreshGrid(string filter = "")
    {
        _suppress = true;
        try
        {
            dgvStudents.Rows.Clear();
            var all = _repository.GetAll();
            var query = all.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var term = filter.Trim();
                query = query.Where(s => s.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                                      || s.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                                      || s.Id.ToString().Contains(term));
            }
            var rows = query.ToList();
            foreach (var s in rows) dgvStudents.Rows.Add(s.Id, s.Name, s.Email, s.Department, s.EnrollmentDate.ToString("dd MMM yyyy"));
            dgvStudents.CurrentCell = null;
            dgvStudents.ClearSelection();
            lblDirSub.Text = string.IsNullOrWhiteSpace(filter)
                ? $"{all.Count} student{(all.Count == 1 ? "" : "s")} registered"
                : $"Showing {rows.Count} of {all.Count} students";
        }
        finally { _suppress = false; }
    }

    private void RefreshMode()
    {
        bool editing = _selectedId is not null;
        lblFormTitle.Text = editing ? $"Editing student #{_selectedId}" : "New student";
        lblFormSub.Text = editing ? "Change the details below, then press Update." : "Fill in the details below to register a student.";
        btnAdd.Enabled = !editing;
        btnUpdate.Enabled = editing;
        btnDelete.Enabled = editing;
    }

    private bool ValidateForm()
    {
        bool nameOk = !string.IsNullOrWhiteSpace(txtName.Text);
        bool emailOk = EmailPattern.IsMatch(txtEmail.Text.Trim());
        bool deptOk = cmbDepartment.SelectedIndex >= 0;
        fbName.SetError(!nameOk);
        fbEmail.SetError(!emailOk);
        fbDept.SetError(!deptOk);

        if (!nameOk || !emailOk || !deptOk)
        {
            toast.Display("Enter a name, a valid email and a department.", ToastKind.Error);
            return false;
        }
        bool duplicate = _repository.GetAll().Any(s => s.Id != (_selectedId ?? 0) && s.Email.Equals(txtEmail.Text.Trim(), StringComparison.OrdinalIgnoreCase));
        if (duplicate)
        {
            fbEmail.SetError(true);
            toast.Display("A student with this email already exists.", ToastKind.Error);
            return false;
        }
        return true;
    }

    private Student ReadForm() => new()
    {
        Id = _selectedId ?? 0, Name = txtName.Text.Trim(), Email = txtEmail.Text.Trim(),
        Department = cmbDepartment.Text, EnrollmentDate = dtpEnrollment.Value.Date
    };

    private void AddStudent()
    {
        if (!ValidateForm()) return;
        var student = ReadForm();
        _repository.Add(student);
        RefreshGrid(txtSearch.Text); ClearForm();
        toast.Display($"{student.Name} was added.", ToastKind.Success);
    }

    private void UpdateStudent()
    {
        if (_selectedId is null) { toast.Display("Select a student from the table first.", ToastKind.Info); return; }
        if (!ValidateForm()) return;
        var student = ReadForm();
        _repository.Update(student);
        RefreshGrid(txtSearch.Text); ClearForm();
        toast.Display($"{student.Name} was updated.", ToastKind.Success);
    }

    private void DeleteStudent()
    {
        if (_selectedId is null) { toast.Display("Select a student from the table first.", ToastKind.Info); return; }
        var name = txtName.Text.Trim();
        if (MessageBox.Show($"Delete {(string.IsNullOrEmpty(name) ? "the selected student" : name)}? This cannot be undone.", "Confirm deletion",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        _repository.Delete(_selectedId.Value);
        RefreshGrid(txtSearch.Text); ClearForm();
        toast.Display("Student deleted.", ToastKind.Success);
    }

    private void LoadSelectedStudent()
    {
        if (_suppress || dgvStudents.SelectedRows.Count == 0) return;
        var row = dgvStudents.SelectedRows[0];
        if (row.Cells["Id"].Value is null) return;

        _selectedId = Convert.ToInt32(row.Cells["Id"].Value);
        txtName.Text = Convert.ToString(row.Cells["Name"].Value) ?? "";
        txtEmail.Text = Convert.ToString(row.Cells["Email"].Value) ?? "";
        var dept = Convert.ToString(row.Cells["Department"].Value) ?? "";
        if (dept.Length > 0 && !cmbDepartment.Items.Contains(dept)) cmbDepartment.Items.Add(dept);
        cmbDepartment.SelectedItem = dept;
        if (DateTime.TryParse(Convert.ToString(row.Cells["EnrollmentDate"].Value), out var date)) dtpEnrollment.Value = date;
        fbName.SetError(false); fbEmail.SetError(false); fbDept.SetError(false);
        RefreshMode();
    }

    private void ClearForm()
    {
        _suppress = true;
        try
        {
            _selectedId = null;
            txtName.Clear(); txtEmail.Clear();
            cmbDepartment.SelectedIndex = -1;
            dtpEnrollment.Value = DateTime.Today;
            fbName.SetError(false); fbEmail.SetError(false); fbDept.SetError(false);
            dgvStudents.CurrentCell = null;
            dgvStudents.ClearSelection();
        }
        finally { _suppress = false; }
        RefreshMode();
    }
}
