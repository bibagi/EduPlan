using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Schedule.Core.Data;
using Schedule.Core.Services;
using Schedule.WinForms.Forms;

namespace Schedule.WinForms;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database
                services.AddDbContext<ScheduleDbContext>(options =>
                    options.UseSqlite("Data Source=schedule.db"));

                // Services
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped(typeof(IRepositoryService<>), typeof(RepositoryService<>));
                services.AddScoped<IExportService, ExportService>();
                services.AddScoped<IImportService, ImportService>();
                services.AddScoped<IScheduleImportService, ScheduleImportService>();
                services.AddScoped<IScheduleGeneratorService, ScheduleGeneratorService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IUpdateService, UpdateService>();
                services.AddScoped<DatabaseInitializer>();

                // ViewModels
                services.AddTransient<Core.ViewModels.LoginViewModel>();
                services.AddTransient<Core.ViewModels.MainViewModel>();

                // Forms
                services.AddTransient<LoginForm>();
                services.AddTransient<MainForm>();
            })
            .Build();

        ServiceProvider = host.Services;

        // Initialize database
        using (var scope = ServiceProvider.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            initializer.InitializeAsync().Wait();
        }

        // Show login form
        var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
        Application.Run(loginForm);
    }
}
