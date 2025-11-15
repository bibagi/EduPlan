using System.Diagnostics;

namespace Schedule.Core.Services;

public class UpdateService : IUpdateService
{
    private readonly string _gitRepoUrl = "https://github.com/yourusername/schedule-app"; // Замените на ваш репозиторий

    public async Task<(bool hasUpdate, string version, string description)> CheckForUpdatesAsync()
    {
        try
        {
            // Проверяем наличие git
            var gitProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch origin",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            gitProcess.Start();
            await gitProcess.WaitForExitAsync();

            // Проверяем разницу версий
            var logProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "log HEAD..origin/main --oneline",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            logProcess.Start();
            var output = await logProcess.StandardOutput.ReadToEndAsync();
            await logProcess.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var latestCommit = lines.FirstOrDefault() ?? "";
                
                // Определяем тип обновления по коммиту
                bool isMajorUpdate = latestCommit.Contains("[MAJOR]") || latestCommit.Contains("[BREAKING]");
                
                return (true, "latest", latestCommit);
            }

            return (false, "", "");
        }
        catch
        {
            return (false, "", "");
        }
    }

    public async Task<bool> DownloadAndInstallUpdateAsync(string version)
    {
        try
        {
            var pullProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "pull origin main",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            pullProcess.Start();
            await pullProcess.WaitForExitAsync();

            return pullProcess.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
