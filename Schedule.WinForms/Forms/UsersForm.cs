using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
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

        dataGridView = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(840, 480),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            Font = new Font("Segoe UI", 10)
        };

        btnAdd = new Button 
        { 
            Text = "Добавить", 
            Location = new Point(20, 520), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Click += BtnAdd_Click;

        btnEdit = new Button 
        { 
            Text = "Изменить", 
            Location = new Point(150, 520), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnEdit.FlatAppearance.BorderSize = 0;
        btnEdit.Click += BtnEdit_Click;

        btnDelete = new Button 
        { 
            Text = "Удалить", 
            Location = new Point(280, 520), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(192, 0, 0),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += BtnDelete_Click;

        this.Controls.AddRange(new Control[] { dataGridView, btnAdd, btnEdit, btnDelete });
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
