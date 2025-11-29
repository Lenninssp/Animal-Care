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
            var animalCare2Context = _context.VetSchedules.Include(v => v.Vet);
            return View(await animalCare2Context.ToListAsync());
        }

        // GET: VetSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.VetSchedules == null)
            {
                return NotFound();
            }

            var vetSchedule = await _context.VetSchedules
                .Include(v => v.Vet)
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
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId");
            return View();
        }

        // POST: VetSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VetId,DayOfWeek,StartTime,EndTime")] VetSchedule vetSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vetSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", vetSchedule.VetId);
            return View(vetSchedule);
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
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", vetSchedule.VetId);
            return View(vetSchedule);
        }

        // POST: VetSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VetId,DayOfWeek,StartTime,EndTime")] VetSchedule vetSchedule)
        {
            if (id != vetSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", vetSchedule.VetId);
            return View(vetSchedule);
        }

        // GET: VetSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VetSchedules == null)
            {
                return NotFound();
            }

            var vetSchedule = await _context.VetSchedules
                .Include(v => v.Vet)
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
                return Problem("Entity set 'AnimalCare2Context.VetSchedules'  is null.");
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
