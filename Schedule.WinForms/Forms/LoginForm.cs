using Schedule.Core.ViewModels;

namespace Schedule.WinForms.Forms;

public partial class LoginForm : Form
{
    private readonly LoginViewModel _viewModel;
    private TextBox txtLogin = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Label lblError = null!;

    public LoginForm(LoginViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        SetupBindings();
    }

    private void InitializeComponent()
    {
        this.Text = "Вход в систему";
        this.Size = new Size(400, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "Система управления расписанием",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(50, 30),
            Size = new Size(300, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblLogin = new Label
        {
            Text = "Логин:",
            Location = new Point(50, 90),
            Size = new Size(80, 20),
            Font = new Font("Segoe UI", 10)
        };

        txtLogin = new TextBox
        {
            Location = new Point(140, 90),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10)
        };

        var lblPassword = new Label
        {
            Text = "Пароль:",
            Location = new Point(50, 130),
            Size = new Size(80, 20),
            Font = new Font("Segoe UI", 10)
        };

        txtPassword = new TextBox
        {
            Location = new Point(140, 130),
            Size = new Size(200, 25),
            PasswordChar = '●',
            Font = new Font("Segoe UI", 10)
        };

        lblError = new Label
        {
            Location = new Point(50, 170),
            Size = new Size(300, 20),
            ForeColor = Color.Red,
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleCenter
        };

        btnLogin = new Button
        {
            Text = "Войти",
            Location = new Point(140, 200),
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += async (s, e) => await OnLoginClickAsync();

        var lnkForgotPassword = new LinkLabel
        {
            Text = "Забыли пароль?",
            Location = new Point(140, 240),
            Size = new Size(120, 20),
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleCenter,
            LinkColor = Color.FromArgb(0, 120, 215)
        };
        lnkForgotPassword.Click += (s, e) => OnForgotPasswordClick();

        this.Controls.AddRange(new Control[] { lblTitle, lblLogin, txtLogin, lblPassword, txtPassword, lblError, btnLogin, lnkForgotPassword });
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
        await _viewModel.LoginCommand.ExecuteAsync(null);
        
        if (_viewModel.AuthenticatedUser != null)
        {
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
}
