using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.Core.Models;

namespace Schedule.WinForms.Forms;

public partial class EntityEditForm : Form
{
    private readonly string _entityType;
    private readonly int? _entityId;
    private Dictionary<string, TextBox> _textBoxes = new();
    private Button btnSave = null!;
    private Button btnCancel = null!;

    public EntityEditForm(string entityType, int? entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = _entityId.HasValue ? "Редактирование" : "Добавление";
        this.Size = new Size(500, 350);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        int y = 30;

        switch (_entityType)
        {
            case "Teachers":
                AddField("FullName", "Полное имя:", ref y);
                AddField("ShortName", "Краткое имя:", ref y);
                break;
            case "Classrooms":
                AddField("Name", "Название:", ref y);
                AddField("Capacity", "Вместимость:", ref y);
                break;
            case "Subjects":
                AddField("Name", "Название:", ref y);
                break;
            case "Groups":
                AddField("Name", "Название:", ref y);
                AddField("Year", "Курс:", ref y);
                break;
        }

        btnSave = new Button 
        { 
            Text = "Сохранить", 
            Location = new Point(150, y + 30), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button 
        { 
            Text = "Отмена", 
            Location = new Point(280, y + 30), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.Add(btnSave);
        this.Controls.Add(btnCancel);
    }

    private void AddField(string fieldName, string label, ref int y)
    {
        var lbl = new Label 
        { 
            Text = label, 
            Location = new Point(30, y), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        var txt = new TextBox 
        { 
            Location = new Point(190, y), 
            Size = new Size(250, 25),
            Font = new Font("Segoe UI", 10)
        };
        _textBoxes[fieldName] = txt;
        this.Controls.Add(lbl);
        this.Controls.Add(txt);
        y += 50;
    }

    private void LoadData()
    {
        if (!_entityId.HasValue)
            return;

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        switch (_entityType)
        {
            case "Teachers":
                var teacher = context.Teachers.Find(_entityId.Value);
                if (teacher != null)
                {
                    _textBoxes["FullName"].Text = teacher.FullName;
                    _textBoxes["ShortName"].Text = teacher.ShortName;
                }
                break;
            case "Classrooms":
                var classroom = context.Classrooms.Find(_entityId.Value);
                if (classroom != null)
                {
                    _textBoxes["Name"].Text = classroom.Name;
                    _textBoxes["Capacity"].Text = classroom.Capacity.ToString();
                }
                break;
            case "Subjects":
                var subject = context.Subjects.Find(_entityId.Value);
                if (subject != null)
                {
                    _textBoxes["Name"].Text = subject.Name;
                }
                break;
            case "Groups":
                var group = context.Groups.Find(_entityId.Value);
                if (group != null)
                {
                    _textBoxes["Name"].Text = group.Name;
                    _textBoxes["Year"].Text = group.Year.ToString();
                }
                break;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        foreach (var kvp in _textBoxes)
        {
            if (string.IsNullOrWhiteSpace(kvp.Value.Text))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        try
        {
            switch (_entityType)
            {
                case "Teachers":
                    SaveTeacher(context);
                    break;
                case "Classrooms":
                    SaveClassroom(context);
                    break;
                case "Subjects":
                    SaveSubject(context);
                    break;
                case "Groups":
                    SaveGroup(context);
                    break;
            }

            context.SaveChanges();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveTeacher(ScheduleDbContext context)
    {
        Teacher teacher;
        if (_entityId.HasValue)
        {
            teacher = context.Teachers.Find(_entityId.Value)!;
        }
        else
        {
            teacher = new Teacher();
            context.Teachers.Add(teacher);
        }

        teacher.FullName = _textBoxes["FullName"].Text;
        teacher.ShortName = _textBoxes["ShortName"].Text;
    }

    private void SaveClassroom(ScheduleDbContext context)
    {
        Classroom classroom;
        if (_entityId.HasValue)
        {
            classroom = context.Classrooms.Find(_entityId.Value)!;
        }
        else
        {
            classroom = new Classroom();
            context.Classrooms.Add(classroom);
        }

        classroom.Name = _textBoxes["Name"].Text;
        classroom.Capacity = int.Parse(_textBoxes["Capacity"].Text);
    }

    private void SaveSubject(ScheduleDbContext context)
    {
        Subject subject;
        if (_entityId.HasValue)
        {
            subject = context.Subjects.Find(_entityId.Value)!;
        }
        else
        {
            subject = new Subject();
            context.Subjects.Add(subject);
        }

        subject.Name = _textBoxes["Name"].Text;
    }

    private void SaveGroup(ScheduleDbContext context)
    {
        Group group;
        if (_entityId.HasValue)
        {
            group = context.Groups.Find(_entityId.Value)!;
        }
        else
        {
            group = new Group();
            context.Groups.Add(group);
        }

        group.Name = _textBoxes["Name"].Text;
        group.Year = int.Parse(_textBoxes["Year"].Text);
    }
}
