using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.Core.Models;
using Schedule.Core.Services;

namespace Schedule.WinForms.Forms;

/// <summary>
/// Форма просмотра расписания: Дни × Группы (все группы в одной таблице)
/// </summary>
public partial class WeekScheduleForm : Form
{
    private readonly User _currentUser;
    private DataGridView dgvSchedule = null!;
    private DateTimePicker dtpWeekStart = null!;
    private Button btnPrevWeek = null!;
    private Button btnNextWeek = null!;
    private Button btnExportPdf = null!;
    private Button btnExportExcel = null!;
    private Button btnImport = null!;
    private DateTime _currentWeekStart;

    public WeekScheduleForm(User user)
    {
        _currentUser = user;
        _currentWeekStart = GetMonday(DateTime.Today);
        InitializeComponent();
        LoadSchedule();
    }

    private void InitializeComponent()
    {
        this.Text = "Расписание на неделю (все группы)";
        this.Size = new Size(1600, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;

        // Панель управления
        var pnlControls = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(240, 240, 240),
            Padding = new Padding(10)
        };

        btnPrevWeek = new Button
        {
            Text = "◄ Предыдущая неделя",
            Location = new Point(10, 10),
            Size = new Size(150, 30),
            Font = new Font("Segoe UI", 9)
        };
        btnPrevWeek.Click += (s, e) => { _currentWeekStart = _currentWeekStart.AddDays(-7); LoadSchedule(); };

        dtpWeekStart = new DateTimePicker
        {
            Location = new Point(170, 10),
            Size = new Size(200, 30),
            Format = DateTimePickerFormat.Short,
            Value = _currentWeekStart
        };
        dtpWeekStart.ValueChanged += (s, e) => { _currentWeekStart = GetMonday(dtpWeekStart.Value); LoadSchedule(); };

        btnNextWeek = new Button
        {
            Text = "Следующая неделя ►",
            Location = new Point(380, 10),
            Size = new Size(150, 30),
            Font = new Font("Segoe UI", 9)
        };
        btnNextWeek.Click += (s, e) => { _currentWeekStart = _currentWeekStart.AddDays(7); LoadSchedule(); };

        btnImport = new Button
        {
            Text = "📥 Импорт расписания",
            Location = new Point(550, 10),
            Size = new Size(150, 30),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(0, 150, 136),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnImport.FlatAppearance.BorderSize = 0;
        btnImport.Click += async (s, e) => await BtnImport_ClickAsync();

        btnExportPdf = new Button
        {
            Text = "📄 PDF",
            Location = new Point(720, 10),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnExportPdf.FlatAppearance.BorderSize = 0;
        btnExportPdf.Click += async (s, e) => await BtnExportPdf_ClickAsync();

        btnExportExcel = new Button
        {
            Text = "📊 Excel",
            Location = new Point(810, 10),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnExportExcel.FlatAppearance.BorderSize = 0;
        btnExportExcel.Click += async (s, e) => await BtnExportExcel_ClickAsync();

        pnlControls.Controls.AddRange(new Control[] { btnPrevWeek, dtpWeekStart, btnNextWeek, btnImport, btnExportPdf, btnExportExcel });

        // Таблица расписания
        dgvSchedule = new DataGridView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 8),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            RowHeadersWidth = 150,
            ColumnHeadersHeight = 40,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        this.Controls.Add(dgvSchedule);
        this.Controls.Add(pnlControls);
    }

    private DateTime GetMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    public void RefreshSchedule()
    {
        LoadSchedule();
    }

    private void LoadSchedule()
    {
        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        dtpWeekStart.Value = _currentWeekStart;

        var endOfWeek = _currentWeekStart.AddDays(5); // Пн-Сб

        // Загружаем все группы
        var groups = context.Groups.OrderBy(g => g.Name).ToList();

        // Загружаем все уроки на неделю
        var lessons = context.Lessons
            .Include(l => l.Group)
            .Include(l => l.Subject)
            .Include(l => l.Teacher)
            .Include(l => l.Classroom)
            .Where(l => l.Date >= _currentWeekStart && l.Date <= endOfWeek)
            .OrderBy(l => l.Date)
            .ThenBy(l => l.LessonNumber)
            .ToList();

        // Очищаем таблицу
        dgvSchedule.Columns.Clear();
        dgvSchedule.Rows.Clear();

        // Создаём колонки: Дата/День + Группы
        dgvSchedule.Columns.Add("Day", "День");
        dgvSchedule.Columns[0].Width = 150;
        dgvSchedule.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvSchedule.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

        foreach (var group in groups)
        {
            var col = dgvSchedule.Columns.Add($"Group_{group.Id}", group.Name);
            dgvSchedule.Columns[col].Width = 200;
            dgvSchedule.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
        }

        // Заполняем строки по дням
        for (int day = 0; day < 6; day++)
        {
            var currentDate = _currentWeekStart.AddDays(day);
            string[] dayNames = { "ПОНЕД", "ВТОР", "СРЕДА", "ЧЕТВ", "ПЯТН", "СУББ" };
            string dayHeader = $"{dayNames[day]}\n{currentDate:dd.MM.yyyy}";

            int rowIndex = dgvSchedule.Rows.Add();
            dgvSchedule.Rows[rowIndex].Cells[0].Value = dayHeader;
            dgvSchedule.Rows[rowIndex].MinimumHeight = 120;

            // Заполняем уроки для каждой группы
            for (int groupIdx = 0; groupIdx < groups.Count; groupIdx++)
            {
                var group = groups[groupIdx];
                var dayLessons = lessons
                    .Where(l => l.Date == currentDate && l.GroupId == group.Id)
                    .OrderBy(l => l.LessonNumber)
                    .ToList();

                if (dayLessons.Any())
                {
                    var lessonsText = string.Join("\n", dayLessons.Select(l =>
                        $"{l.LessonNumber} пара: {l.Subject.Name}\n   {l.Teacher.ShortName}, ауд.{l.Classroom.Name}"));

                    dgvSchedule.Rows[rowIndex].Cells[groupIdx + 1].Value = lessonsText;
                    dgvSchedule.Rows[rowIndex].Cells[groupIdx + 1].Style.BackColor = Color.FromArgb(230, 240, 255);
                }
            }
        }
    }

    private async Task BtnImport_ClickAsync()
    {
        if (_currentUser.Role != "Admin")
        {
            MessageBox.Show("Только администратор может импортировать расписание", "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var openDialog = new OpenFileDialog
        {
            Filter = "Текстовые файлы (*.txt;*.csv)|*.txt;*.csv|Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Выберите файл расписания"
        };

        if (openDialog.ShowDialog() != DialogResult.OK)
            return;

        var instruction = @"Формат импорта расписания:

Каждая строка: Дата;Группа;Номер пары;Предмет;Учитель;Аудитория

Пример:
10.11.2025;ИС-21;1;Математика;Иванов И.И.;101
10.11.2025;ИС-21;2;Физика;Петрова М.С.;102
11.11.2025;ПО-31;1;Информатика;Сидоров П.А.;201

Продолжить импорт?";

        var result = MessageBox.Show(instruction, "Инструкция по импорту", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result != DialogResult.Yes)
            return;

        try
        {
            using var scope = Program.ServiceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IScheduleImportService>();

            var importResult = await importService.ImportScheduleAsync(openDialog.FileName);

            var message = $"Импорт завершён!\n\nУспешно: {importResult.success}\nОшибок: {importResult.failed}";

            if (importResult.errors.Count > 0)
            {
                message += "\n\nОшибки:\n" + string.Join("\n", importResult.errors.Take(10));
                if (importResult.errors.Count > 10)
                    message += $"\n... и ещё {importResult.errors.Count - 10} ошибок";
            }

            MessageBox.Show(message, "Результат импорта", MessageBoxButtons.OK,
                importResult.failed > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            LoadSchedule();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task BtnExportPdf_ClickAsync()
    {
        using var saveDialog = new SaveFileDialog 
        { 
            Filter = "PDF Files|*.pdf", 
            FileName = $"schedule_week_{_currentWeekStart:yyyy-MM-dd}.pdf" 
        };
        
        if (saveDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            using var scope = Program.ServiceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
            
            var endOfWeek = _currentWeekStart.AddDays(5);
            var lessons = context.Lessons
                .Include(l => l.Group)
                .Include(l => l.Subject)
                .Include(l => l.Teacher)
                .Include(l => l.Classroom)
                .Where(l => l.Date >= _currentWeekStart && l.Date <= endOfWeek)
                .ToList();

            var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
            
            // Создаём временную группу для экспорта всех групп
            var allGroups = new Group { Name = "Все группы" };
            var pdfBytes = await exportService.ExportToPdfAsync(allGroups, lessons, true);
            
            await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);

            MessageBox.Show("Экспорт в PDF выполнен успешно", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task BtnExportExcel_ClickAsync()
    {
        using var saveDialog = new SaveFileDialog 
        { 
            Filter = "Excel Files|*.xlsx", 
            FileName = $"schedule_week_{_currentWeekStart:yyyy-MM-dd}.xlsx" 
        };
        
        if (saveDialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            using var scope = Program.ServiceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
            
            var endOfWeek = _currentWeekStart.AddDays(5);
            
            // Получаем все группы
            var groups = context.Groups.OrderBy(g => g.Name).ToList();
            
            // Получаем все уроки на неделю
            var lessons = context.Lessons
                .Include(l => l.Group)
                .Include(l => l.Subject)
                .Include(l => l.Teacher)
                .Include(l => l.Classroom)
                .Where(l => l.Date >= _currentWeekStart && l.Date <= endOfWeek)
                .ToList();

            var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
            
            // Экспортируем все группы в один файл
            var excelBytes = await exportService.ExportAllGroupsToExcelAsync(groups, lessons, _currentWeekStart);
            
            await File.WriteAllBytesAsync(saveDialog.FileName, excelBytes);

            MessageBox.Show("Экспорт в Excel выполнен успешно", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
