using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Animal_Care.Models;
using Microsoft.AspNetCore.Authorization;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClinicHoursController : Controller
    {
        private readonly AnimalCare2Context _context;

        private static readonly string[] ValidDays = new[]
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        public ClinicHoursController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: ClinicHours
        public async Task<IActionResult> Index()
        {
            if (_context.ClinicHours == null)
                return Problem("Entity set 'AnimalCare2Context.ClinicHours' is null.");

            // Order by weekday if you want; simplest is by Id or by DayOfWeek
            var hours = await _context.ClinicHours
                .OrderBy(ch => ch.Id)
                .ToListAsync();

            return View(hours);
        }

        // GET: ClinicHours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ClinicHours == null)
            {
                return NotFound();
            }

            var clinicHour = await _context.ClinicHours
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clinicHour == null)
            {
                return NotFound();
            }

            return View(clinicHour);
        }

        // GET: ClinicHours/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ClinicHours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClinicHour clinicHour)
        {
            // Basic validation
            ValidateClinicHour(clinicHour, isEdit: false);

            if (!ModelState.IsValid)
            {
                return View(clinicHour);
            }

            _context.Add(clinicHour);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ClinicHours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ClinicHours == null)
            {
                return NotFound();
            }

            var clinicHour = await _context.ClinicHours.FindAsync(id);
            if (clinicHour == null)
            {
                return NotFound();
            }
            return View(clinicHour);
        }

        // POST: ClinicHours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClinicHour clinicHour)
        {
            if (id != clinicHour.Id)
            {
                return NotFound();
            }

            ValidateClinicHour(clinicHour, isEdit: true);

            if (!ModelState.IsValid)
            {
                return View(clinicHour);
            }

            try
            {
                _context.Update(clinicHour);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicHourExists(clinicHour.Id))
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

        // GET: ClinicHours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ClinicHours == null)
            {
                return NotFound();
            }

            var clinicHour = await _context.ClinicHours
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clinicHour == null)
            {
                return NotFound();
            }

            return View(clinicHour);
        }

        // POST: ClinicHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ClinicHours == null)
            {
                return Problem("Entity set 'AnimalCare2Context.ClinicHours' is null.");
            }

            var clinicHour = await _context.ClinicHours.FindAsync(id);
            if (clinicHour != null)
            {
                _context.ClinicHours.Remove(clinicHour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClinicHourExists(int id)
        {
            return (_context.ClinicHours?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private void ValidateClinicHour(ClinicHour clinicHour, bool isEdit)
        {
            // Day required and must be valid
            if (string.IsNullOrWhiteSpace(clinicHour.DayOfWeek))
            {
                ModelState.AddModelError(nameof(ClinicHour.DayOfWeek), "Day of week is required.");
            }
            else if (!ValidDays.Contains(clinicHour.DayOfWeek))
            {
                ModelState.AddModelError(nameof(ClinicHour.DayOfWeek), "Invalid day of week.");
            }

            // Open < Close
            if (clinicHour.CloseTime <= clinicHour.OpenTime)
            {
                ModelState.AddModelError("", "Closing time must be after opening time.");
            }

            // Optional: prevent duplicate day entries
            if (_context.ClinicHours != null &&
                !string.IsNullOrWhiteSpace(clinicHour.DayOfWeek))
            {
                var duplicate = _context.ClinicHours
                    .Any(ch =>
                        ch.DayOfWeek == clinicHour.DayOfWeek &&
                        (!isEdit || ch.Id != clinicHour.Id));

                if (duplicate)
                {
                    ModelState.AddModelError(nameof(ClinicHour.DayOfWeek),
                        "Clinic hours for this day already exist.");
                }
            }
        }
    }
}
