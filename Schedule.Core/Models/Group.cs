namespace Schedule.Core.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
