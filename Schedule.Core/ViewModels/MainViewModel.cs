using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schedule.Core.Models;
using Schedule.Core.Services;

namespace Schedule.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private User _currentUser = null!;

    [ObservableProperty]
    private string _theme = "Light";

    [ObservableProperty]
    private bool _hasUpdate;

    [ObservableProperty]
    private string _updateVersion = string.Empty;

    public MainViewModel(IUpdateService updateService, ISettingsService settingsService)
    {
        _updateService = updateService;
        _settingsService = settingsService;
    }

    public async Task InitializeAsync(User user)
    {
        CurrentUser = user;
        
        var settings = await _settingsService.LoadSettingsAsync();
        Theme = settings.Theme;

        await CheckForUpdatesAsync();
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        var (hasUpdate, version, description) = await _updateService.CheckForUpdatesAsync();
        HasUpdate = hasUpdate;
        UpdateVersion = version;
    }

    [RelayCommand]
    private async Task ApplyUpdateAsync()
    {
        var success = await _updateService.DownloadAndInstallUpdateAsync(UpdateVersion);
        if (success)
        {
            HasUpdate = false;
        }
    }

    [RelayCommand]
    private async Task ChangeThemeAsync(string theme)
    {
        Theme = theme;
        var settings = await _settingsService.LoadSettingsAsync();
        settings.Theme = theme;
        await _settingsService.SaveSettingsAsync(settings);
    }
}
