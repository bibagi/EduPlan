using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Core.Data;
using System.Security.Cryptography;
using System.Text;

namespace Schedule.WinForms.Forms;

public partial class PasswordRecoveryForm : Form
{
    private TextBox txtEmailOrPhone = null!;
    private TextBox txtNewPassword = null!;
    private TextBox txtConfirmPassword = null!;
    private Button btnRecover = null!;
    private Label lblMessage = null!;
    private Label lblStep = null!;
    private Panel pnlStep1 = null!;
    private Panel pnlStep2 = null!;
    private string? _recoveryCode;
    private string? _userLogin;

    public PasswordRecoveryForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Восстановление пароля";
        this.Size = new Size(450, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "🔐 Восстановление пароля",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(50, 20),
            Size = new Size(350, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblStep = new Label
        {
            Text = "Шаг 1: Введите email или телефон",
            Font = new Font("Segoe UI", 10),
            Location = new Point(50, 60),
            Size = new Size(350, 25),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Панель шага 1
        pnlStep1 = new Panel
        {
            Location = new Point(50, 100),
            Size = new Size(350, 150),
            Visible = true
        };

        var lblEmailOrPhone = new Label
        {
            Text = "Email или телефон:",
            Location = new Point(0, 10),
            Size = new Size(150, 20),
            Font = new Font("Segoe UI", 9)
        };

        txtEmailOrPhone = new TextBox
        {
            Location = new Point(0, 35),
            Size = new Size(350, 25),
            Font = new Font("Segoe UI", 10),
            PlaceholderText = "example@mail.ru или +79991234567"
        };

        var btnSendCode = new Button
        {
            Text = "Отправить код",
            Location = new Point(100, 80),
            Size = new Size(150, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSendCode.FlatAppearance.BorderSize = 0;
        btnSendCode.Click += async (s, e) => await OnSendCodeClickAsync();

        pnlStep1.Controls.AddRange(new Control[] { lblEmailOrPhone, txtEmailOrPhone, btnSendCode });

        // Панель шага 2
        pnlStep2 = new Panel
        {
            Location = new Point(50, 100),
            Size = new Size(350, 200),
            Visible = false
        };

        var lblNewPassword = new Label
        {
            Text = "Новый пароль:",
            Location = new Point(0, 10),
            Size = new Size(150, 20),
            Font = new Font("Segoe UI", 9)
        };

        txtNewPassword = new TextBox
        {
            Location = new Point(0, 35),
            Size = new Size(350, 25),
            PasswordChar = '●',
            Font = new Font("Segoe UI", 10)
        };

        var lblConfirmPassword = new Label
        {
            Text = "Подтвердите пароль:",
            Location = new Point(0, 70),
            Size = new Size(150, 20),
            Font = new Font("Segoe UI", 9)
        };

        txtConfirmPassword = new TextBox
        {
            Location = new Point(0, 95),
            Size = new Size(350, 25),
            PasswordChar = '●',
            Font = new Font("Segoe UI", 10)
        };

        btnRecover = new Button
        {
            Text = "Сменить пароль",
            Location = new Point(100, 140),
            Size = new Size(150, 35),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnRecover.FlatAppearance.BorderSize = 0;
        btnRecover.Click += async (s, e) => await OnRecoverClickAsync();

        pnlStep2.Controls.AddRange(new Control[] { lblNewPassword, txtNewPassword, lblConfirmPassword, txtConfirmPassword, btnRecover });

        lblMessage = new Label
        {
            Location = new Point(50, 310),
            Size = new Size(350, 40),
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnCancel = new Button
        {
            Text = "Отмена",
            Location = new Point(175, 320),
            Size = new Size(100, 30),
            Font = new Font("Segoe UI", 9),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => this.Close();

        this.Controls.AddRange(new Control[] { lblTitle, lblStep, pnlStep1, pnlStep2, lblMessage, btnCancel });
    }

    private async Task OnSendCodeClickAsync()
    {
        var input = txtEmailOrPhone.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            ShowMessage("Введите email или телефон", Color.Red);
            return;
        }

        using var scope = Program.ServiceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == input || u.Phone == input);

        if (user == null)
        {
            ShowMessage("Пользователь с таким email или телефоном не найден", Color.Red);
            return;
        }

        // Проверка: admin не может восстанавливать пароль через эту форму
        if (user.Login.ToLower() == "admin")
        {
            MessageBox.Show("Восстановление пароля для администратора недоступно.\n\n" +
                "Для сброса пароля администратора:\n" +
                "1. Закройте приложение\n" +
                "2. Удалите файл schedule.db\n" +
                "3. Запустите приложение заново\n" +
                "4. Войдите как admin/admin",
                "Администратор", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Генерируем код восстановления (в реальном приложении отправляем на email/SMS)
        _recoveryCode = new Random().Next(100000, 999999).ToString();
        _userLogin = user.Login;

        // В реальном приложении здесь отправка кода
        MessageBox.Show($"Код восстановления: {_recoveryCode}\n\n(В реальном приложении код будет отправлен на {input})",
            "Код восстановления", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Переходим к шагу 2
        pnlStep1.Visible = false;
        pnlStep2.Visible = true;
        lblStep.Text = "Шаг 2: Введите новый пароль";
        ShowMessage($"Код отправлен на {input}", Color.Green);
    }

    private async Task OnRecoverClickAsync()
    {
        if (string.IsNullOrEmpty(txtNewPassword.Text))
        {
            ShowMessage("Введите новый пароль", Color.Red);
            return;
        }

        if (txtNewPassword.Text != txtConfirmPassword.Text)
        {
            ShowMessage("Пароли не совпадают", Color.Red);
            return;
        }

        if (txtNewPassword.Text.Length < 4)
        {
            ShowMessage("Пароль должен содержать минимум 4 символа", Color.Red);
            return;
        }

        try
        {
            using var scope = Program.ServiceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

            var user = await context.Users.FirstOrDefaultAsync(u => u.Login == _userLogin);
            if (user == null)
            {
                ShowMessage("Ошибка: пользователь не найден", Color.Red);
                return;
            }

            // Хешируем новый пароль
            user.PasswordHash = HashPassword(txtNewPassword.Text);
            await context.SaveChangesAsync();

            MessageBox.Show("Пароль успешно изменён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", Color.Red);
        }
    }

    private void ShowMessage(string message, Color color)
    {
        lblMessage.Text = message;
        lblMessage.ForeColor = color;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
