namespace Schedule.Core.Services;

public interface IImportService
{
    Task<(int success, int failed, List<string> errors)> ImportFromTextAsync(string filePath, string entityType);
    Task<(int success, int failed, List<string> errors)> ImportFromExcelAsync(string filePath, string entityType);
}
