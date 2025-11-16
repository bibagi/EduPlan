namespace Schedule.Core.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, Methodist, Teacher
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
