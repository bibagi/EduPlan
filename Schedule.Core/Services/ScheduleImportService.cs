using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Schedule.Core.Data;
using Schedule.Core.Models;
using System.Globalization;

namespace Schedule.Core.Services;

public class ScheduleImportService : IScheduleImportService
{
    private readonly ScheduleDbContext _context;

    public ScheduleImportService(ScheduleDbContext context)
    {
        _context = context;
    }

    public async Task<(int success, int failed, List<string> errors)> ImportScheduleAsync(string filePath)
    {
        var errors = new List<string>();
        int success = 0;
        int failed = 0;

        try
        {
            if (filePath.EndsWith(".xlsx"))
            {
                return await ImportFromExcelAsync(filePath);
            }
            else
            {
                return await ImportFromTextAsync(filePath);
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Ошибка чтения файла: {ex.Message}");
            return (0, 0, errors);
        }
    }

    private async Task<(int success, int failed, List<string> errors)> ImportFromTextAsync(string filePath)
    {
        var errors = new List<string>();
        int success = 0;
        int failed = 0;

        var lines = await File.ReadAllLinesAsync(filePath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(new[] { ';', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            if (parts.Length < 6)
            {
                errors.Add($"Недостаточно данных в строке: {line}");
                failed++;
                continue;
            }

            try
            {
                // Формат: Дата;Группа;Номер пары;Предмет;Учитель;Аудитория
                var date = DateTime.ParseExact(parts[0], new[] { "dd.MM.yyyy", "dd.MM.yy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None);
                var groupName = parts[1];
                var lessonNumber = int.Parse(parts[2]);
                var subjectName = parts[3];
                var teacherShortName = parts[4];
                var classroomName = parts[5];

                // Находим сущности
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == groupName);
                if (group == null)
                {
                    errors.Add($"Группа '{groupName}' не найдена");
                    failed++;
                    continue;
                }

                var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == subjectName);
                if (subject == null)
                {
                    errors.Add($"Предмет '{subjectName}' не найден");
                    failed++;
                    continue;
                }

                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.ShortName == teacherShortName);
                if (teacher == null)
                {
                    errors.Add($"Учитель '{teacherShortName}' не найден");
                    failed++;
                    continue;
                }

                var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Name == classroomName);
                if (classroom == null)
                {
                    errors.Add($"Аудитория '{classroomName}' не найдена");
                    failed++;
                    continue;
                }

                // Проверяем дубликат
                var exists = await _context.Lessons.AnyAsync(l =>
                    l.Date == date &&
                    l.GroupId == group.Id &&
                    l.LessonNumber == lessonNumber);

                if (exists)
                {
                    errors.Add($"Урок уже существует: {date:dd.MM.yyyy}, {groupName}, пара {lessonNumber}");
                    failed++;
                    continue;
                }

                // Создаём урок
                var lesson = new Lesson
                {
                    Date = date,
                    GroupId = group.Id,
                    SubjectId = subject.Id,
                    TeacherId = teacher.Id,
                    ClassroomId = classroom.Id,
                    LessonNumber = lessonNumber
                };

                _context.Lessons.Add(lesson);
                success++;
            }
            catch (Exception ex)
            {
                errors.Add($"Ошибка в строке '{line}': {ex.Message}");
                failed++;
            }
        }

        await _context.SaveChangesAsync();
        return (success, failed, errors);
    }

    private async Task<(int success, int failed, List<string> errors)> ImportFromExcelAsync(string filePath)
    {
        var errors = new List<string>();
        int success = 0;
        int failed = 0;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];

        int rowCount = worksheet.Dimension?.Rows ?? 0;

        // Начинаем со 2-й строки (1-я - заголовки)
        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                var dateStr = worksheet.Cells[row, 1].Value?.ToString();
                var groupName = worksheet.Cells[row, 2].Value?.ToString();
                var lessonNumberStr = worksheet.Cells[row, 3].Value?.ToString();
                var subjectName = worksheet.Cells[row, 4].Value?.ToString();
                var teacherShortName = worksheet.Cells[row, 5].Value?.ToString();
                var classroomName = worksheet.Cells[row, 6].Value?.ToString();

                if (string.IsNullOrWhiteSpace(dateStr) || string.IsNullOrWhiteSpace(groupName))
                    continue;

                var date = DateTime.Parse(dateStr!);
                var lessonNumber = int.Parse(lessonNumberStr!);

                // Находим сущности
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == groupName);
                if (group == null)
                {
                    errors.Add($"Строка {row}: Группа '{groupName}' не найдена");
                    failed++;
                    continue;
                }

                var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == subjectName);
                if (subject == null)
                {
                    errors.Add($"Строка {row}: Предмет '{subjectName}' не найден");
                    failed++;
                    continue;
                }

                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.ShortName == teacherShortName);
                if (teacher == null)
                {
                    errors.Add($"Строка {row}: Учитель '{teacherShortName}' не найден");
                    failed++;
                    continue;
                }

                var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Name == classroomName);
                if (classroom == null)
                {
                    errors.Add($"Строка {row}: Аудитория '{classroomName}' не найдена");
                    failed++;
                    continue;
                }

                // Проверяем дубликат
                var exists = await _context.Lessons.AnyAsync(l =>
                    l.Date == date &&
                    l.GroupId == group.Id &&
                    l.LessonNumber == lessonNumber);

                if (!exists)
                {
                    var lesson = new Lesson
                    {
                        Date = date,
                        GroupId = group.Id,
                        SubjectId = subject.Id,
                        TeacherId = teacher.Id,
                        ClassroomId = classroom.Id,
                        LessonNumber = lessonNumber
                    };

                    _context.Lessons.Add(lesson);
                    success++;
                }
                else
                {
                    failed++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Строка {row}: {ex.Message}");
                failed++;
            }
        }

        await _context.SaveChangesAsync();
        return (success, failed, errors);
    }
}
