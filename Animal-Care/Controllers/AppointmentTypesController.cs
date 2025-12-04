using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Animal_Care.Models;
using Microsoft.AspNetCore.Authorization;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppointmentTypesController : Controller
    {
        private readonly AnimalCare2Context _context;

        public AppointmentTypesController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: AppointmentTypes
        public async Task<IActionResult> Index()
        {
            if (_context.AppointmentTypes == null)
            {
                return Problem("Entity set 'AnimalCare2Context.AppointmentTypes' is null.");
            }

            var types = await _context.AppointmentTypes
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(types);
        }

        // GET: AppointmentTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AppointmentTypes == null)
            {
                return NotFound();
            }

            var appointmentType = await _context.AppointmentTypes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointmentType == null)
            {
                return NotFound();
            }

            return View(appointmentType);
        }

        // GET: AppointmentTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppointmentTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentType appointmentType)
        {
            // Simple validation for duration
            if (appointmentType.DurationMinutes <= 0)
            {
                ModelState.AddModelError(nameof(appointmentType.DurationMinutes),
                    "Duration must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                return View(appointmentType);
            }

            _context.Add(appointmentType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: AppointmentTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AppointmentTypes == null)
            {
                return NotFound();
            }

            var appointmentType = await _context.AppointmentTypes.FindAsync(id);
            if (appointmentType == null)
            {
                return NotFound();
            }

            return View(appointmentType);
        }

        // POST: AppointmentTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentType appointmentType)
        {
            if (id != appointmentType.Id)
            {
                return NotFound();
            }

            if (appointmentType.DurationMinutes <= 0)
            {
                ModelState.AddModelError(nameof(appointmentType.DurationMinutes),
                    "Duration must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                return View(appointmentType);
            }

            try
            {
                _context.Update(appointmentType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentTypeExists(appointmentType.Id))
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

        // GET: AppointmentTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AppointmentTypes == null)
            {
                return NotFound();
            }

            var appointmentType = await _context.AppointmentTypes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointmentType == null)
            {
                return NotFound();
            }

            return View(appointmentType);
        }

        // POST: AppointmentTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AppointmentTypes == null)
            {
                return Problem("Entity set 'AnimalCare2Context.AppointmentTypes' is null.");
            }

            var appointmentType = await _context.AppointmentTypes.FindAsync(id);
            if (appointmentType != null)
            {
                _context.AppointmentTypes.Remove(appointmentType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentTypeExists(int id)
        {
            return (_context.AppointmentTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
