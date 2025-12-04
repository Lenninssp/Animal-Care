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
            var appointments = _context.Appointments
                .Include(a => a.AppointmentType)
                .Include(a => a.Pet)
                    .ThenInclude(p => p.Owner)
                .Include(a => a.RecepcionistUser)
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
                .OrderBy(a => a.StartTime);

            return View(await appointments.ToListAsync());
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
                    .ThenInclude(p => p.Owner)
                .Include(a => a.RecepcionistUser)
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
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

            // For display: vet schedules and upcoming appointments
            LoadAvailabilityDataForView();

            // Receptionist is set automatically in POST
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Navigation props not posted by the form
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

            var dayName = appointment.StartTime.DayOfWeek.ToString(); // "Monday", etc.

            // 1) Check clinic hours (global)
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

            // 2) Check vet's personal schedule
            var vetSchedules = await _context.VetSchedules
                .Where(vs => vs.VetId == appointment.VetId && vs.DayOfWeek == dayName)
                .ToListAsync();

            if (!vetSchedules.Any())
            {
                ModelState.AddModelError("", "This veterinarian has no schedule defined for that day.");
            }
            else
            {
                var apptStart = appointment.StartTime.TimeOfDay;
                var apptEnd = appointment.EndTime.TimeOfDay;

                var fitsInSomeSchedule = vetSchedules.Any(vs =>
                    apptStart >= vs.StartTime && apptEnd <= vs.EndTime
                );

                if (!fitsInSomeSchedule)
                {
                    ModelState.AddModelError("", "Appointment is outside this veterinarian's working hours.");
                }
            }

            // 3) Check overlapping appointments for this vet
            var overlaps = await _context.Appointments
                .Where(a => a.VetId == appointment.VetId
                            && a.Status != "Canceled"
                            && a.StartTime < appointment.EndTime
                            && a.EndTime > appointment.StartTime)
                .AnyAsync();

            if (overlaps)
            {
                ModelState.AddModelError("", "This veterinarian already has an appointment in that time range.");
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

                LoadAvailabilityDataForView();
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

            LoadAvailabilityDataForView();
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

            // Ignore navigation props + system fields for validation
            ModelState.Remove("Pet");
            ModelState.Remove("Vet");
            ModelState.Remove("AppointmentType");
            ModelState.Remove("RecepcionistUser");
            ModelState.Remove("MedicalRecordAppointments");
            ModelState.Remove("MedicalRecordIdNavigation");
            ModelState.Remove("RecepcionistUserId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            // Default status if somehow empty
            if (string.IsNullOrWhiteSpace(appointment.Status))
            {
                appointment.Status = "Scheduled";
            }

            // Basic validation
            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            var dayName = appointment.StartTime.DayOfWeek.ToString();

            // 1) Clinic hours
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

            // 2) Vet schedule
            var vetSchedules = await _context.VetSchedules
                .Where(vs => vs.VetId == appointment.VetId && vs.DayOfWeek == dayName)
                .ToListAsync();

            if (!vetSchedules.Any())
            {
                ModelState.AddModelError("", "This veterinarian has no schedule defined for that day.");
            }
            else
            {
                var apptStart = appointment.StartTime.TimeOfDay;
                var apptEnd = appointment.EndTime.TimeOfDay;

                var fitsInSomeSchedule = vetSchedules.Any(vs =>
                    apptStart >= vs.StartTime && apptEnd <= vs.EndTime
                );

                if (!fitsInSomeSchedule)
                {
                    ModelState.AddModelError("", "Appointment is outside this veterinarian's working hours.");
                }
            }

            // 3) Overlaps — exclude this same appointment
            var overlaps = await _context.Appointments
                .Where(a => a.VetId == appointment.VetId
                            && a.Id != appointment.Id
                            && a.Status != "Canceled"
                            && a.StartTime < appointment.EndTime
                            && a.EndTime > appointment.StartTime)
                .AnyAsync();

            if (overlaps)
            {
                ModelState.AddModelError("", "This veterinarian already has an appointment in that time range.");
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

                LoadAvailabilityDataForView();
                return View(appointment);
            }

            // Load existing entity and update only allowed fields
            var existing = await _context.Appointments.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.StartTime = appointment.StartTime;
            existing.EndTime = appointment.EndTime;
            existing.Status = appointment.Status;
            existing.PetId = appointment.PetId;
            existing.VetId = appointment.VetId;
            existing.AppointmentTypeId = appointment.AppointmentTypeId;

            // Handle canceled flag
            if (appointment.Status == "Canceled")
            {
                if (existing.CanceledAt == null)
                {
                    existing.CanceledAt = DateTime.Now;
                }
            }
            else
            {
                existing.CanceledAt = null;
            }

            existing.UpdatedAt = DateTime.Now;

            try
            {
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
                    .ThenInclude(p => p.Owner)
                .Include(a => a.RecepcionistUser)
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
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
                // Soft delete: mark as canceled instead of removing
                appointment.Status = "Canceled";
                appointment.CanceledAt = DateTime.Now;
                appointment.UpdatedAt = DateTime.Now;

                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return (_context.Appointments?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Loads vet schedules and upcoming appointments into ViewBag
        /// to help receptionists see availability.
        /// </summary>
        private void LoadAvailabilityDataForView()
        {
            var vetSchedules = _context.VetSchedules
                .Include(vs => vs.Vet)
                    .ThenInclude(v => v.User)
                .OrderBy(vs => vs.Vet.User.FullName)
                .ThenBy(vs => vs.DayOfWeek)
                .ThenBy(vs => vs.StartTime)
                .ToList();

            var upcomingAppointments = _context.Appointments
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
                .Include(a => a.Pet)
                .Where(a => a.Status != "Canceled" && a.StartTime >= DateTime.Today)
                .OrderBy(a => a.StartTime)
                .Take(20)
                .ToList();

            ViewBag.VetSchedules = vetSchedules;
            ViewBag.UpcomingAppointments = upcomingAppointments;
        }
    }
}
