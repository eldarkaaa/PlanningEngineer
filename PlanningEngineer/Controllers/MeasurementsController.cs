using PlanningEngineerApplication.Data;
using Microsoft.AspNetCore.Mvc;
using PlanningEngineerApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace PlanningEngineerApplication.Controllers
{
    [Authorize]
    public class MeasurementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeasurementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int roomId)
        {
            var measurements = await _context.Measurements
                .Where(m => m.RoomId == roomId)
                .ToListAsync();
            return View(measurements);
        }

        public IActionResult Create(int roomId)
        {
            ViewBag.RoomId = roomId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Measurement measurement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(measurement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { roomId = measurement.RoomId });
            }
            return View(measurement);
        }

       
    }


}
