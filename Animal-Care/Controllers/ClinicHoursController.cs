using System;
using System.Collections.Generic;
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
    public class ClinicHoursController : Controller
    {
        private readonly AnimalCare2Context _context;

        public ClinicHoursController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: ClinicHours
        public async Task<IActionResult> Index()
        {
              return _context.ClinicHours != null ? 
                          View(await _context.ClinicHours.ToListAsync()) :
                          Problem("Entity set 'AnimalCare2Context.ClinicHours'  is null.");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DayOfWeek,OpenTime,CloseTime")] ClinicHour clinicHour)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clinicHour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(clinicHour);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DayOfWeek,OpenTime,CloseTime")] ClinicHour clinicHour)
        {
            if (id != clinicHour.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            return View(clinicHour);
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
                return Problem("Entity set 'AnimalCare2Context.ClinicHours'  is null.");
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
    }
}
