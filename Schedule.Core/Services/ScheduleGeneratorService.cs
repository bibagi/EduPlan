using Microsoft.EntityFrameworkCore;
using Schedule.Core.Data;
using Schedule.Core.Models;

namespace Schedule.Core.Services;

public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly ScheduleDbContext _context;

    public ScheduleGeneratorService(ScheduleDbContext context)
    {
        _context = context;
    }

    public async Task GenerateWeekScheduleAsync(DateTime startDate, DateTime endDate)
    {
        var templates = await _context.WeekSchedules
            .Include(w => w.Group)
            .Include(w => w.Subject)
            .Include(w => w.Teacher)
            .Include(w => w.Classroom)
            .ToListAsync();

        var currentDate = startDate.Date;
        
        while (currentDate <= endDate.Date)
        {
            int dayOfWeek = (int)currentDate.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Воскресенье = 7
            dayOfWeek--; // 0=Пн, 6=Вс

            // Определяем чётность недели
            int weekNumber = GetWeekNumber(currentDate);
            bool isEvenWeek = weekNumber % 2 == 0;

            // Находим шаблоны для этого дня
            var dayTemplates = templates.Where(t => 
                t.DayOfWeek == dayOfWeek && 
                t.IsEvenWeek == isEvenWeek).ToList();

            foreach (var template in dayTemplates)
            {
                // Проверяем, нет ли уже урока
                var exists = await _context.Lessons.AnyAsync(l =>
                    l.Date == currentDate &&
                    l.GroupId == template.GroupId &&
                    l.LessonNumber == template.LessonNumber);

                if (!exists)
                {
                    var lesson = new Lesson
                    {
                        Date = currentDate,
                        GroupId = template.GroupId,
                        SubjectId = template.SubjectId,
                        TeacherId = template.TeacherId,
                        ClassroomId = template.ClassroomId,
                        LessonNumber = template.LessonNumber
                    };

                    _context.Lessons.Add(lesson);
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<DateTime, Dictionary<int, List<Lesson>>>> GetWeekScheduleAsync(DateTime startDate)
    {
        var endDate = startDate.AddDays(6);
        
        var lessons = await _context.Lessons
            .Include(l => l.Group)
            .Include(l => l.Subject)
            .Include(l => l.Teacher)
            .Include(l => l.Classroom)
            .Where(l => l.Date >= startDate && l.Date <= endDate)
            .OrderBy(l => l.Date)
            .ThenBy(l => l.LessonNumber)
            .ToListAsync();

        // Группируем: Дата -> GroupId -> List<Lesson>
        var result = new Dictionary<DateTime, Dictionary<int, List<Lesson>>>();

        foreach (var lesson in lessons)
        {
            if (!result.ContainsKey(lesson.Date))
                result[lesson.Date] = new Dictionary<int, List<Lesson>>();

            if (!result[lesson.Date].ContainsKey(lesson.GroupId))
                result[lesson.Date][lesson.GroupId] = new List<Lesson>();

            result[lesson.Date][lesson.GroupId].Add(lesson);
        }

        return result;
    }

    private int GetWeekNumber(DateTime date)
    {
        var jan1 = new DateTime(date.Year, 1, 1);
        var daysOffset = (int)jan1.DayOfWeek;
        var firstMonday = jan1.AddDays(daysOffset == 0 ? 1 : 8 - daysOffset);
        var weekNumber = ((date - firstMonday).Days / 7) + 1;
        return weekNumber;
    }
}
