using PlanningEngineerApplication.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace PlanningEngineerApplication.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["BuildingAddressSortParm"] = String.IsNullOrEmpty(sortOrder) ? "address_desc" : "";
            ViewData["ApartmentSortParm"] = sortOrder == "Apartment" ? "apartment_desc" : "Apartment";
            ViewData["RoomSortParm"] = sortOrder == "Room" ? "room_desc" : "Room";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CitySortParm"] = sortOrder == "City" ? "city_desc" : "City";

            var buildings = await _context.Buildings
                .Include(b => b.Apartments)
                    .ThenInclude(a => a.Rooms)
                        .ThenInclude(r => r.Measurements)
                .ToListAsync();

            var reports = from b in buildings
                          from a in b.Apartments
                          from r in a.Rooms
                          from m in r.Measurements.DefaultIfEmpty()
                          select new
                          {
                              BuildingCity = b.City,
                              BuildingAddress = b.Address,
                              BuildingMaterial = b.Material,
                              ApartmentNumber = a.Number,
                              RoomName = r.Name,
                              MeasurementDate = m != null ? m.DateMeasured : (DateTime?)null,
                              CeilingDeviation = m != null ? m.CeilingDeviation : (double?)null,
                              Wall1Deviation = m != null ? m.Wall1Deviation : (double?)null,
                              Wall2Deviation = m != null ? m.Wall2Deviation : (double?)null,
                              Wall3Deviation = m != null ? m.Wall3Deviation : (double?)null,
                              Wall4Deviation = m != null ? m.Wall4Deviation : (double?)null,
                              FloorDeviation = m != null ? m.FloorDeviation : (double?)null
                          };

            if (!String.IsNullOrEmpty(searchString))
            {
                reports = reports.Where(r => r.BuildingAddress.Contains(searchString)
                                       || r.ApartmentNumber.ToString().Contains(searchString)
                                       || r.RoomName.Contains(searchString)
                                       || r.BuildingCity.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "address_desc":
                    reports = reports.OrderByDescending(r => r.BuildingAddress);
                    break;
                case "Apartment":
                    reports = reports.OrderBy(r => r.ApartmentNumber);
                    break;
                case "apartment_desc":
                    reports = reports.OrderByDescending(r => r.ApartmentNumber);
                    break;
                case "Room":
                    reports = reports.OrderBy(r => r.RoomName);
                    break;
                case "room_desc":
                    reports = reports.OrderByDescending(r => r.RoomName);
                    break;
                case "Date":
                    reports = reports.OrderBy(r => r.MeasurementDate);
                    break;
                case "date_desc":
                    reports = reports.OrderByDescending(r => r.MeasurementDate);
                    break;
                case "City":
                    reports = reports.OrderBy(r => r.BuildingCity);
                    break;
                case "city_desc":
                    reports = reports.OrderByDescending(r => r.BuildingCity);
                    break;
                default:
                    reports = reports.OrderBy(r => r.BuildingAddress);
                    break;
            }

            return View(reports.ToList());
        }

        public async Task<IActionResult> DownloadReport()
        {
            var buildings = await _context.Buildings
                .Include(b => b.Apartments)
                    .ThenInclude(a => a.Rooms)
                        .ThenInclude(r => r.Measurements)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                worksheet.Cells[1, 1].Value = "Город";
                worksheet.Cells[1, 2].Value = "Адрес здания";
                worksheet.Cells[1, 3].Value = "Материал здания";
                worksheet.Cells[1, 4].Value = "Квартира";
                worksheet.Cells[1, 5].Value = "Комната";
                worksheet.Cells[1, 6].Value = "Дата замера";
                worksheet.Cells[1, 7].Value = "Отклонение потолка (мм.)";
                worksheet.Cells[1, 8].Value = "Отклонение стены 1 (мм.)";
                worksheet.Cells[1, 9].Value = "Отклонение стены 2 (мм.)";
                worksheet.Cells[1, 10].Value = "Отклонение стены 3 (мм.)";
                worksheet.Cells[1, 11].Value = "Отклонение стены 4 (мм.)";
                worksheet.Cells[1, 12].Value = "Отклонение пола (мм.)";

                int row = 2;

                foreach (var building in buildings)
                {
                    foreach (var apartment in building.Apartments)
                    {
                        foreach (var room in apartment.Rooms)
                        {
                            if (room.Measurements != null && room.Measurements.Any())
                            {
                                foreach (var measurement in room.Measurements)
                                {
                                    worksheet.Cells[row, 1].Value = building.City;
                                    worksheet.Cells[row, 2].Value = building.Address;
                                    worksheet.Cells[row, 3].Value = building.Material.GetDisplayName();
                                    worksheet.Cells[row, 4].Value = $"Квартира № {apartment.Number}";
                                    worksheet.Cells[row, 5].Value = room.Name;
                                    worksheet.Cells[row, 6].Value = measurement.DateMeasured.ToString("dd.MM.yyyy");
                                    worksheet.Cells[row, 7].Value = measurement.CeilingDeviation != null ? $"{measurement.CeilingDeviation} мм" : "Нет данных";
                                    worksheet.Cells[row, 8].Value = measurement.Wall1Deviation != null ? $"{measurement.Wall1Deviation} мм" : "Нет данных";
                                    worksheet.Cells[row, 9].Value = measurement.Wall2Deviation != null ? $"{measurement.Wall2Deviation} мм" : "Нет данных";
                                    worksheet.Cells[row, 10].Value = measurement.Wall3Deviation != null ? $"{measurement.Wall3Deviation} мм" : "Нет данных";
                                    worksheet.Cells[row, 11].Value = measurement.Wall4Deviation != null ? $"{measurement.Wall4Deviation} мм" : "Нет данных";
                                    worksheet.Cells[row, 12].Value = measurement.FloorDeviation != null ? $"{measurement.FloorDeviation} мм" : "Нет данных";

                                    row++;
                                }
                            }
                            else
                            {
                                worksheet.Cells[row, 1].Value = building.City;
                                worksheet.Cells[row, 2].Value = building.Address;
                                worksheet.Cells[row, 3].Value = building.Material.GetDisplayName();
                                worksheet.Cells[row, 4].Value = $"Квартира № {apartment.Number}";
                                worksheet.Cells[row, 5].Value = room.Name;
                                worksheet.Cells[row, 6].Value = "Нет данных о замерах";
                                worksheet.Cells[row, 7].Value = "Нет данных";
                                worksheet.Cells[row, 8].Value = "Нет данных";
                                worksheet.Cells[row, 9].Value = "Нет данных";
                                worksheet.Cells[row, 10].Value = "Нет данных";
                                worksheet.Cells[row, 11].Value = "Нет данных";
                                worksheet.Cells[row, 12].Value = "Нет данных";

                                row++;
                            }
                        }
                    }
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
            }
        }
    }
}
