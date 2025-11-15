namespace Schedule.Core.Models;

public class Lesson
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public int ClassroomId { get; set; }
    public DateTime Date { get; set; } // Конкретная дата урока
    public int LessonNumber { get; set; } // Номер пары (1-8)
    public string? Notes { get; set; } // Дополнительные заметки
    
    public Group Group { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Classroom Classroom { get; set; } = null!;
}
