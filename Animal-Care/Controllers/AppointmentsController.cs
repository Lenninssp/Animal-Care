using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Animal_Care.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
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
            var animalCare2Context = _context.Appointments
                .Include(a => a.AppointmentType)
                .Include(a => a.Pet)
                .Include(a => a.RecepcionistUser)
                .Include(a => a.Vet);
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
            ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Name");
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name");
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Include(v => v.User),
                "UserId",
                "User.FullName"
            );
            // Receptionist is set automatically in POST
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // These navigation properties are not posted by the form, ignore them for validation
            ModelState.Remove("Pet");
            ModelState.Remove("Vet");
            ModelState.Remove("AppointmentType");
            ModelState.Remove("RecepcionistUser");
            ModelState.Remove("MedicalRecordAppointments");
            ModelState.Remove("MedicalRecordIdNavigation");

            // We set the receptionist in code, not from the form
            ModelState.Remove("RecepcionistUserId");

            // Default status if empty
            if (string.IsNullOrWhiteSpace(appointment.Status))
            {
                appointment.Status = "Scheduled";
            }

            // Basic validation: EndTime must be after StartTime
            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            // Check clinic hours
            var dayName = appointment.StartTime.DayOfWeek.ToString(); // "Monday", etc.
            var clinicHours = await _context.ClinicHours
                .FirstOrDefaultAsync(ch => ch.DayOfWeek == dayName);

            if (clinicHours == null)
            {
                ModelState.AddModelError("", "Clinic hours are not configured for this day.");
            }
            else
            {
                var startTime = appointment.StartTime.TimeOfDay;
                var endTime = appointment.EndTime.TimeOfDay;

                if (startTime < clinicHours.OpenTime || endTime > clinicHours.CloseTime)
                {
                    ModelState.AddModelError("", "Appointment is outside clinic opening hours.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Name", appointment.AppointmentTypeId);
                ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", appointment.PetId);
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Include(v => v.User),
                    "UserId",
                    "User.FullName",
                    appointment.VetId
                );
                return View(appointment);
            }

            // Set receptionist as currently logged-in user
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out var userId))
            {
                appointment.RecepcionistUserId = userId;
            }

            // Set timestamps
            appointment.CreatedAt = DateTime.Now;
            appointment.UpdatedAt = DateTime.Now;

            _context.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
            ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Name", appointment.AppointmentTypeId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", appointment.PetId);
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Include(v => v.User),
                "UserId",
                "User.FullName",
                appointment.VetId
            );
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            // Same navigation props ignore
            ModelState.Remove("Pet");
            ModelState.Remove("Vet");
            ModelState.Remove("AppointmentType");
            ModelState.Remove("RecepcionistUser");
            ModelState.Remove("MedicalRecordAppointments");
            ModelState.Remove("MedicalRecordIdNavigation");

            // Basic validation
            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            var dayName = appointment.StartTime.DayOfWeek.ToString();
            var clinicHours = await _context.ClinicHours
                .FirstOrDefaultAsync(ch => ch.DayOfWeek == dayName);

            if (clinicHours != null)
            {
                var startTime = appointment.StartTime.TimeOfDay;
                var endTime = appointment.EndTime.TimeOfDay;

                if (startTime < clinicHours.OpenTime || endTime > clinicHours.CloseTime)
                {
                    ModelState.AddModelError("", "Appointment is outside clinic opening hours.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["AppointmentTypeId"] = new SelectList(_context.AppointmentTypes, "Id", "Name", appointment.AppointmentTypeId);
                ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", appointment.PetId);
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Include(v => v.User),
                    "UserId",
                    "User.FullName",
                    appointment.VetId
                );
                return View(appointment);
            }

            try
            {
                // update timestamp
                appointment.UpdatedAt = DateTime.Now;

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
                return Problem("Entity set 'AnimalCare2Context.Appointments' is null.");
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
