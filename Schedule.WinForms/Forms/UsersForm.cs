using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.WinForms.Helpers;
using Schedule.Core.Models;

namespace Schedule.WinForms.Forms;

public partial class UsersForm : Form
{
    private DataGridView dataGridView = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;

    public UsersForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Управление пользователями";
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

        pnlButtons.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
        this.Controls.AddRange(new Control[] { dataGridView, pnlButtons });
    }

    public void RefreshData()
    {
        LoadData();
    }

    private void LoadData()
    {
        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        var users = context.Users.Select(u => new { u.Id, u.Login, u.Role }).ToList();
        dataGridView.DataSource = users;
        RenameUserColumns();
    }

    private void RenameUserColumns()
    {
        if (dataGridView.Columns["Id"] != null)
            dataGridView.Columns["Id"]!.HeaderText = "ID";
        if (dataGridView.Columns["Login"] != null)
            dataGridView.Columns["Login"]!.HeaderText = "Логин";
        if (dataGridView.Columns["Role"] != null)
            dataGridView.Columns["Role"]!.HeaderText = "Роль";
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var form = new UserEditForm(null);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите пользователя для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedRow = dataGridView.SelectedRows[0];
        int id = (int)selectedRow.Cells["Id"].Value;

        var form = new UserEditForm(id);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите пользователя для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("Удалить выбранного пользователя?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
            return;

        var selectedRow = dataGridView.SelectedRows[0];
        int id = (int)selectedRow.Cells["Id"].Value;

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        var user = context.Users.Find(id);
        if (user != null)
        {
            if (user.Login == "admin")
            {
                MessageBox.Show("Нельзя удалить главного администратора", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            context.Users.Remove(user);
            context.SaveChanges();
            LoadData();
        }
    }
}
