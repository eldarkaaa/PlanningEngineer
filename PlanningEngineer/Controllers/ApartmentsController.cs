using PlanningEngineerApplication.Data;
using PlanningEngineerApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace PlanningEngineerApplication.Controllers
{
    [Authorize]
    public class ApartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int buildingId, string sortOrder, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["NumberSortParm"] = String.IsNullOrEmpty(sortOrder) ? "number_desc" : "";
            ViewData["RoomsSortParm"] = sortOrder == "Rooms" ? "rooms_desc" : "Rooms";

            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .ThenInclude(a => a.Rooms)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null)
            {
                return NotFound();
            }

            ViewBag.BuildingId = buildingId;
            ViewBag.BuildingAddress = building.Address;

            var apartments = building.Apartments.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                apartments = apartments.Where(a => a.Number.ToString().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "number_desc":
                    apartments = apartments.OrderByDescending(a => a.Number);
                    break;
                case "Rooms":
                    apartments = apartments.OrderBy(a => a.Rooms.Count);
                    break;
                case "rooms_desc":
                    apartments = apartments.OrderByDescending(a => a.Rooms.Count);
                    break;
                default:
                    apartments = apartments.OrderBy(a => a.Number);
                    break;
            }

            return View(apartments.ToList());
        }


        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartment = await _context.Apartments
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartment == null)
            {
                return NotFound();
            }

            return View(apartment);
        }

        
        public IActionResult Create(int buildingId)
        {
            ViewBag.BuildingId = buildingId;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApartmentCreateDto apartmentDto)
        {
            if (ModelState.IsValid)
            {
                var apartment = new Apartment
                {
                    Number = apartmentDto.Number,
                    BuildingId = apartmentDto.BuildingId
                };

                _context.Add(apartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { buildingId = apartmentDto.BuildingId });
            }
            return View(apartmentDto);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartment = await _context.Apartments.FindAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            var model = new ApartmentCreateDto
            {
                Id = apartment.Id,
                Number = apartment.Number,
                BuildingId = apartment.BuildingId
            };

            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApartmentCreateDto model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var apartment = await _context.Apartments.FindAsync(id);
                if (apartment == null)
                {
                    return NotFound();
                }

                apartment.Number = model.Number;
                apartment.BuildingId = model.BuildingId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApartmentExists(apartment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index), new { buildingId = model.BuildingId });
            }

            return View(model);
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartment == null)
            {
                return NotFound();
            }

            return View(apartment);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment != null)
            {
                _context.Apartments.Remove(apartment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { buildingId = apartment.BuildingId });
        }

        private bool ApartmentExists(int id)
        {
            return _context.Apartments.Any(e => e.Id == id);
        }
    }
}
