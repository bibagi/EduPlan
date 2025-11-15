using Schedule.Core.Models;
using Schedule.Core.ViewModels;

namespace Schedule.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly MainViewModel _viewModel;
    private Panel tabMain = null!;
    private Label lblWelcome = null!;
    private User? _currentUser;

    public MainForm(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        SetupBindings();
    }

    public async Task InitializeAsync(User user)
    {
        _currentUser = user;
        await _viewModel.InitializeAsync(user);
        lblWelcome.Text = user.Login;
        _lblRole.Text = user.Role == "Admin" ? "Администратор" : user.Role == "Teacher" ? "Преподаватель" : "Просмотр";
        
        // Создаём меню после получения пользователя
        CreateTabs();
    }

    private void InitializeComponent()
    {
        this.Text = "Система управления расписанием";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(243, 243, 243);
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MinimumSize = new Size(1000, 600);
        this.WindowState = FormWindowState.Maximized;



        // Боковое меню (Navigation)
        var pnlNav = new Panel
        {
            Dock = DockStyle.Left,
            Width = 280,
            BackColor = Color.FromArgb(249, 249, 249),
            Padding = new Padding(0, 10, 0, 0)
        };

        // Профиль пользователя
        var pnlProfile = new Panel
        {
            Location = new Point(0, 10),
            Size = new Size(280, 80),
            BackColor = Color.Transparent
        };

        var picProfile = new PictureBox
        {
            Location = new Point(20, 15),
            Size = new Size(50, 50),
            BackColor = Color.FromArgb(0, 120, 212),
            SizeMode = PictureBoxSizeMode.CenterImage
        };

        lblWelcome = new Label
        {
            Location = new Point(80, 20),
            Size = new Size(180, 20),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(32, 32, 32),
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        var lblRole = new Label
        {
            Location = new Point(80, 42),
            Size = new Size(180, 18),
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(96, 96, 96),
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        pnlProfile.Controls.AddRange(new Control[] { picProfile, lblWelcome, lblRole });
        pnlNav.Controls.Add(pnlProfile);

        // Панель для кнопок меню
        var pnlMenuButtons = new Panel
        {
            Location = new Point(0, 100),
            Size = new Size(280, pnlNav.Height - 100),
            BackColor = Color.Transparent,
            AutoScroll = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
        };

        pnlNav.Controls.Add(pnlMenuButtons);

        // Основная панель контента
        var pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            AutoScroll = false
        };

        // Внутренняя панель с отступами
        var pnlInner = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20, 0, 20, 0),
            AutoScroll = false
        };

        // Заголовок страницы
        var lblPageTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 60,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.FromArgb(32, 32, 32),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(0, 0, 0, 10)
        };

        // Панель для контента страницы
        tabMain = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            AutoScroll = true,
            Padding = new Padding(0, 10, 0, 0)
        };

        pnlInner.Controls.AddRange(new Control[] { tabMain, lblPageTitle });
        pnlContent.Controls.Add(pnlInner);

        this.Controls.Add(pnlContent);
        this.Controls.Add(pnlNav);

        // Сохраняем ссылки для дальнейшего использования
        _pnlMenuButtons = pnlMenuButtons;
        _lblPageTitle = lblPageTitle;
        _lblRole = lblRole;

    }

    private Panel _pnlMenuButtons = null!;
    private Label _lblPageTitle = null!;
    private Label _lblRole = null!;

    private Button CreateMenuButton(string text, string icon, int yPosition)
    {
        var btn = new Button
        {
            Text = $"  {icon}  {text}",
            Location = new Point(8, yPosition),
            Size = new Size(264, 44),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(32, 32, 32),
            Cursor = Cursors.Hand,
            UseCompatibleTextRendering = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(243, 243, 243);
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(237, 237, 237);
        
        return btn;
    }

    private void SelectMenuButton(Button selectedButton)
    {
        foreach (Control ctrl in _pnlMenuButtons.Controls)
        {
            if (ctrl is Button btn)
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(32, 32, 32);
            }
        }
        
        selectedButton.BackColor = Color.FromArgb(243, 243, 243);
        selectedButton.ForeColor = Color.FromArgb(0, 120, 212);
    }

    private void CreateTabs()
    {
        if (_currentUser == null) return;

        _pnlMenuButtons.Controls.Clear();
        tabMain.Controls.Clear();

        int yPos = 0;

        // Кнопка: Расписание на неделю
        var btnWeek = CreateMenuButton("Расписание на неделю", "📅", yPos);
        btnWeek.Click += (s, e) =>
        {
            SelectMenuButton(btnWeek);
            _lblPageTitle.Text = "Расписание на неделю";
            ShowContent(new WeekScheduleControl(_currentUser));
        };
        _pnlMenuButtons.Controls.Add(btnWeek);
        yPos += 48;

        // Кнопка: Редактор расписания
        var btnEditor = CreateMenuButton("Редактор расписания", "✏️", yPos);
        btnEditor.Click += (s, e) =>
        {
            SelectMenuButton(btnEditor);
            _lblPageTitle.Text = "Редактор расписания";
            ShowContent(new ScheduleEditorControl(_currentUser));
        };
        _pnlMenuButtons.Controls.Add(btnEditor);
        yPos += 48;

        // Кнопка: Просмотр расписания
        var btnViewer = CreateMenuButton("Просмотр расписания", "👁️", yPos);
        btnViewer.Click += (s, e) =>
        {
            SelectMenuButton(btnViewer);
            _lblPageTitle.Text = "Просмотр расписания";
            ShowContent(new ScheduleViewerControl(_currentUser));
        };
        _pnlMenuButtons.Controls.Add(btnViewer);
        yPos += 48;

        // Разделитель
        if (_currentUser.Role == "Admin")
        {
            var separator = new Label
            {
                Location = new Point(20, yPos + 10),
                Size = new Size(240, 1),
                BackColor = Color.FromArgb(229, 229, 229)
            };
            _pnlMenuButtons.Controls.Add(separator);
            yPos += 30;

            // Кнопка: Справочники
            var btnDirectory = CreateMenuButton("Справочники", "📚", yPos);
            btnDirectory.Click += (s, e) =>
            {
                SelectMenuButton(btnDirectory);
                _lblPageTitle.Text = "Справочники";
                ShowContent(new DirectoryControl());
            };
            _pnlMenuButtons.Controls.Add(btnDirectory);
            yPos += 48;

            // Кнопка: Пользователи
            var btnUsers = CreateMenuButton("Пользователи", "👥", yPos);
            btnUsers.Click += (s, e) =>
            {
                SelectMenuButton(btnUsers);
                _lblPageTitle.Text = "Пользователи";
                ShowContent(new UsersControl());
            };
            _pnlMenuButtons.Controls.Add(btnUsers);
            yPos += 48;
        }

        // Разделитель
        var separator2 = new Label
        {
            Location = new Point(20, yPos + 10),
            Size = new Size(240, 1),
            BackColor = Color.FromArgb(229, 229, 229)
        };
        _pnlMenuButtons.Controls.Add(separator2);
        yPos += 30;

        // Кнопка: Настройки
        var btnSettings = CreateMenuButton("Настройки", "⚙️", yPos);
        btnSettings.Click += (s, e) =>
        {
            SelectMenuButton(btnSettings);
            _lblPageTitle.Text = "Настройки";
            ShowContent(CreateSettingsPanel());
        };
        _pnlMenuButtons.Controls.Add(btnSettings);
        yPos += 48;

        // Кнопка: Выход
        var btnExit = CreateMenuButton("Выход", "🚪", yPos);
        btnExit.Click += (s, e) => Application.Exit();
        _pnlMenuButtons.Controls.Add(btnExit);

        // Показываем первую страницу по умолчанию
        SelectMenuButton(btnWeek);
        _lblPageTitle.Text = "Расписание на неделю";
        ShowContent(new WeekScheduleControl(_currentUser));
    }

    private void ShowContent(Control content)
    {
        tabMain.Controls.Clear();
        content.Dock = DockStyle.Fill;
        tabMain.Controls.Add(content);
    }

    private Panel CreateSettingsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        var lblAbout = new Label
        {
            Text = "О приложении",
            Location = new Point(0, 20),
            Size = new Size(600, 28),
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        var lblVersion = new Label
        {
            Text = "Система управления расписанием",
            Location = new Point(0, 60),
            Size = new Size(600, 24),
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.FromArgb(96, 96, 96)
        };

        var lblVersionNum = new Label
        {
            Text = "Версия 1.0.0",
            Location = new Point(0, 90),
            Size = new Size(600, 20),
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(96, 96, 96)
        };

        // Карточка с информацией
        var pnlCard = new Panel
        {
            Location = new Point(0, 140),
            Size = new Size(600, 200),
            BackColor = Color.FromArgb(249, 249, 249),
            Padding = new Padding(20)
        };

        var lblUser = new Label
        {
            Text = $"Пользователь: {_currentUser?.Login}",
            Location = new Point(20, 20),
            Size = new Size(560, 24),
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        var lblUserRole = new Label
        {
            Text = $"Роль: {_currentUser?.Role}",
            Location = new Point(20, 50),
            Size = new Size(560, 24),
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        var lblEmail = new Label
        {
            Text = $"Email: {_currentUser?.Email ?? "Не указан"}",
            Location = new Point(20, 80),
            Size = new Size(560, 24),
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        var lblPhone = new Label
        {
            Text = $"Телефон: {_currentUser?.Phone ?? "Не указан"}",
            Location = new Point(20, 110),
            Size = new Size(560, 24),
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(32, 32, 32)
        };

        pnlCard.Controls.AddRange(new Control[] { lblUser, lblUserRole, lblEmail, lblPhone });

        // Кнопка выхода
        var btnLogout = new Button
        {
            Text = "Выйти из аккаунта",
            Location = new Point(0, 360),
            Size = new Size(180, 40),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (s, e) =>
        {
            Application.Restart();
        };

        panel.Controls.AddRange(new Control[] { lblAbout, lblVersion, lblVersionNum, pnlCard, btnLogout });
        return panel;
    }

    private void SetupBindings()
    {
        // Убрали привязки к теме, так как используем только светлую тему
    }

}

// UserControl для расписания на неделю
public class WeekScheduleControl : UserControl
{
    private readonly User _currentUser;
    private WeekScheduleForm? _form;

    public WeekScheduleControl(User user)
    {
        _currentUser = user;
        this.Dock = DockStyle.Fill;
        this.Load += (s, e) => LoadForm();
    }

    private void LoadForm()
    {
        if (_form == null)
        {
            _form = new WeekScheduleForm(_currentUser);
            _form.TopLevel = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Dock = DockStyle.Fill;
            this.Controls.Add(_form);
            _form.Show();
        }
    }

    public void RefreshData()
    {
        _form?.RefreshSchedule();
    }
}

// UserControl для редактора расписания
public class ScheduleEditorControl : UserControl
{
    private readonly User _currentUser;
    private ScheduleEditorForm? _form;

    public ScheduleEditorControl(User user)
    {
        _currentUser = user;
        this.Dock = DockStyle.Fill;
        this.Load += (s, e) => LoadForm();
    }

    private void LoadForm()
    {
        if (_form == null)
        {
            _form = new ScheduleEditorForm(_currentUser);
            _form.TopLevel = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Dock = DockStyle.Fill;
            this.Controls.Add(_form);
            _form.Show();
        }
    }

    public void RefreshData()
    {
        _form?.RefreshSchedule();
    }
}

// UserControl для просмотра расписания
public class ScheduleViewerControl : UserControl
{
    private readonly User _currentUser;
    private ScheduleViewerForm? _form;

    public ScheduleViewerControl(User user)
    {
        _currentUser = user;
        this.Dock = DockStyle.Fill;
        this.Load += (s, e) => LoadForm();
    }

    private void LoadForm()
    {
        if (_form == null)
        {
            _form = new ScheduleViewerForm(_currentUser);
            _form.TopLevel = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Dock = DockStyle.Fill;
            this.Controls.Add(_form);
            _form.Show();
        }
    }

    public void RefreshData()
    {
        _form?.RefreshSchedule();
    }
}

// UserControl для справочников
public class DirectoryControl : UserControl
{
    private TabControl tabDirectory = null!;
    private Dictionary<string, DirectoryForm> _forms = new Dictionary<string, DirectoryForm>();

    public DirectoryControl()
    {
        this.Dock = DockStyle.Fill;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        tabDirectory = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10)
        };

        string[] entities = { "Teachers", "Classrooms", "Subjects", "Groups" };
        string[] titles = { "👨‍🏫 Учителя", "🏫 Аудитории", "📖 Предметы", "👥 Группы" };

        for (int i = 0; i < entities.Length; i++)
        {
            var tab = new TabPage(titles[i]);
            var form = new DirectoryForm(entities[i]);
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            tab.Controls.Add(form);
            form.Show();
            tabDirectory.TabPages.Add(tab);
            _forms[entities[i]] = form;
        }

        this.Controls.Add(tabDirectory);
    }

    public void RefreshData()
    {
        foreach (var form in _forms.Values)
        {
            form.RefreshData();
        }
    }
}

// UserControl для пользователей
public class UsersControl : UserControl
{
    private UsersForm? _form;

    public UsersControl()
    {
        this.Dock = DockStyle.Fill;
        this.Load += (s, e) => LoadForm();
    }

    private void LoadForm()
    {
        if (_form == null)
        {
            _form = new UsersForm();
            _form.TopLevel = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Dock = DockStyle.Fill;
            this.Controls.Add(_form);
            _form.Show();
        }
    }

    public void RefreshData()
    {
        _form?.RefreshData();
    }
}
