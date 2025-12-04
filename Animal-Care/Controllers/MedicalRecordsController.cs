using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Animal_Care.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin,Veterinarian")]
    public class MedicalRecordsController : Controller
    {
        private readonly AnimalCare2Context _context;

        public MedicalRecordsController(AnimalCare2Context context)
        {
            _context = context;
        }

        // Helper: get current logged-in user id (int?) from claims
        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }

        // Helper: get veterinarian linked to current user (if any)
        private async Task<Veterinarian?> GetCurrentVeterinarianAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;

            return await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == userId.Value);
        }

        // Helper: build the appointment dropdown, filtered by role and excluding appointments
        // that already have a MedicalRecord.
        private async Task PopulateAppointmentDropdownAsync(int? selectedAppointmentId = null)
        {
            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User);

            // Veterinarian: only their own appointments
            if (User.IsInRole("Veterinarian"))
            {
                var vet = await GetCurrentVeterinarianAsync();
                if (vet != null)
                {
                    query = query.Where(a => a.VetId == vet.UserId);
                }
                else
                {
                    // If somehow no Vet row for this user, show nothing
                    query = query.Where(a => false);
                }
            }

            // Optional: only appointments that are "Complete"
            query = query.Where(a => a.Status == "Complete");

            // Exclude appointments that already have a medical record
            query = query.Where(a => !_context.MedicalRecords.Any(m => m.AppointmentId == a.Id));

            var appointmentsList = await query
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            var appointmentItems = appointmentsList
                .Select(a => new
                {
                    a.Id,
                    Display = $"{a.StartTime:g} - {a.Pet.Name} ({a.Pet.Species}) - Dr. {a.Vet.User.FullName}"
                })
                .ToList();

            ViewData["AppointmentId"] = new SelectList(appointmentItems, "Id", "Display", selectedAppointmentId);
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index()
        {
            var records = _context.MedicalRecords
                .Include(m => m.Pet)
                .Include(m => m.Vet).ThenInclude(v => v.User)
                .Include(m => m.Appointment);

            // Veterinarian: only see their own records
            if (User.IsInRole("Veterinarian"))
            {
                var vet = await GetCurrentVeterinarianAsync();
                if (vet != null)
                {
                    return View(await records
                        .Where(m => m.VetId == vet.UserId)
                        .OrderByDescending(m => m.VisitDate)
                        .ToListAsync());
                }

                // No veterinarian row for this user => no records
                return View(Enumerable.Empty<MedicalRecord>());
            }

            // Admin: see all
            return View(await records
                .OrderByDescending(m => m.VisitDate)
                .ToListAsync());
        }

        // GET: MedicalRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MedicalRecords == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Pet)
                .Include(m => m.Vet).ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            // Veterinarian can only see their own records
            if (User.IsInRole("Veterinarian"))
            {
                var vet = await GetCurrentVeterinarianAsync();
                if (vet == null || medicalRecord.VetId != vet.UserId)
                {
                    return Forbid();
                }
            }

            return View(medicalRecord);
        }

        // GET: MedicalRecords/Create
        public async Task<IActionResult> Create()
        {
            await PopulateAppointmentDropdownAsync();

            // Default visit date: today
            var model = new MedicalRecord
            {
                VisitDate = DateTime.Today
            };

            return View(model);
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecord medicalRecord)
        {
            // Ignore navigation properties for validation
            ModelState.Remove("Appointment");
            ModelState.Remove("IdNavigation");
            ModelState.Remove("Pet");
            ModelState.Remove("Vet");

            // We set Id, PetId, VetId from the appointment
            ModelState.Remove("Id");
            ModelState.Remove("PetId");
            ModelState.Remove("VetId");

            // Validate appointment
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(a => a.Id == medicalRecord.AppointmentId);

            if (appointment == null)
            {
                ModelState.AddModelError("AppointmentId", "Selected appointment does not exist.");
            }
            else
            {
                // Veterinarian: must only create records for their own appointments
                if (User.IsInRole("Veterinarian"))
                {
                    var vet = await GetCurrentVeterinarianAsync();
                    if (vet == null || appointment.VetId != vet.UserId)
                    {
                        ModelState.AddModelError("AppointmentId", "You can only create medical records for your own appointments.");
                    }
                }

                // Check that appointment does not already have a medical record
                var alreadyHasRecord = await _context.MedicalRecords
                    .AnyAsync(m => m.AppointmentId == appointment.Id);

                if (alreadyHasRecord)
                {
                    // This is the crash you were seeing – now we block it gracefully.
                    ModelState.AddModelError("AppointmentId", "This appointment already has a medical record.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateAppointmentDropdownAsync(medicalRecord.AppointmentId);
                return View(medicalRecord);
            }

            // Now safe: set keys based on appointment (1:1 relationship)
            medicalRecord.Id = appointment!.Id;          // PK = Appointment.Id
            medicalRecord.PetId = appointment.PetId;     // same pet as appointment
            medicalRecord.VetId = appointment.VetId;     // same vet as appointment

            // If no visit date provided, default to appointment date
            if (medicalRecord.VisitDate == default)
            {
                medicalRecord.VisitDate = appointment.StartTime.Date;
            }

            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MedicalRecords == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Pet)
                .Include(m => m.Vet).ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Veterinarian"))
            {
                var vet = await GetCurrentVeterinarianAsync();
                if (vet == null || medicalRecord.VetId != vet.UserId)
                {
                    return Forbid();
                }
            }

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalRecord formModel)
        {
            if (id != formModel.Id)
            {
                return NotFound();
            }

            // Ignore navigation properties
            ModelState.Remove("Appointment");
            ModelState.Remove("IdNavigation");
            ModelState.Remove("Pet");
            ModelState.Remove("Vet");
            ModelState.Remove("PetId");
            ModelState.Remove("VetId");
            ModelState.Remove("AppointmentId");

            if (!ModelState.IsValid)
            {
                // Reload full entity for the view
                var reload = await _context.MedicalRecords
                    .Include(m => m.Appointment)
                    .Include(m => m.Pet)
                    .Include(m => m.Vet).ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (reload == null)
                    return NotFound();

                return View(reload);
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Vet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Veterinarian"))
            {
                var vet = await GetCurrentVeterinarianAsync();
                if (vet == null || medicalRecord.VetId != vet.UserId)
                {
                    return Forbid();
                }
            }

            // Only update editable fields
            medicalRecord.VisitDate = formModel.VisitDate;
            medicalRecord.Diagnosis = formModel.Diagnosis;
            medicalRecord.Treatment = formModel.Treatment;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: MedicalRecords/Delete/5
        // (Optional) Only Admin should delete records
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MedicalRecords == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Pet)
                .Include(m => m.Vet).ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MedicalRecords == null)
            {
                return Problem("Entity set 'AnimalCare2Context.MedicalRecords' is null.");
            }

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
            {
                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MedicalRecordExists(int id)
        {
            return (_context.MedicalRecords?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
