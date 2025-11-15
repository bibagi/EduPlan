using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.Core.Models;
using Schedule.WinForms.Helpers;

namespace Schedule.WinForms.Forms;

public partial class ScheduleEditorForm : Form
{
    private readonly User _currentUser;
    private ComboBox cmbGroup = null!;
    private TabControl tabWeeks = null!;
    private DataGridView dgvEven = null!;
    private DataGridView dgvOdd = null!;
    private int? _selectedGroupId;

    public ScheduleEditorForm(User user)
    {
        _currentUser = user;
        InitializeComponent();
        LoadGroups();
    }

    private void InitializeComponent()
    {
        this.Text = "Редактор расписания";
        this.Size = new Size(1400, 800);
        this.StartPosition = FormStartPosition.CenterScreen;

        var lblGroup = new Label 
        { 
            Text = "Группа:", 
            Location = new Point(20, 20), 
            Size = new Size(80, 25),
            Font = new Font("Segoe UI", 10)
        };
        cmbGroup = new ComboBox 
        { 
            Location = new Point(110, 20), 
            Size = new Size(250, 25), 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

        tabWeeks = new TabControl 
        { 
            Location = new Point(20, 60), 
            Size = new Size(1350, 680),
            Font = new Font("Segoe UI", 10)
        };

        var tabEven = new TabPage("Чётная неделя");
        dgvEven = CreateScheduleGrid();
        tabEven.Controls.Add(dgvEven);

        var tabOdd = new TabPage("Нечётная неделя");
        dgvOdd = CreateScheduleGrid();
        tabOdd.Controls.Add(dgvOdd);

        tabWeeks.TabPages.Add(tabEven);
        tabWeeks.TabPages.Add(tabOdd);

        this.Controls.AddRange(new Control[] { lblGroup, cmbGroup, tabWeeks });
    }

    private DataGridView CreateScheduleGrid()
    {
        var dgv = new DataGridView
        {
            Location = new Point(10, 10),
            Size = new Size(1320, 620),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.CellSelect
        };
        
        ModernStyles.ApplyModernStyle(dgv);

        dgv.Columns.Add("Lesson", "Урок");
        dgv.Columns.Add("Mon", "Понедельник");
        dgv.Columns.Add("Tue", "Вторник");
        dgv.Columns.Add("Wed", "Среда");
        dgv.Columns.Add("Thu", "Четверг");
        dgv.Columns.Add("Fri", "Пятница");
        dgv.Columns.Add("Sat", "Суббота");

        dgv.Columns[0].Width = 60;
        for (int i = 1; i < 7; i++)
            dgv.Columns[i].Width = 200;

        for (int i = 1; i <= 8; i++)
        {
            dgv.Rows.Add(i.ToString());
            dgv.Rows[i - 1].Height = 70;
        }

        dgv.CellClick += Dgv_CellClick;

        return dgv;
    }

    private void LoadGroups()
    {
        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        var groups = context.Groups.ToList();

        if (groups.Count == 0)
        {
            MessageBox.Show("Нет доступных групп. Сначала создайте группы в справочнике.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        cmbGroup.DisplayMember = "Name";
        cmbGroup.ValueMember = "Id";
        cmbGroup.DataSource = groups;
    }

    private void CmbGroup_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbGroup.DataSource == null)
            return;
            
        if (cmbGroup.SelectedValue is int groupId)
        {
            _selectedGroupId = groupId;
            LoadSchedule();
        }
    }

    public void RefreshSchedule()
    {
        LoadSchedule();
    }

    private void LoadSchedule()
    {
        if (!_selectedGroupId.HasValue)
            return;

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        // Получаем уроки на текущую неделю
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
        var endOfWeek = startOfWeek.AddDays(6);
        
        var lessons = context.Lessons
            .Include(l => l.Subject)
            .Include(l => l.Teacher)
            .Include(l => l.Classroom)
            .Where(l => l.GroupId == _selectedGroupId.Value && l.Date >= startOfWeek && l.Date <= endOfWeek)
            .ToList();

        FillGrid(dgvEven, lessons, startOfWeek);
        FillGrid(dgvOdd, lessons, startOfWeek.AddDays(7));
    }

    private void FillGrid(DataGridView dgv, List<Lesson> lessons, DateTime weekStart)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 1; col < 7; col++)
            {
                int lessonNum = row + 1;
                var lessonDate = weekStart.AddDays(col - 1);

                var lesson = lessons.FirstOrDefault(l =>
                    l.LessonNumber == lessonNum &&
                    l.Date == lessonDate);

                if (lesson != null)
                {
                    dgv.Rows[row].Cells[col].Value = $"{lesson.Subject.Name}\n{lesson.Teacher.ShortName}\n{lesson.Classroom.Name}";
                    dgv.Rows[row].Cells[col].Style.BackColor = Color.LightBlue;
                }
                else
                {
                    dgv.Rows[row].Cells[col].Value = "";
                    dgv.Rows[row].Cells[col].Style.BackColor = Color.White;
                }
            }
        }
    }

    private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 1 || !_selectedGroupId.HasValue)
            return;

        if (_currentUser.Role != "Admin")
        {
            MessageBox.Show("Только администратор может редактировать расписание", "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int lessonNum = e.RowIndex + 1;
        int dayOfWeek = e.ColumnIndex - 1;
        
        // Вычисляем дату на основе дня недели
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
        var lessonDate = startOfWeek.AddDays(dayOfWeek);

        var form = new LessonEditForm(_selectedGroupId.Value, lessonNum, lessonDate);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadSchedule();
        }
    }
}
