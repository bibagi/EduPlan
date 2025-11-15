namespace Schedule.Core.Services;

public interface IScheduleImportService
{
    Task<(int success, int failed, List<string> errors)> ImportScheduleAsync(string filePath);
}
