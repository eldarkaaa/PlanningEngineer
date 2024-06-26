using System.IO;
using Microsoft.AspNetCore.Mvc;
using Xceed.Words.NET;
using PlanningEngineerApplication.Data;
using Microsoft.EntityFrameworkCore;
using Xceed.Document.NET;
using PlanningEngineerApplication.Models;

namespace PlanningEngineerApplication.Controllers
{
    public class ApartmentReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApartmentReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GenerateReport(int apartmentId)
        {
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .Include(a => a.Rooms)
                .ThenInclude(r => r.Measurements)
                .FirstOrDefaultAsync(a => a.Id == apartmentId);

            if (apartment == null)
            {
                return NotFound();
            }

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "ReportTemplate.docx");
            string reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");

            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            string reportPath = Path.Combine(reportsDirectory, $"Report_{apartmentId}.docx");

            using (var document = DocX.Load(templatePath))
            {
                document.ReplaceText("{BuildingAddress}", apartment.Building.Address, false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16 });
                document.ReplaceText("{ApartmentNumber}", apartment.Number.ToString(), false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16 });
                document.ReplaceText("{ApartmentAddress}", $"{apartment.Building.Address}, квартира {apartment.Number}", false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16 });
                document.ReplaceText("{ReportDate}", DateTime.Now.ToString("dd.MM.yyyy"), false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16 });

                var userEmail = HttpContext.User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    string userInfo = $"{user.LastName} {user.FirstName} {user.MiddleName}";
                    int totalLength = 100; 
                    int paddingLength = totalLength - userInfo.Length;

                    document.ReplaceText("{UserLastName}", user.LastName, false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16, UnderlineStyle = UnderlineStyle.singleLine });
                    document.ReplaceText("{UserFirstName}", user.FirstName, false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16, UnderlineStyle = UnderlineStyle.singleLine });
                    document.ReplaceText("{UserMiddleName}", user.MiddleName.PadRight(paddingLength), false, System.Text.RegularExpressions.RegexOptions.None, new Formatting { FontFamily = new Font("Times New Roman"), Size = 16, UnderlineStyle = UnderlineStyle.singleLine });
                }

                var table = document.Tables[0];

                if (table.ColumnCount >= 9)
                {
                    foreach (var room in apartment.Rooms)
                    {
                        foreach (var measurement in room.Measurements)
                        {
                            var row = table.InsertRow();
                            row.Cells[0].Paragraphs[0].Append($"Квартира № {apartment.Number}")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[1].Paragraphs[0].Append(room.Name)
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[2].Paragraphs[0].Append(measurement.DateMeasured.ToString("dd.MM.yyyy"))
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[3].Paragraphs[0].Append($"{measurement.CeilingDeviation} мм")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[4].Paragraphs[0].Append($"{measurement.Wall1Deviation} мм")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[5].Paragraphs[0].Append($"{measurement.Wall2Deviation} мм")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[6].Paragraphs[0].Append($"{measurement.Wall3Deviation} мм")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[7].Paragraphs[0].Append($"{measurement.Wall4Deviation} мм")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                            row.Cells[8].Paragraphs[0].Append($"{measurement.FloorDeviation} мм")
                                .Font(new Font("Times New Roman"))
                                .FontSize(12)
                                .Alignment = Alignment.center;
                        }
                    }
                }

                
                var conclusionSection = document.InsertSection();
                var conclusionTitle = conclusionSection.InsertParagraph("ВЫВОД")
                    .Font(new Font("Times New Roman"))
                    .FontSize(16)
                    .Bold()
                    .Alignment = Alignment.center;

                foreach (var room in apartment.Rooms)
                {
                    var deviationLimits = apartment.Building.Material.GetDeviationLimits(); 

                    foreach (var measurement in room.Measurements)
                    {
                        var conclusionText = $"\tПо комнате {room.Name} необходимо устранить следующие недостатки:";

                        bool hasDefects = false;

                        if (measurement.CeilingDeviation > deviationLimits.FloorAndCeilingDeviation)
                        {
                            conclusionText += $"\n\t- Отклонение потолка: {measurement.CeilingDeviation} мм (Значение превышает норму на {measurement.CeilingDeviation - deviationLimits.FloorAndCeilingDeviation} мм)";
                            hasDefects = true;
                        }
                        if (measurement.Wall1Deviation > deviationLimits.WallDeviation)
                        {
                            conclusionText += $"\n\t- Отклонение стены 1: {measurement.Wall1Deviation} мм (Значение превышает норму на {measurement.Wall1Deviation - deviationLimits.WallDeviation} мм)";
                            hasDefects = true;
                        }
                        if (measurement.Wall2Deviation > deviationLimits.WallDeviation)
                        {
                            conclusionText += $"\n\t- Отклонение стены 2: {measurement.Wall2Deviation} мм (Значение превышает норму на {measurement.Wall2Deviation - deviationLimits.WallDeviation} мм)";
                            hasDefects = true;
                        }
                        if (measurement.Wall3Deviation > deviationLimits.WallDeviation)
                        {
                            conclusionText += $"\n\t- Отклонение стены 3: {measurement.Wall3Deviation} мм (Значение превышает норму на {measurement.Wall3Deviation - deviationLimits.WallDeviation} мм)";
                            hasDefects = true;
                        }
                        if (measurement.Wall4Deviation > deviationLimits.WallDeviation)
                        {
                            conclusionText += $"\n\t- Отклонение стены 4: {measurement.Wall4Deviation} мм (Значение превышает норму на {measurement.Wall4Deviation - deviationLimits.WallDeviation} мм)";
                            hasDefects = true;
                        }
                        if (measurement.FloorDeviation > deviationLimits.FloorAndCeilingDeviation)
                        {
                            conclusionText += $"\n\t- Отклонение пола: {measurement.FloorDeviation} мм (Значение превышает норму на {measurement.FloorDeviation - deviationLimits.FloorAndCeilingDeviation} мм)";
                            hasDefects = true;
                        }

                        if (!hasDefects)
                        {
                            conclusionText += "\n\tНедостатки не выявлены.";
                        }

                        var conclusionParagraph = conclusionSection.InsertParagraph(conclusionText)
                            .Font(new Font("Times New Roman"))
                            .FontSize(14)
                            .SpacingBefore(15)
                            .SpacingAfter(15)
                            .Alignment = Alignment.left;
                    }
                }

                document.SaveAs(reportPath);
            }

            var stream = new MemoryStream();
            using (var fileStream = new FileStream(reportPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(stream);
            }
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Report_{apartmentId}.docx");
        }
    }
}
