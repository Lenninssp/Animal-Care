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
            var animalCare2Context = _context.Veterinarians.Include(v => v.User);
            return View(await animalCare2Context.ToListAsync());
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
            // Show only users that have the "Veterinarian" role, display FullName
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
        public async Task<IActionResult> Create([Bind("UserId,Specialty")] Veterinarian veterinarian)
        {
            // Ignore navigation property 'User' during validation
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Add(veterinarian);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Specialty")] Veterinarian veterinarian)
        {
            if (id != veterinarian.UserId)
            {
                return NotFound();
            }

            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
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
