using Schedule.Core.ViewModels;

namespace Schedule.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly LoginViewModel _viewModel;
    private TextBox txtLogin = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Label lblError = null!;
    private CheckBox chkRememberMe = null!;
    private const string SettingsFile = "login_settings.json";

    public LoginForm(LoginViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        SetupBindings();
        LoadSavedCredentials();
    }

    private void InitializeComponent()
    {
        this.Text = "EduPlan - Вход в систему";
        this.Size = new Size(450, 550);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = Color.White;
        
        // Загружаем иконку
        try
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }
        }
        catch
        {
            // Игнорируем ошибки загрузки иконки
        }

        // Заголовок приложения
        var lblTitle = new Label
        {
            Text = "EduPlan",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            Location = new Point(50, 60),
            Size = new Size(350, 50),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(0, 120, 212)
        };

        var lblSubtitle = new Label
        {
            Text = "Система управления расписанием",
            Font = new Font("Segoe UI", 11),
            Location = new Point(50, 115),
            Size = new Size(350, 25),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(96, 96, 96)
        };

        // Разделительная линия
        var separator = new Panel
        {
            Location = new Point(75, 160),
            Size = new Size(300, 1),
            BackColor = Color.FromArgb(229, 229, 229)
        };

        var lblLogin = new Label
        {
            Text = "Логин",
            Location = new Point(75, 190),
            Size = new Size(300, 22),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        txtLogin = new TextBox
        {
            Location = new Point(75, 217),
            Size = new Size(300, 35),
            Font = new Font("Segoe UI", 11),
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Только английские буквы"
        };
        txtLogin.KeyPress += TxtLogin_KeyPress;

        var lblPassword = new Label
        {
            Text = "Пароль",
            Location = new Point(75, 270),
            Size = new Size(300, 22),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        txtPassword = new TextBox
        {
            Location = new Point(75, 297),
            Size = new Size(300, 35),
            PasswordChar = '●',
            Font = new Font("Segoe UI", 11),
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Только английские буквы"
        };
        txtPassword.KeyPress += TxtPassword_KeyPress;

        chkRememberMe = new CheckBox
        {
            Text = "Запомнить меня",
            Location = new Point(75, 345),
            Size = new Size(300, 22),
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(32, 32, 32),
            Checked = false
        };

        lblError = new Label
        {
            Location = new Point(75, 375),
            Size = new Size(300, 22),
            ForeColor = Color.FromArgb(196, 43, 28),
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleLeft
        };

        btnLogin = new Button
        {
            Text = "Войти",
            Location = new Point(75, 410),
            Size = new Size(300, 42),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += async (s, e) => await OnLoginClickAsync();

        var lnkForgotPassword = new LinkLabel
        {
            Text = "Забыли пароль?",
            Location = new Point(75, 465),
            Size = new Size(300, 22),
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleCenter,
            LinkColor = Color.FromArgb(0, 120, 212),
            ActiveLinkColor = Color.FromArgb(0, 90, 158)
        };
        lnkForgotPassword.LinkBehavior = LinkBehavior.HoverUnderline;
        lnkForgotPassword.Click += (s, e) => OnForgotPasswordClick();

        this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, separator, lblLogin, txtLogin, lblPassword, txtPassword, chkRememberMe, lblError, btnLogin, lnkForgotPassword });
        this.AcceptButton = btnLogin;
    }

    private void SetupBindings()
    {
        txtLogin.DataBindings.Add(nameof(txtLogin.Text), _viewModel, nameof(_viewModel.Login), false, DataSourceUpdateMode.OnPropertyChanged);
        txtPassword.DataBindings.Add(nameof(txtPassword.Text), _viewModel, nameof(_viewModel.Password), false, DataSourceUpdateMode.OnPropertyChanged);
        lblError.DataBindings.Add(nameof(lblError.Text), _viewModel, nameof(_viewModel.ErrorMessage), false, DataSourceUpdateMode.OnPropertyChanged);
    }

    private async Task OnLoginClickAsync()
    {
        btnLogin.Enabled = false;
        lblError.Text = "";
        
        await _viewModel.LoginCommand.ExecuteAsync(null);
        
        if (_viewModel.AuthenticatedUser != null)
        {
            // Сохраняем учётные данные если выбрано "Запомнить меня"
            SaveCredentials();
            
            var mainForm = Program.ServiceProvider.GetService(typeof(MainForm)) as MainForm;
            if (mainForm != null)
            {
                await mainForm.InitializeAsync(_viewModel.AuthenticatedUser);
                this.Hide();
                mainForm.ShowDialog();
                this.Close();
            }
        }
        
        btnLogin.Enabled = true;
    }

    private void OnForgotPasswordClick()
    {
        var recoveryForm = new PasswordRecoveryForm();
        recoveryForm.ShowDialog();
    }

    private void TxtLogin_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Разрешаем только английские буквы, цифры, backspace и некоторые спецсимволы
        if (!char.IsControl(e.KeyChar) && !IsEnglishChar(e.KeyChar))
        {
            e.Handled = true;
            lblError.Text = "Только английские символы!";
        }
        else if (char.IsControl(e.KeyChar))
        {
            lblError.Text = "";
        }
    }

    private void TxtPassword_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Разрешаем только английские буквы, цифры, backspace и некоторые спецсимволы
        if (!char.IsControl(e.KeyChar) && !IsEnglishChar(e.KeyChar))
        {
            e.Handled = true;
            lblError.Text = "Только английские символы!";
        }
        else if (char.IsControl(e.KeyChar))
        {
            lblError.Text = "";
        }
    }

    private bool IsEnglishChar(char c)
    {
        // Английские буквы, цифры и некоторые спецсимволы
        return (c >= 'a' && c <= 'z') || 
               (c >= 'A' && c <= 'Z') || 
               (c >= '0' && c <= '9') || 
               c == '_' || c == '-' || c == '.' || c == '@';
    }

    private void LoadSavedCredentials()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = System.Text.Json.JsonSerializer.Deserialize<LoginSettings>(json);
                if (settings != null && settings.RememberMe)
                {
                    txtLogin.Text = settings.Login ?? "";
                    chkRememberMe.Checked = true;
                }
            }
        }
        catch
        {
            // Игнорируем ошибки загрузки
        }
    }

    private void SaveCredentials()
    {
        try
        {
            var settings = new LoginSettings
            {
                Login = chkRememberMe.Checked ? txtLogin.Text : "",
                RememberMe = chkRememberMe.Checked
            };
            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            File.WriteAllText(SettingsFile, json);
        }
        catch
        {
            // Игнорируем ошибки сохранения
        }
    }

    private class LoginSettings
    {
        public string? Login { get; set; }
        public bool RememberMe { get; set; }
    }
}
