using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.Core.Models;

namespace Schedule.WinForms.Forms;

public partial class LessonEditForm : Form
{
    private readonly int _groupId;
    private readonly int _lessonNumber;
    private readonly DateTime _date;
    private ComboBox cmbSubject = null!;
    private ComboBox cmbTeacher = null!;
    private ComboBox cmbClassroom = null!;
    private Button btnSave = null!;
    private Button btnDelete = null!;
    private Button btnCancel = null!;
    private Lesson? _existingLesson;

    public LessonEditForm(int groupId, int lessonNumber, DateTime date)
    {
        _groupId = groupId;
        _lessonNumber = lessonNumber;
        _date = date;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Редактирование урока";
        this.Size = new Size(500, 350);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblSubject = new Label 
        { 
            Text = "Предмет:", 
            Location = new Point(30, 30), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        cmbSubject = new ComboBox 
        { 
            Location = new Point(190, 30), 
            Size = new Size(250, 25), 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };

        var lblTeacher = new Label 
        { 
            Text = "Преподаватель:", 
            Location = new Point(30, 80), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        cmbTeacher = new ComboBox 
        { 
            Location = new Point(190, 80), 
            Size = new Size(250, 25), 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };

        var lblClassroom = new Label 
        { 
            Text = "Аудитория:", 
            Location = new Point(30, 130), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        cmbClassroom = new ComboBox 
        { 
            Location = new Point(190, 130), 
            Size = new Size(250, 25), 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };

        btnSave = new Button 
        { 
            Text = "Сохранить", 
            Location = new Point(70, 200), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnDelete = new Button 
        { 
            Text = "Удалить", 
            Location = new Point(200, 200), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(192, 0, 0),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += BtnDelete_Click;

        btnCancel = new Button 
        { 
            Text = "Отмена", 
            Location = new Point(330, 200), 
            Size = new Size(110, 35),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.AddRange(new Control[] { lblSubject, cmbSubject, lblTeacher, cmbTeacher, lblClassroom, cmbClassroom, btnSave, btnDelete, btnCancel });
    }

    private void LoadData()
    {
        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        // Загружаем все данные сразу, пока контекст активен
        var subjects = context.Subjects.ToList();
        var teachers = context.Teachers.ToList();
        var classrooms = context.Classrooms.ToList();
        
        var existingLesson = context.Lessons.FirstOrDefault(l =>
            l.GroupId == _groupId &&
            l.LessonNumber == _lessonNumber &&
            l.Date == _date);

        // Сохраняем ID для дальнейшего использования
        int? existingSubjectId = existingLesson?.SubjectId;
        int? existingTeacherId = existingLesson?.TeacherId;
        int? existingClassroomId = existingLesson?.ClassroomId;
        
        if (existingLesson != null)
        {
            _existingLesson = new Lesson
            {
                Id = existingLesson.Id,
                GroupId = existingLesson.GroupId,
                LessonNumber = existingLesson.LessonNumber,
                Date = existingLesson.Date,
                SubjectId = existingLesson.SubjectId,
                TeacherId = existingLesson.TeacherId,
                ClassroomId = existingLesson.ClassroomId
            };
        }

        // Теперь заполняем ComboBox'ы после закрытия контекста
        cmbSubject.DisplayMember = "Name";
        cmbSubject.ValueMember = "Id";
        cmbSubject.DataSource = subjects;

        cmbTeacher.DisplayMember = "ShortName";
        cmbTeacher.ValueMember = "Id";
        cmbTeacher.DataSource = teachers;

        cmbClassroom.DisplayMember = "Name";
        cmbClassroom.ValueMember = "Id";
        cmbClassroom.DataSource = classrooms;

        if (existingSubjectId.HasValue)
        {
            cmbSubject.SelectedValue = existingSubjectId.Value;
            cmbTeacher.SelectedValue = existingTeacherId!.Value;
            cmbClassroom.SelectedValue = existingClassroomId!.Value;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (cmbSubject.SelectedValue == null || cmbTeacher.SelectedValue == null || cmbClassroom.SelectedValue == null)
        {
            MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        int teacherId = (int)cmbTeacher.SelectedValue;
        int classroomId = (int)cmbClassroom.SelectedValue;

        var conflicts = context.Lessons.Where(l =>
            l.LessonNumber == _lessonNumber &&
            l.Date == _date &&
            (l.TeacherId == teacherId || l.ClassroomId == classroomId) &&
            (_existingLesson == null || l.Id != _existingLesson.Id)).ToList();

        if (conflicts.Any())
        {
            var result = MessageBox.Show("Обнаружен конфликт: преподаватель или аудитория заняты. Продолжить?",
                "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;
        }

        Lesson lesson;
        if (_existingLesson != null)
        {
            lesson = context.Lessons.Find(_existingLesson.Id)!;
        }
        else
        {
            lesson = new Lesson
            {
                GroupId = _groupId,
                LessonNumber = _lessonNumber,
                Date = _date
            };
            context.Lessons.Add(lesson);
        }

        lesson.SubjectId = (int)cmbSubject.SelectedValue;
        lesson.TeacherId = teacherId;
        lesson.ClassroomId = classroomId;

        context.SaveChanges();
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_existingLesson == null)
        {
            MessageBox.Show("Нет урока для удаления", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("Удалить урок?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
            return;

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        var lesson = context.Lessons.Find(_existingLesson.Id);
        if (lesson != null)
        {
            context.Lessons.Remove(lesson);
            context.SaveChanges();
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
