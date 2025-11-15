using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.Core.Models;
using Schedule.WinForms.Helpers;

namespace Schedule.WinForms.Forms;

public partial class DirectoryForm : Form
{
    private readonly string _entityType;
    private DataGridView dataGridView = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;

    public DirectoryForm(string entityType)
    {
        _entityType = entityType;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = $"Справочник: {GetEntityTitle()}";
        this.Size = new Size(900, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = ModernStyles.BackgroundColor;

        dataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 60)
        };
        ModernStyles.ApplyModernStyle(dataGridView);

        var pnlButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = ModernStyles.BackgroundColor,
            Padding = new Padding(20, 12, 20, 12)
        };

        btnAdd = ModernStyles.CreateModernButton("Добавить");
        btnAdd.Location = new Point(20, 12);
        btnAdd.Width = 120;
        btnAdd.Click += BtnAdd_Click;

        btnEdit = ModernStyles.CreateModernButton("Изменить");
        btnEdit.Location = new Point(150, 12);
        btnEdit.Width = 120;
        btnEdit.Click += BtnEdit_Click;

        btnDelete = ModernStyles.CreateModernButton("Удалить", ModernStyles.DangerColor);
        btnDelete.Location = new Point(280, 12);
        btnDelete.Width = 120;
        btnDelete.Click += BtnDelete_Click;

        var btnImport = ModernStyles.CreateModernButton("Импорт", ModernStyles.SuccessColor);
        btnImport.Location = new Point(410, 12);
        btnImport.Width = 120;
        btnImport.Click += async (s, e) => await BtnImport_ClickAsync();

        pnlButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnImport });
        this.Controls.AddRange(new Control[] { dataGridView, pnlButtons });
    }

    private string GetEntityTitle()
    {
        return _entityType switch
        {
            "Teachers" => "Учителя",
            "Classrooms" => "Аудитории",
            "Subjects" => "Предметы",
            "Groups" => "Группы",
            _ => _entityType
        };
    }

    public void RefreshData()
    {
        LoadData();
    }

    private void LoadData()
    {
        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        switch (_entityType)
        {
            case "Teachers":
                dataGridView.DataSource = context.Teachers.ToList();
                RenameTeacherColumns();
                break;
            case "Classrooms":
                dataGridView.DataSource = context.Classrooms.ToList();
                RenameClassroomColumns();
                break;
            case "Subjects":
                dataGridView.DataSource = context.Subjects.ToList();
                RenameSubjectColumns();
                break;
            case "Groups":
                dataGridView.DataSource = context.Groups.ToList();
                RenameGroupColumns();
                break;
        }
    }

    private void RenameTeacherColumns()
    {
        if (dataGridView.Columns["Id"] != null)
            dataGridView.Columns["Id"]!.HeaderText = "ID";
        if (dataGridView.Columns["FullName"] != null)
            dataGridView.Columns["FullName"]!.HeaderText = "Полное имя";
        if (dataGridView.Columns["ShortName"] != null)
            dataGridView.Columns["ShortName"]!.HeaderText = "Краткое имя";
        if (dataGridView.Columns["Lessons"] != null)
            dataGridView.Columns["Lessons"]!.Visible = false;
    }

    private void RenameClassroomColumns()
    {
        if (dataGridView.Columns["Id"] != null)
            dataGridView.Columns["Id"]!.HeaderText = "ID";
        if (dataGridView.Columns["Name"] != null)
            dataGridView.Columns["Name"]!.HeaderText = "Номер аудитории";
        if (dataGridView.Columns["Capacity"] != null)
            dataGridView.Columns["Capacity"]!.HeaderText = "Вместимость";
        if (dataGridView.Columns["Lessons"] != null)
            dataGridView.Columns["Lessons"]!.Visible = false;
    }

    private void RenameSubjectColumns()
    {
        if (dataGridView.Columns["Id"] != null)
            dataGridView.Columns["Id"]!.HeaderText = "ID";
        if (dataGridView.Columns["Name"] != null)
            dataGridView.Columns["Name"]!.HeaderText = "Название предмета";
        if (dataGridView.Columns["Lessons"] != null)
            dataGridView.Columns["Lessons"]!.Visible = false;
    }

    private void RenameGroupColumns()
    {
        if (dataGridView.Columns["Id"] != null)
            dataGridView.Columns["Id"]!.HeaderText = "ID";
        if (dataGridView.Columns["Name"] != null)
            dataGridView.Columns["Name"]!.HeaderText = "Название группы";
        if (dataGridView.Columns["Year"] != null)
            dataGridView.Columns["Year"]!.HeaderText = "Курс";
        if (dataGridView.Columns["Lessons"] != null)
            dataGridView.Columns["Lessons"]!.Visible = false;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var form = new EntityEditForm(_entityType, null);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите запись для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedRow = dataGridView.SelectedRows[0];
        int id = (int)selectedRow.Cells["Id"].Value;

        var form = new EntityEditForm(_entityType, id);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите запись для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
            return;

        var selectedRow = dataGridView.SelectedRows[0];
        int id = (int)selectedRow.Cells["Id"].Value;

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        try
        {
            switch (_entityType)
            {
                case "Teachers":
                    var teacher = context.Teachers.Find(id);
                    if (teacher != null) context.Teachers.Remove(teacher);
                    break;
                case "Classrooms":
                    var classroom = context.Classrooms.Find(id);
                    if (classroom != null) context.Classrooms.Remove(classroom);
                    break;
                case "Subjects":
                    var subject = context.Subjects.Find(id);
                    if (subject != null) context.Subjects.Remove(subject);
                    break;
                case "Groups":
                    var group = context.Groups.Find(id);
                    if (group != null) context.Groups.Remove(group);
                    break;
            }

            context.SaveChanges();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task BtnImport_ClickAsync()
    {
        using var openDialog = new OpenFileDialog
        {
            Filter = "Текстовые файлы (*.txt;*.csv)|*.txt;*.csv|Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Выберите файл для импорта"
        };

        if (openDialog.ShowDialog() != DialogResult.OK)
            return;

        // Показываем инструкцию
        var instruction = GetImportInstruction();
        var result = MessageBox.Show(
            $"Формат импорта для {GetEntityTitle()}:\n\n{instruction}\n\nПродолжить импорт?",
            "Инструкция по импорту",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);

        if (result != DialogResult.Yes)
            return;

        try
        {
            using var scope = Program.ServiceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<Schedule.Core.Services.IImportService>();

            (int success, int failed, List<string> errors) importResult;

            if (openDialog.FileName.EndsWith(".xlsx"))
            {
                importResult = await importService.ImportFromExcelAsync(openDialog.FileName, _entityType);
            }
            else
            {
                importResult = await importService.ImportFromTextAsync(openDialog.FileName, _entityType);
            }

            // Показываем результат
            var message = $"Импорт завершён!\n\nУспешно: {importResult.success}\nОшибок: {importResult.failed}";
            
            if (importResult.errors.Count > 0)
            {
                message += "\n\nОшибки:\n" + string.Join("\n", importResult.errors.Take(10));
                if (importResult.errors.Count > 10)
                    message += $"\n... и ещё {importResult.errors.Count - 10} ошибок";
            }

            MessageBox.Show(message, "Результат импорта", MessageBoxButtons.OK, 
                importResult.failed > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string GetImportInstruction()
    {
        return _entityType switch
        {
            "Teachers" => "Каждая строка: Полное имя [TAB/;/,] Краткое имя\nПример:\nИванов Иван Иванович\tИванов И.И.\nПетрова Мария Сергеевна;Петрова М.С.",
            "Classrooms" => "Каждая строка: Название [TAB/;/,] Вместимость\nПример:\n101\t30\n102;25\nСпортзал,50",
            "Subjects" => "Каждая строка: Название предмета\nПример:\nМатематика\nРусский язык\nФизика",
            "Groups" => "Каждая строка: Название [TAB/;/,] Курс\nПример:\nИС-21\t2\nИС-22;2\nПО-31,3",
            _ => "Неизвестный формат"
        };
    }
}
