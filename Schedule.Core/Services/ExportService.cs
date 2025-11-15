using OfficeOpenXml;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Schedule.Core.Models;

namespace Schedule.Core.Services;

public class ExportService : IExportService
{
    public Task<byte[]> ExportToPdfAsync(Group group, List<Lesson> lessons, bool isEvenWeek)
    {
        return Task.Run(() =>
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Width = XUnit.FromMillimeter(297).Point;
            page.Height = XUnit.FromMillimeter(210).Point;
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12);
            var fontBold = new XFont("Arial", 14, XFontStyleEx.Bold);

            string weekType = isEvenWeek ? "чётная" : "нечётная";
            gfx.DrawString($"Расписание группы {group.Name}, неделя {weekType}",
                fontBold, XBrushes.Black, new XRect(0, 20, page.Width.Point, 30), XStringFormats.TopCenter);

            double startX = 40;
            double startY = 60;
            double cellWidth = 120;
            double cellHeight = 60;
            string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };

            for (int i = 0; i < 6; i++)
            {
                gfx.DrawRectangle(XPens.Black, startX + i * cellWidth, startY, cellWidth, cellHeight);
                gfx.DrawString(days[i], font, XBrushes.Black,
                    new XRect(startX + i * cellWidth, startY + 5, cellWidth, cellHeight),
                    XStringFormats.TopCenter);
            }

            // Группируем уроки по дате
            var lessonsByDate = lessons.GroupBy(l => l.Date).OrderBy(g => g.Key).ToList();
            
