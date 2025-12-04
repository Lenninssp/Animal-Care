using System;
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
    public class VetSchedulesController : Controller
    {
        private readonly AnimalCare2Context _context;

        public VetSchedulesController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: VetSchedules
        public async Task<IActionResult> Index()
        {
            var schedules = _context.VetSchedules
                .Include(vs => vs.Vet)
                    .ThenInclude(v => v.User);   // so you can show vet name in the view

            return View(await schedules.ToListAsync());
        }

        // GET: VetSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VetSchedules == null)
            {
                return NotFound();
            }

            var vetSchedule = await _context.VetSchedules
                .Include(vs => vs.Vet)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vetSchedule == null)
            {
                return NotFound();
            }

            return View(vetSchedule);
        }

        // GET: VetSchedules/Create
        public IActionResult Create()
        {
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Include(v => v.User),
                "UserId",
                "User.FullName"
            );
            return View();
        }

        // POST: VetSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VetSchedule vetSchedule)
        {
            // Ignore navigation property
            ModelState.Remove("Vet");

            // Basic validation: end after start
            if (vetSchedule.EndTime <= vetSchedule.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Include(v => v.User),
                    "UserId",
                    "User.FullName",
                    vetSchedule.VetId
                );
                return View(vetSchedule);
            }

            _context.Add(vetSchedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: VetSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VetSchedules == null)
            {
                return NotFound();
            }

            var vetSchedule = await _context.VetSchedules.FindAsync(id);
            if (vetSchedule == null)
            {
                return NotFound();
            }

            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Include(v => v.User),
                "UserId",
                "User.FullName",
                vetSchedule.VetId
            );
            return View(vetSchedule);
        }

        // POST: VetSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VetSchedule vetSchedule)
        {
            if (id != vetSchedule.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Vet");

            if (vetSchedule.EndTime <= vetSchedule.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Include(v => v.User),
                    "UserId",
                    "User.FullName",
                    vetSchedule.VetId
                );
                return View(vetSchedule);
            }

            try
            {
                _context.Update(vetSchedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VetScheduleExists(vetSchedule.Id))
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

        // GET: VetSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VetSchedules == null)
            {
                return NotFound();
            }

            var vetSchedule = await _context.VetSchedules
                .Include(vs => vs.Vet)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vetSchedule == null)
            {
                return NotFound();
            }

            return View(vetSchedule);
        }

        // POST: VetSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VetSchedules == null)
            {
                return Problem("Entity set 'AnimalCare2Context.VetSchedules' is null.");
            }
            var vetSchedule = await _context.VetSchedules.FindAsync(id);
            if (vetSchedule != null)
            {
                _context.VetSchedules.Remove(vetSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VetScheduleExists(int id)
        {
            return (_context.VetSchedules?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
