using Schedule.Core.Models;
using Schedule.Core.ViewModels;

namespace Schedule.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly MainViewModel _viewModel;
    private TabControl tabMain = null!;
    private Label lblWelcome = null!;
    private Panel pnlUpdate = null!;
    private Panel pnlTop = null!;
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
        lblWelcome.Text = $"Добро пожаловать, {user.Login} ({user.Role})";
        
        // Создаём вкладки после получения пользователя
        CreateTabs();
        
        // Настройка видимости по ролям после инициализации
        if (user.Role != "Admin")
        {
            // Скрываем вкладки для не-админов
            var directoryTab = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Text.Contains("Справочники"));
            var usersTab = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Text.Contains("Пользователи"));
            if (directoryTab != null) tabMain.TabPages.Remove(directoryTab);
            if (usersTab != null) tabMain.TabPages.Remove(usersTab);
        }
        
        ApplyTheme();
    }

    private void InitializeComponent()
    {
        this.Text = "Система управления расписанием";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;

        // Панель приветствия
        pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.FromArgb(240, 240, 240)
        };

        lblWelcome = new Label
        {
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(20, 15),
            Size = new Size(800, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };

        pnlUpdate = new Panel
        {
            Location = new Point(20, 50),
            Size = new Size(1000, 25),
            BackColor = Color.LightYellow,
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var lblUpdate = new Label
        {
            Text = "Доступно обновление!",
            Location = new Point(10, 3),
            Size = new Size(700, 20),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var btnUpdate = new Button
        {
            Text = "Обновить",
            Location = new Point(850, 0),
            Size = new Size(100, 25),
            BackColor = Color.Green,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnUpdate.FlatAppearance.BorderSize = 0;
        btnUpdate.Click += async (s, e) => await OnUpdateClickAsync();

        pnlUpdate.Controls.AddRange(new Control[] { lblUpdate, btnUpdate });
        pnlTop.Controls.AddRange(new Control[] { lblWelcome, pnlUpdate });
        
        // Обработка изменения размера для адаптивности
        this.Resize += (s, e) =>
        {
            if (pnlUpdate != null && lblUpdate != null && btnUpdate != null)
            {
                pnlUpdate.Width = this.ClientSize.Width - 40;
                lblUpdate.Width = pnlUpdate.Width - 120;
                btnUpdate.Left = pnlUpdate.Width - 110;
            }
        };

        // TabControl для всех разделов
        tabMain = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10)
        };
        
        // Обработчик переключения вкладок для автообновления
        tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

        this.Controls.Add(tabMain);
        this.Controls.Add(pnlTop);
    }

    private void CreateTabs()
    {
        if (_currentUser == null) return;

        tabMain.TabPages.Clear();

        // Вкладка: Расписание на неделю
        var tabWeek = new TabPage("📅 Расписание на неделю");
        var weekScheduleControl = new WeekScheduleControl(_currentUser);
        weekScheduleControl.Dock = DockStyle.Fill;
        tabWeek.Controls.Add(weekScheduleControl);

        // Вкладка: Редактор расписания
        var tabEditor = new TabPage("✏️ Редактор расписания");
        var scheduleEditorControl = new ScheduleEditorControl(_currentUser);
        scheduleEditorControl.Dock = DockStyle.Fill;
        tabEditor.Controls.Add(scheduleEditorControl);

        // Вкладка: Просмотр расписания
        var tabViewer = new TabPage("👁️ Просмотр расписания");
        var scheduleViewerControl = new ScheduleViewerControl(_currentUser);
        scheduleViewerControl.Dock = DockStyle.Fill;
        tabViewer.Controls.Add(scheduleViewerControl);

        // Вкладка: Справочники (только для админов)
        var tabDirectory = new TabPage("📚 Справочники");
        var directoryControl = new DirectoryControl();
        directoryControl.Dock = DockStyle.Fill;
        tabDirectory.Controls.Add(directoryControl);

        // Вкладка: Пользователи (только для админов)
        var tabUsers = new TabPage("👥 Пользователи");
        var usersControl = new UsersControl();
        usersControl.Dock = DockStyle.Fill;
        tabUsers.Controls.Add(usersControl);

        // Вкладка: Настройки
        var tabSettings = new TabPage("⚙️ Настройки");
        var settingsPanel = CreateSettingsPanel();
        tabSettings.Controls.Add(settingsPanel);

        tabMain.TabPages.AddRange(new TabPage[] { tabWeek, tabEditor, tabViewer, tabDirectory, tabUsers, tabSettings });
    }

    private Panel CreateSettingsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var lblTheme = new Label
        {
            Text = "Тема оформления:",
            Location = new Point(20, 20),
            Size = new Size(200, 30),
            Font = new Font("Segoe UI", 11)
        };

        var btnLight = new Button
        {
            Text = "☀️ Светлая тема",
            Location = new Point(20, 60),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnLight.Click += async (s, e) => await _viewModel.ChangeThemeCommand.ExecuteAsync("Light");

        var btnDark = new Button
        {
            Text = "🌙 Тёмная тема",
            Location = new Point(180, 60),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDark.Click += async (s, e) => await _viewModel.ChangeThemeCommand.ExecuteAsync("Dark");

        var btnExit = new Button
        {
            Text = "🚪 Выход",
            Location = new Point(20, 120),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnExit.FlatAppearance.BorderSize = 0;
        btnExit.Click += (s, e) => Application.Exit();

        panel.Controls.AddRange(new Control[] { lblTheme, btnLight, btnDark, btnExit });
        return panel;
    }

    private void SetupBindings()
    {
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.HasUpdate))
            {
                pnlUpdate.Visible = _viewModel.HasUpdate;
            }
            else if (e.PropertyName == nameof(_viewModel.Theme))
            {
                ApplyTheme();
            }
        };
    }

    private void ApplyTheme()
    {
        if (_viewModel.Theme == "Dark")
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            lblWelcome.ForeColor = Color.White;
            tabMain.BackColor = Color.FromArgb(45, 45, 45);
        }
        else
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            lblWelcome.ForeColor = Color.Black;
            tabMain.BackColor = Color.White;
        }
    }

    private async Task OnUpdateClickAsync()
    {
        var result = MessageBox.Show("Применить обновление? Приложение будет перезапущено.", 
            "Обновление", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            await _viewModel.ApplyUpdateCommand.ExecuteAsync(null);
            MessageBox.Show("Обновление применено. Перезапустите приложение.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }
    }

    private void TabMain_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tabMain.SelectedTab == null) return;

        // Обновляем данные при переключении на вкладку с расписанием
        foreach (Control control in tabMain.SelectedTab.Controls)
        {
            if (control is WeekScheduleControl weekControl)
            {
                weekControl.RefreshData();
            }
            else if (control is ScheduleEditorControl editorControl)
            {
                editorControl.RefreshData();
            }
            else if (control is ScheduleViewerControl viewerControl)
            {
                viewerControl.RefreshData();
            }
            else if (control is DirectoryControl directoryControl)
            {
                directoryControl.RefreshData();
            }
            else if (control is UsersControl usersControl)
            {
                usersControl.RefreshData();
            }
        }
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
