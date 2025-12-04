using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Animal_Care.Models;
using Microsoft.AspNetCore.Authorization;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class PetsController : Controller
    {
        private readonly AnimalCare2Context _context;

        public PetsController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: Pets
        public async Task<IActionResult> Index()
        {
            var animalCare2Context = _context.Pets.Include(p => p.Owner);
            return View(await animalCare2Context.ToListAsync());
        }

        // GET: Pets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pets == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: Pets/Create
        public IActionResult Create()
        {
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name");
            return View();
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            ModelState.Remove("Owner");

            if (!ModelState.IsValid)
            {
                ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name", pet.OwnerId);
                return View(pet);
            }

            try
            {
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error saving pet: {ex.Message}");
                ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name", pet.OwnerId);
                return View(pet);
            }
        }

        // GET: Pets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pets == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name", pet.OwnerId);
            return View(pet);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet)
        {
            if (id != pet.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Owner");

            if (!ModelState.IsValid)
            {
                ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name", pet.OwnerId);
                return View(pet);
            }

            try
            {
                _context.Update(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetExists(pet.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Pets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pets == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: Pets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pets == null)
            {
                return Problem("Entity set 'AnimalCare2Context.Pets' is null.");
            }

            var pet = await _context.Pets
                .Include(p => p.Appointments)
                .Include(p => p.MedicalRecords)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // ❌ Block delete if there are related rows
            if (pet.Appointments.Any() || pet.MedicalRecords.Any())
            {
                TempData["ErrorMessage"] =
                    "Cannot delete this pet because it has existing appointments or medical records.";
                return RedirectToAction(nameof(Details), new { id = pet.Id });
            }

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PetExists(int id)
        {
            return (_context.Pets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
