using Schedule.Core.Models;

namespace Schedule.Core.Services;

public interface IExportService
{
    Task<byte[]> ExportToPdfAsync(Group group, List<Lesson> lessons, bool isEvenWeek);
    Task<byte[]> ExportToExcelAsync(Group group, List<Lesson> lessons, bool isEvenWeek);
    Task<byte[]> ExportAllGroupsToExcelAsync(List<Group> groups, List<Lesson> lessons, DateTime weekStart);
}
