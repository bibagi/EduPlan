namespace Schedule.Core.Services;

public interface IScheduleGeneratorService
{
    /// <summary>
    /// Генерирует расписание на неделю на основе шаблона
    /// </summary>
    Task GenerateWeekScheduleAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Получает расписание на неделю для всех групп
    /// </summary>
    Task<Dictionary<DateTime, Dictionary<int, List<Core.Models.Lesson>>>> GetWeekScheduleAsync(DateTime startDate);
}
