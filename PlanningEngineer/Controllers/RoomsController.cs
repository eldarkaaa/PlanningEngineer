using PlanningEngineerApplication.Data;
using PlanningEngineerApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningEngineerApplication.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RoomsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

       
        public async Task<IActionResult> Index(int? apartmentId)
        {
            if (apartmentId == null)
            {
                return NotFound();
            }

            var apartment = await _context.Apartments
                .Include(a => a.Rooms)
                .Include(a => a.Building)
                .FirstOrDefaultAsync(a => a.Id == apartmentId);

            if (apartment == null)
            {
                return NotFound();
            }

            ViewBag.ApartmentId = apartmentId;
            ViewBag.ApartmentNumber = apartment.Number;
            ViewBag.ApartmentImagePath = apartment.ImagePath;

            return View(apartment.Rooms);
        }

        
        [Authorize(Roles = "Admin, Moderator")]
        public IActionResult Create(int apartmentId)
        {
            ViewBag.ApartmentId = apartmentId;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Create(RoomsDto roomsDto)
        {
            if (ModelState.IsValid)
            {
                var room = new Rooms
                {
                    Name = roomsDto.Name,
                    ApartmentId = roomsDto.ApartmentId
                };

                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { apartmentId = room.ApartmentId });
            }
            ViewBag.ApartmentId = roomsDto.ApartmentId;
            return View(roomsDto);
        }

       
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.ApartmentId = room.ApartmentId; 
            return View(room);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ApartmentId")] Rooms room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { apartmentId = room.ApartmentId });
            }
            return View(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Apartment)
                .ThenInclude(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            var roomsMeasurementDto = new RoomsMeasurementDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                ApartmentId = room.ApartmentId,
                ApartmentNumber = room.Apartment.Number,
                BuildingAddress = room.Apartment.Building.Address,
                CeilingDeviation = 0,
                Wall1Deviation = 0,
                Wall2Deviation = 0,
                Wall3Deviation = 0,
                Wall4Deviation = 0,
                FloorDeviation = 0
            };

            ViewBag.RoomImagePath = room.ImagePath;
            return View(roomsMeasurementDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> UploadImage(int apartmentId, IFormFile imageFile)
        {
            var apartment = await _context.Apartments.Include(a => a.Rooms).FirstOrDefaultAsync(a => a.Id == apartmentId);
            if (apartment == null)
            {
                return NotFound();
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid image file.");
                return RedirectToAction("Index", new { apartmentId = apartmentId });
            }

            var uploadPath = Path.Combine(_environment.WebRootPath, "Images");
            Directory.CreateDirectory(uploadPath); 

            var filePath = Path.Combine(uploadPath, imageFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            apartment.ImagePath = $"/Images/{imageFile.FileName}";
            _context.Update(apartment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { apartmentId = apartmentId });
        }

        
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Apartment)
                .ThenInclude(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { apartmentId = room.ApartmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> SaveMeasurements(int roomId, double ceilingDeviation, double wall1Deviation, double wall2Deviation, double wall3Deviation, double wall4Deviation, double floorDeviation, int apartmentId)
        {
            if (ModelState.IsValid)
            {
                var existingMeasurement = await _context.Measurements
                    .FirstOrDefaultAsync(m => m.RoomId == roomId);

                if (existingMeasurement != null)
                {
                    
                    existingMeasurement.CeilingDeviation = ceilingDeviation;
                    existingMeasurement.Wall1Deviation = wall1Deviation;
                    existingMeasurement.Wall2Deviation = wall2Deviation;
                    existingMeasurement.Wall3Deviation = wall3Deviation;
                    existingMeasurement.Wall4Deviation = wall4Deviation;
                    existingMeasurement.FloorDeviation = floorDeviation;
                    existingMeasurement.DateMeasured = DateTime.Now;

                    _context.Update(existingMeasurement);
                }
                else
                {
                    
                    var measurement = new Measurement
                    {
                        RoomId = roomId,
                        CeilingDeviation = ceilingDeviation,
                        Wall1Deviation = wall1Deviation,
                        Wall2Deviation = wall2Deviation,
                        Wall3Deviation = wall3Deviation,
                        Wall4Deviation = wall4Deviation,
                        FloorDeviation = floorDeviation,
                        DateMeasured = DateTime.Now
                    };

                    _context.Measurements.Add(measurement);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { apartmentId = apartmentId });
            }

            return View(new RoomsMeasurementDto
            {
                RoomId = roomId,
                CeilingDeviation = ceilingDeviation,
                Wall1Deviation = wall1Deviation,
                Wall2Deviation = wall2Deviation,
                Wall3Deviation = wall3Deviation,
                Wall4Deviation = wall4Deviation,
                FloorDeviation = floorDeviation,
                ApartmentId = apartmentId
            });
        }
    }
}
