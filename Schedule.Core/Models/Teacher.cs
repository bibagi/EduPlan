namespace Schedule.Core.Models;

public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
