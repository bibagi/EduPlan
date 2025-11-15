using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using Schedule.Core.Models;

namespace Schedule.WinForms.Forms;

public partial class UserEditForm : Form
{
    private readonly int? _userId;
    private TextBox txtLogin = null!;
    private TextBox txtPassword = null!;
    private TextBox txtEmail = null!;
    private TextBox txtPhone = null!;
    private ComboBox cmbRole = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    public UserEditForm(int? userId)
    {
        _userId = userId;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = _userId.HasValue ? "Редактирование пользователя" : "Добавление пользователя";
        this.Size = new Size(500, 420);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblLogin = new Label 
        { 
            Text = "Логин:", 
            Location = new Point(30, 30), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        txtLogin = new TextBox 
        { 
            Location = new Point(190, 30), 
            Size = new Size(250, 25),
            Font = new Font("Segoe UI", 10)
        };

        var lblPassword = new Label 
        { 
            Text = "Пароль:", 
            Location = new Point(30, 70), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        txtPassword = new TextBox 
        { 
            Location = new Point(190, 70), 
            Size = new Size(250, 25), 
            PasswordChar = '●',
            Font = new Font("Segoe UI", 10)
        };

        var lblEmail = new Label 
        { 
            Text = "Email:", 
            Location = new Point(30, 110), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        txtEmail = new TextBox 
        { 
            Location = new Point(190, 110), 
            Size = new Size(250, 25),
            Font = new Font("Segoe UI", 10),
            PlaceholderText = "example@mail.ru"
        };

        var lblPhone = new Label 
        { 
            Text = "Телефон:", 
            Location = new Point(30, 150), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        txtPhone = new TextBox 
        { 
            Location = new Point(190, 150), 
            Size = new Size(250, 25),
            Font = new Font("Segoe UI", 10),
            PlaceholderText = "+79991234567"
        };

        var lblRole = new Label 
        { 
            Text = "Роль:", 
            Location = new Point(30, 190), 
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10)
        };
        cmbRole = new ComboBox 
        { 
            Location = new Point(190, 190), 
            Size = new Size(250, 25), 
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        cmbRole.Items.AddRange(new object[] { "Admin", "Teacher", "Viewer" });
        cmbRole.SelectedIndex = 0;

        var lblNote = new Label
        {
            Text = "* Email и телефон нужны для восстановления пароля\n  (для admin не обязательны)",
            Location = new Point(30, 230),
            Size = new Size(410, 40),
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.Gray
        };

        btnSave = new Button 
        { 
            Text = "Сохранить", 
            Location = new Point(190, 280), 
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
            Location = new Point(320, 280), 
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.AddRange(new Control[] { lblLogin, txtLogin, lblPassword, txtPassword, lblEmail, txtEmail, lblPhone, txtPhone, lblRole, cmbRole, lblNote, btnSave, btnCancel });
    }

    private void LoadData()
    {
        if (!_userId.HasValue)
            return;

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        var user = context.Users.Find(_userId.Value);
        if (user != null)
        {
            txtLogin.Text = user.Login;
            txtLogin.ReadOnly = true;
            txtEmail.Text = user.Email ?? "";
            txtPhone.Text = user.Phone ?? "";
            cmbRole.SelectedItem = user.Role;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Заполните логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var scope = Program.ServiceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

            if (_userId.HasValue)
            {
                var user = context.Users.Find(_userId.Value);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
                    user.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                    user.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
                    user.Role = cmbRole.SelectedItem?.ToString() ?? "Viewer";
                    context.SaveChanges();
                }
            }
            else
            {
                if (context.Users.Any(u => u.Login == txtLogin.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var user = new User
                {
                    Login = txtLogin.Text,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Role = cmbRole.SelectedItem?.ToString() ?? "Viewer"
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
