namespace Schedule.Core.Models;

/// <summary>
/// Шаблон недельного расписания (для автоматического создания)
/// </summary>
public class WeekSchedule
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public int ClassroomId { get; set; }
    public int DayOfWeek { get; set; } // 0=Пн, 6=Вс
    public int LessonNumber { get; set; } // 1-8
    public bool IsEvenWeek { get; set; } // true=чётная, false=нечётная
    
    public Group Group { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Classroom Classroom { get; set; } = null!;
}
