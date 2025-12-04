using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Animal_Care.Models;
using Microsoft.AspNetCore.Authorization;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VeterinariansController : Controller
    {
        private readonly AnimalCare2Context _context;

        public VeterinariansController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: Veterinarians
        public async Task<IActionResult> Index()
        {
            var vets = _context.Veterinarians
                .Include(v => v.User); // so we can show FullName, Email, etc.

            return View(await vets.ToListAsync());
        }

        // GET: Veterinarians/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Veterinarians == null)
            {
                return NotFound();
            }

            var veterinarian = await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (veterinarian == null)
            {
                return NotFound();
            }

            return View(veterinarian);
        }

        // GET: Veterinarians/Create
        public IActionResult Create()
        {
            // Only users that already have the "Veterinarian" role
            ViewData["UserId"] = new SelectList(
                _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.Name == "Veterinarian"),
                "Id",
                "FullName"
            );

            return View();
        }

        // POST: Veterinarians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Veterinarian veterinarian)
        {
            // Ignore navigation property
            ModelState.Remove("User");
            ModelState.Remove("Appointments");
            ModelState.Remove("MedicalRecords");
            ModelState.Remove("VetSchedules");

            if (!ModelState.IsValid)
            {
                ViewData["UserId"] = new SelectList(
                    _context.Users
                        .Include(u => u.Role)
                        .Where(u => u.Role.Name == "Veterinarian"),
                    "Id",
                    "FullName",
                    veterinarian.UserId
                );

                return View(veterinarian);
            }

            _context.Add(veterinarian);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Veterinarians/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Veterinarians == null)
            {
                return NotFound();
            }

            var veterinarian = await _context.Veterinarians.FindAsync(id);
            if (veterinarian == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = new SelectList(
                _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.Name == "Veterinarian"),
                "Id",
                "FullName",
                veterinarian.UserId
            );

            return View(veterinarian);
        }

        // POST: Veterinarians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Veterinarian veterinarian)
        {
            if (id != veterinarian.UserId)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("Appointments");
            ModelState.Remove("MedicalRecords");
            ModelState.Remove("VetSchedules");

            if (!ModelState.IsValid)
            {
                ViewData["UserId"] = new SelectList(
                    _context.Users
                        .Include(u => u.Role)
                        .Where(u => u.Role.Name == "Veterinarian"),
                    "Id",
                    "FullName",
                    veterinarian.UserId
                );

                return View(veterinarian);
            }

            try
            {
                _context.Update(veterinarian);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VeterinarianExists(veterinarian.UserId))
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

        // GET: Veterinarians/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Veterinarians == null)
            {
                return NotFound();
            }

            var veterinarian = await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (veterinarian == null)
            {
                return NotFound();
            }

            return View(veterinarian);
        }

        // POST: Veterinarians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Veterinarians == null)
            {
                return Problem("Entity set 'AnimalCare2Context.Veterinarians'  is null.");
            }

            var veterinarian = await _context.Veterinarians.FindAsync(id);
            if (veterinarian != null)
            {
                _context.Veterinarians.Remove(veterinarian);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VeterinarianExists(int id)
        {
            return (_context.Veterinarians?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
