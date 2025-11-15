namespace Schedule.Core.Services;

public interface IUpdateService
{
    Task<(bool hasUpdate, string version, string description)> CheckForUpdatesAsync();
    Task<bool> DownloadAndInstallUpdateAsync(string version);
}
