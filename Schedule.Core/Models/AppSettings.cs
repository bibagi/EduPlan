namespace Schedule.Core.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Light"; // Light, Dark
    public string LastUpdateCheck { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = "1.0.0";
}
