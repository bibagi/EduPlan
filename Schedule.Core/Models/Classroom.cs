namespace Schedule.Core.Models;

public class Classroom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
