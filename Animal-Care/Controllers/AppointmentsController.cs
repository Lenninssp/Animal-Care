using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Animal_Care.Models;

namespace Animal_Care.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly AnimalCare2Context _context;

        public AppointmentsController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var animalCare2Context = _context.Appointments.Include(a => a.AppointmentType).Include(a => a.Pet).Include(a => a.RecepcionistUser).Include(a => a.Vet);
            return View(await animalCare2Context.ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.AppointmentType)
                .Include(a => a.Pet)
                .Include(a => a.RecepcionistUser)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Id");
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id");
            ViewData["RecepcionistUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartTime,EndTime,Status,PetId,VetId,RecepcionistUserId,AppointmentTypeId,CreatedAt,UpdatedAt,CanceledAt")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Id", appointment.AppointmentTypeId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id", appointment.PetId);
            ViewData["RecepcionistUserId"] = new SelectList(_context.Users, "Id", "Id", appointment.RecepcionistUserId);
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", appointment.VetId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Id", appointment.AppointmentTypeId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id", appointment.PetId);
            ViewData["RecepcionistUserId"] = new SelectList(_context.Users, "Id", "Id", appointment.RecepcionistUserId);
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", appointment.VetId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartTime,EndTime,Status,PetId,VetId,RecepcionistUserId,AppointmentTypeId,CreatedAt,UpdatedAt,CanceledAt")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
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
            ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Id", appointment.AppointmentTypeId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id", appointment.PetId);
            ViewData["RecepcionistUserId"] = new SelectList(_context.Users, "Id", "Id", appointment.RecepcionistUserId);
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", appointment.VetId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.AppointmentType)
                .Include(a => a.Pet)
                .Include(a => a.RecepcionistUser)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Appointments == null)
            {
                return Problem("Entity set 'AnimalCare2Context.Appointments'  is null.");
            }
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
          return (_context.Appointments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
