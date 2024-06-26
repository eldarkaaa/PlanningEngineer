using PlanningEngineerApplication.Data;
using PlanningEngineerApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlanningEngineerApplication.Controllers
{
    [Authorize]
    public class BuildingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuildingsController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["AddressSortParm"] = String.IsNullOrEmpty(sortOrder) ? "address_desc" : "";
            ViewData["MaterialSortParm"] = sortOrder == "Material" ? "material_desc" : "Material";
            ViewData["CitySortParm"] = sortOrder == "City" ? "city_desc" : "City";
            ViewData["NumberOfApartmentsSortParm"] = sortOrder == "NumberOfApartments" ? "number_desc" : "NumberOfApartments";

            var buildings = from b in _context.Buildings select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                buildings = buildings.Where(b => b.Address.Contains(searchString) || b.City.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "address_desc":
                    buildings = buildings.OrderByDescending(b => b.Address);
                    break;
                case "Material":
                    buildings = buildings.OrderBy(b => b.Material);
                    break;
                case "material_desc":
                    buildings = buildings.OrderByDescending(b => b.Material);
                    break;
                case "City":
                    buildings = buildings.OrderBy(b => b.City);
                    break;
                case "city_desc":
                    buildings = buildings.OrderByDescending(b => b.City);
                    break;
                case "NumberOfApartments":
                    buildings = buildings.OrderBy(b => b.NumberOfApartments);
                    break;
                case "number_desc":
                    buildings = buildings.OrderByDescending(b => b.NumberOfApartments);
                    break;
                default:
                    buildings = buildings.OrderBy(b => b.Address);
                    break;
            }

            return View(await buildings.AsNoTracking().ToListAsync());
        }

        
        [Authorize(Roles = "Admin, Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BuildingCreateDto buildingDto)
        {
            if (ModelState.IsValid)
            {
                var building = new Building
                {
                    Address = buildingDto.Address,
                    Material = buildingDto.Material,
                    City = buildingDto.City,
                    NumberOfApartments = buildingDto.NumberOfApartments
                };

                _context.Add(building);
                await _context.SaveChangesAsync();

               
                for (int i = 1; i <= buildingDto.NumberOfApartments; i++)
                {
                    var apartment = new Apartment
                    {
                        Number = i,
                        BuildingId = building.Id
                    };
                    _context.Apartments.Add(apartment);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(buildingDto);
        }

        
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Edit(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }

            var model = new BuildingCreateDto
            {
                Id = building.Id,
                Address = building.Address,
                Material = building.Material,
                City = building.City,
                NumberOfApartments = building.NumberOfApartments
            };

            return View(model);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Edit(int id, BuildingCreateDto model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var building = await _context.Buildings.FindAsync(id);
                if (building == null)
                {
                    return NotFound();
                }

                building.Address = model.Address;
                building.Material = model.Material;
                building.City = model.City;
                building.NumberOfApartments = model.NumberOfApartments;

               
                var existingApartments = await _context.Apartments.Where(a => a.BuildingId == id).ToListAsync();
                if (existingApartments.Count < model.NumberOfApartments)
                {
                    for (int i = existingApartments.Count + 1; i <= model.NumberOfApartments; i++)
                    {
                        var apartment = new Apartment
                        {
                            Number = i,
                            BuildingId = id
                        };
                        _context.Apartments.Add(apartment);
                    }
                }
                else if (existingApartments.Count > model.NumberOfApartments)
                {
                    var apartmentsToRemove = existingApartments.Skip(model.NumberOfApartments).ToList();
                    _context.Apartments.RemoveRange(apartmentsToRemove);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingExists(building.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private bool BuildingExists(int id)
        {
            return _context.Buildings.Any(e => e.Id == id);
        }

        
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings
                .FirstOrDefaultAsync(m => m.Id == id);

            if (building == null)
            {
                return NotFound();
            }

            return View(building);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building != null)
            {
                _context.Buildings.Remove(building);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
