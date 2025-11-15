using OfficeOpenXml;
using Schedule.Core.Data;
using Schedule.Core.Models;

namespace Schedule.Core.Services;

public class ImportService : IImportService
{
    private readonly ScheduleDbContext _context;

    public ImportService(ScheduleDbContext context)
    {
        _context = context;
    }

    public async Task<(int success, int failed, List<string> errors)> ImportFromTextAsync(string filePath, string entityType)
    {
        var errors = new List<string>();
        int success = 0;
        int failed = 0;

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Поддержка разделителей: табуляция, точка с запятой, запятая
                var parts = line.Split(new[] { '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();

                try
                {
                    switch (entityType)
                    {
                        case "Teachers":
                            await ImportTeacherAsync(parts);
                            break;
                        case "Classrooms":
                            await ImportClassroomAsync(parts);
                            break;
                        case "Subjects":
                            await ImportSubjectAsync(parts);
                            break;
                        case "Groups":
                            await ImportGroupAsync(parts);
                            break;
                        default:
                            errors.Add($"Неизвестный тип: {entityType}");
                            failed++;
                            continue;
                    }
                    success++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Строка '{line}': {ex.Message}");
                    failed++;
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            errors.Add($"Ошибка чтения файла: {ex.Message}");
        }

        return (success, failed, errors);
    }

    public async Task<(int success, int failed, List<string> errors)> ImportFromExcelAsync(string filePath, string entityType)
    {
        var errors = new List<string>();
        int success = 0;
        int failed = 0;

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            
            // Начинаем со 2-й строки (1-я - заголовки)
            for (int row = 2; row <= rowCount; row++)
            {
                var parts = new List<string>();
                int col = 1;
                
                while (worksheet.Cells[row, col].Value != null)
                {
                    parts.Add(worksheet.Cells[row, col].Value.ToString()?.Trim() ?? "");
                    col++;
                }

                if (parts.Count == 0)
                    continue;

                try
                {
                    switch (entityType)
                    {
                        case "Teachers":
                            await ImportTeacherAsync(parts.ToArray());
                            break;
                        case "Classrooms":
                            await ImportClassroomAsync(parts.ToArray());
                            break;
                        case "Subjects":
                            await ImportSubjectAsync(parts.ToArray());
                            break;
                        case "Groups":
                            await ImportGroupAsync(parts.ToArray());
                            break;
                        default:
                            errors.Add($"Неизвестный тип: {entityType}");
                            failed++;
                            continue;
                    }
                    success++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Строка {row}: {ex.Message}");
                    failed++;
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            errors.Add($"Ошибка чтения Excel: {ex.Message}");
        }

        return (success, failed, errors);
    }

    private async Task ImportTeacherAsync(string[] parts)
    {
        if (parts.Length < 2)
            throw new Exception("Требуется: Полное имя, Краткое имя");

        var fullName = parts[0];
        var shortName = parts[1];

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(shortName))
            throw new Exception("Имена не могут быть пустыми");

        // Проверяем дубликаты
        if (_context.Teachers.Any(t => t.FullName == fullName))
            throw new Exception($"Учитель '{fullName}' уже существует");

        var teacher = new Teacher
        {
            FullName = fullName,
            ShortName = shortName
        };

        _context.Teachers.Add(teacher);
    }

    private async Task ImportClassroomAsync(string[] parts)
    {
        if (parts.Length < 2)
            throw new Exception("Требуется: Название, Вместимость");

        var name = parts[0];
        
        if (!int.TryParse(parts[1], out int capacity))
            throw new Exception("Вместимость должна быть числом");

        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Название не может быть пустым");

        // Проверяем дубликаты
        if (_context.Classrooms.Any(c => c.Name == name))
            throw new Exception($"Аудитория '{name}' уже существует");

        var classroom = new Classroom
        {
            Name = name,
            Capacity = capacity
        };

        _context.Classrooms.Add(classroom);
        await Task.CompletedTask;
    }

    private async Task ImportSubjectAsync(string[] parts)
    {
        if (parts.Length < 1)
            throw new Exception("Требуется: Название предмета");

        var name = parts[0];

        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Название не может быть пустым");

        // Проверяем дубликаты
        if (_context.Subjects.Any(s => s.Name == name))
            throw new Exception($"Предмет '{name}' уже существует");

        var subject = new Subject
        {
            Name = name
        };

        _context.Subjects.Add(subject);
        await Task.CompletedTask;
    }

    private async Task ImportGroupAsync(string[] parts)
    {
        if (parts.Length < 2)
            throw new Exception("Требуется: Название, Курс");

        var name = parts[0];
        
        if (!int.TryParse(parts[1], out int year))
            throw new Exception("Курс должен быть числом");

        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Название не может быть пустым");

        // Проверяем дубликаты
        if (_context.Groups.Any(g => g.Name == name))
            throw new Exception($"Группа '{name}' уже существует");

        var group = new Group
        {
            Name = name,
            Year = year
        };

        _context.Groups.Add(group);
        await Task.CompletedTask;
    }
}