            int row = 1;
            foreach (var dateGroup in lessonsByDate)
            {
                double y = startY + row * cellHeight;
                
                // Рисуем дату
                gfx.DrawRectangle(XPens.Black, startX - 100, y, 100, cellHeight);
                gfx.DrawString(dateGroup.Key.ToString("dd.MM"), font, XBrushes.Black,
                    new XRect(startX - 100, y + 5, 100, cellHeight), XStringFormats.TopCenter);
                
                // Рисуем уроки для этой даты
                var dayLessons = dateGroup.OrderBy(l => l.LessonNumber).ToList();
                string lessonsText = string.Join("\n", dayLessons.Select(l => 
                    $"{l.LessonNumber}. {l.Subject.Name} ({l.Classroom.Name})"));
                
                gfx.DrawRectangle(XPens.Black, startX, y, cellWidth * 6, cellHeight);
                gfx.DrawString(lessonsText, new XFont("Arial", 7), XBrushes.Black,
                    new XRect(startX + 5, y + 5, cellWidth * 6 - 10, cellHeight - 10),
                    XStringFormats.TopLeft);
                
                row++;
            }

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        });
    }

    public Task<byte[]> ExportToExcelAsync(Group group, List<Lesson> lessons, bool isEvenWeek)
    {
        return Task.Run(() =>
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Расписание");

            // Заголовок
            worksheet.Cells[1, 1].Value = $"Расписание группы {group.Name}";
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 25;

            // Заголовки столбцов
            worksheet.Cells[3, 1].Value = "Пара";
            worksheet.Cells[3, 2].Value = "Понедельник";
            worksheet.Cells[3, 3].Value = "КАБ.";
            
            for (int i = 1; i <= 3; i++)
            {
                var cell = worksheet.Cells[3, i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 11;
                cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }
            worksheet.Row(3).Height = 20;

            // Группируем уроки по дню недели и номеру пары
            var lessonsByDay = lessons
                .GroupBy(l => l.Date.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.GroupBy(l => l.LessonNumber).ToDictionary(lg => lg.Key, lg => lg.ToList()));

            int currentRow = 4;
            string[] dayNames = { "", "ПОНЕДЕЛЬНИК", "ВТОРНИК", "СРЕДА", "ЧЕТВЕРГ", "ПЯТНИЦА", "СУББОТА" };
            
            // Проходим по дням недели
            for (int dayIndex = 1; dayIndex <= 6; dayIndex++) // Пн=1, Вт=2, ..., Сб=6
            {
                var dayOfWeek = (DayOfWeek)dayIndex;
                
                if (!lessonsByDay.ContainsKey(dayOfWeek) || !lessonsByDay[dayOfWeek].Any())
                    continue;

                // Заголовок дня
                var dayCell = worksheet.Cells[currentRow, 1];
                dayCell.Value = dayNames[dayIndex];
                dayCell.Style.Font.Bold = true;
                dayCell.Style.Font.Size = 11;
                dayCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                dayCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                dayCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                worksheet.Cells[currentRow, 2, currentRow, 3].Merge = true;
                worksheet.Cells[currentRow, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                worksheet.Row(currentRow).Height = 20;
                currentRow++;

                // Уроки для этого дня (максимум 5 пар)
                for (int lessonNum = 1; lessonNum <= 5; lessonNum++)
                {
                    // Номер пары
                    var lessonCell = worksheet.Cells[currentRow, 1];
                    lessonCell.Value = lessonNum;
                    lessonCell.Style.Font.Size = 10;
                    lessonCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    lessonCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    lessonCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    // Предмет и преподаватель
                    var subjectCell = worksheet.Cells[currentRow, 2];
                    var classroomCell = worksheet.Cells[currentRow, 3];
                    
                    if (lessonsByDay[dayOfWeek].ContainsKey(lessonNum))
                    {
                        var dayLessons = lessonsByDay[dayOfWeek][lessonNum];
                        var lessonTexts = dayLessons.Select(l => $"{l.Subject.Name}\n{l.Teacher.ShortName}");
                        var classrooms = dayLessons.Select(l => l.Classroom.Name);
                        
                        subjectCell.Value = string.Join("\n\n", lessonTexts);
                        classroomCell.Value = string.Join("\n\n", classrooms);
                    }
                    else
                    {
                        subjectCell.Value = "";
                        classroomCell.Value = "";
                    }
                    
                    subjectCell.Style.Font.Size = 10;
                    subjectCell.Style.WrapText = true;
                    subjectCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    subjectCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    subjectCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    
                    classroomCell.Style.Font.Size = 10;
                    classroomCell.Style.WrapText = true;
                    classroomCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    classroomCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    classroomCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    
                    worksheet.Row(currentRow).Height = 45;
                    currentRow++;
                }
            }

            // Настройка ширины столбцов
            worksheet.Column(1).Width = 6;   // Пара
            worksheet.Column(2).Width = 30;  // Предмет + Преподаватель
            worksheet.Column(3).Width = 8;   // КАБ.

            return package.GetAsByteArray();
        });
    }

    public Task<byte[]> ExportAllGroupsToExcelAsync(List<Group> groups, List<Lesson> lessons, DateTime weekStart)
    {
        return Task.Run(() =>
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Расписание");

            // Заголовок
            int totalColumns = 1 + (groups.Count * 2); // День + (Группа + КАБ.) * количество групп
            worksheet.Cells[1, 1].Value = $"Расписание групп";
            worksheet.Cells[1, 1, 1, totalColumns].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Row(1).Height = 25;

            // Заголовки столбцов
            int col = 1;
            worksheet.Cells[3, col].Value = "День";
            worksheet.Cells[3, col].Style.Font.Bold = true;
            worksheet.Cells[3, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[3, col].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Cells[3, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            col++;

            foreach (var group in groups.OrderBy(g => g.Name))
            {
                // Заголовок группы
                worksheet.Cells[3, col].Value = group.Name;
                worksheet.Cells[3, col].Style.Font.Bold = true;
                worksheet.Cells[3, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, col].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[3, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                
                // Заголовок КАБ.
                worksheet.Cells[3, col + 1].Value = "КАБ.";
                worksheet.Cells[3, col + 1].Style.Font.Bold = true;
                worksheet.Cells[3, col + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, col + 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[3, col + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                
                col += 2;
            }
            worksheet.Row(3).Height = 20;

            // Группируем уроки по группе, дню недели и номеру пары
            var lessonsByGroup = lessons
                .GroupBy(l => l.GroupId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(l => l.Date.DayOfWeek)
                          .ToDictionary(
                              dg => dg.Key,
                              dg => dg.GroupBy(l => l.LessonNumber)
                                     .ToDictionary(lg => lg.Key, lg => lg.ToList())
                          )
                );

            int currentRow = 4;
            string[] dayNames = { "", "ПОНЕДЕЛЬНИК", "ВТОРНИК", "СРЕДА", "ЧЕТВЕРГ", "ПЯТНИЦА", "СУББОТА" };
            
            // Проходим по дням недели
            for (int dayIndex = 1; dayIndex <= 6; dayIndex++) // Пн=1, Вт=2, ..., Сб=6
            {
                var dayOfWeek = (DayOfWeek)dayIndex;
                var currentDate = weekStart.AddDays(dayIndex - 1);

                // Проверяем, есть ли уроки в этот день
                bool hasLessonsThisDay = lessonsByGroup.Values.Any(dayDict => dayDict.ContainsKey(dayOfWeek));
                if (!hasLessonsThisDay)
                    continue;

                // Заголовок дня с датой
                var dayCell = worksheet.Cells[currentRow, 1];
                dayCell.Value = $"{dayNames[dayIndex]} ({currentDate:dd.MM})";
                dayCell.Style.Font.Bold = true;
                dayCell.Style.Font.Size = 11;
                dayCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                dayCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                dayCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                worksheet.Cells[currentRow, 2, currentRow, totalColumns].Merge = true;
                worksheet.Cells[currentRow, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                worksheet.Row(currentRow).Height = 20;
                currentRow++;

                // Уроки для этого дня (максимум 5 пар)
                for (int lessonNum = 1; lessonNum <= 5; lessonNum++)
                {
                    // Номер пары
                    var lessonCell = worksheet.Cells[currentRow, 1];
                    lessonCell.Value = lessonNum;
                    lessonCell.Style.Font.Size = 10;
                    lessonCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    lessonCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    lessonCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    // Для каждой группы
                    col = 2;
                    foreach (var group in groups.OrderBy(g => g.Name))
                    {
                        var subjectCell = worksheet.Cells[currentRow, col];
                        var classroomCell = worksheet.Cells[currentRow, col + 1];
                        
                        if (lessonsByGroup.ContainsKey(group.Id) &&
                            lessonsByGroup[group.Id].ContainsKey(dayOfWeek) &&
                            lessonsByGroup[group.Id][dayOfWeek].ContainsKey(lessonNum))
                        {
                            var dayLessons = lessonsByGroup[group.Id][dayOfWeek][lessonNum];
                            var lessonTexts = dayLessons.Select(l => $"{l.Subject.Name}\n{l.Teacher.ShortName}");
                            var classrooms = dayLessons.Select(l => l.Classroom.Name);
                            
                            subjectCell.Value = string.Join("\n\n", lessonTexts);
                            classroomCell.Value = string.Join("\n\n", classrooms);
                        }
                        else
                        {
                            subjectCell.Value = "";
                            classroomCell.Value = "";
                        }
                        
                        subjectCell.Style.Font.Size = 9;
                        subjectCell.Style.WrapText = true;
                        subjectCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                        subjectCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        subjectCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        
                        classroomCell.Style.Font.Size = 9;
                        classroomCell.Style.WrapText = true;
                        classroomCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        classroomCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        classroomCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        
                        col += 2;
                    }
                    
                    worksheet.Row(currentRow).Height = 40;
                    currentRow++;
                }
            }

            // Настройка ширины столбцов
            worksheet.Column(1).Width = 18;  // День
            for (int i = 0; i < groups.Count; i++)
            {
                worksheet.Column(2 + i * 2).Width = 22;      // Группа
                worksheet.Column(2 + i * 2 + 1).Width = 6;   // КАБ.
            }

            return package.GetAsByteArray();
        });
    }

}
