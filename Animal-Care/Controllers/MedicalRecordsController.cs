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
    [Authorize(Roles = "Veterinarian,Admin")]
    public class MedicalRecordsController : Controller
    {
        private readonly AnimalCare2Context _context;

        public MedicalRecordsController(AnimalCare2Context context)
        {
            _context = context;
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index()
        {
            // Base query with includes
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Pet)
                .Include(m => m.Vet)
                    .ThenInclude(v => v.User)
                .AsQueryable();

            // If the logged-in user is a veterinarian, show ONLY their records
            if (User.IsInRole("Veterinarian"))
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdString, out var userId))
                {
                    // In your schema, VetId == User.Id for veterinarians
                    query = query.Where(m => m.VetId == userId);
                }
            }
            // If Admin, don't filter (see everything)

            var records = await query.ToListAsync();
            return View(records);
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
                .Include(m => m.Vet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // GET: MedicalRecords/Create
        public IActionResult Create()
        {
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id");
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name");
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Include(v => v.User),
                "UserId",
                "User.FullName"
            );
            return View();
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

            // Basic check: appointment must exist
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.Id == medicalRecord.AppointmentId);

            if (appointment == null)
            {
                ModelState.AddModelError("AppointmentId", "Selected appointment does not exist.");
            }
            else
            {
                // MEDICAL_RECORD.id must equal APPOINTMENT.id
                medicalRecord.Id = appointment.Id;

                // Optionally make Pet / Vet match the appointment automatically
                medicalRecord.PetId = appointment.PetId;
                medicalRecord.VetId = appointment.VetId;

                // If VisitDate not set, default to appointment date
                if (medicalRecord.VisitDate == default)
                {
                    medicalRecord.VisitDate = appointment.StartTime.Date;
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.AppointmentId);
                ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", medicalRecord.PetId);
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Include(v => v.User),
                    "UserId",
                    "User.FullName",
                    medicalRecord.VetId
                );
                return View(medicalRecord);
            }

            _context.MedicalRecords.Add(medicalRecord);

            // Optional: mark appointment as Complete
            if (appointment != null && appointment.Status == "Scheduled")
            {
                appointment.Status = "Complete";
                _context.Appointments.Update(appointment);
            }

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

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.AppointmentId);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", medicalRecord.PetId);
            ViewData["VetId"] = new SelectList(
                _context.Veterinarians.Include(v => v.User),
                "UserId",
                "User.FullName",
                medicalRecord.VetId
            );
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Appointment");
            ModelState.Remove("IdNavigation");
            ModelState.Remove("Pet");
            ModelState.Remove("Vet");

            // Ensure id still matches appointment
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == medicalRecord.AppointmentId);

            if (appointment == null)
            {
                ModelState.AddModelError("AppointmentId", "Selected appointment does not exist.");
            }
            else
            {
                medicalRecord.Id = appointment.Id;
            }

            if (!ModelState.IsValid)
            {
                ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.AppointmentId);
                ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Name", medicalRecord.PetId);
                ViewData["VetId"] = new SelectList(
                    _context.Veterinarians.Include(v => v.User),
                    "UserId",
                    "User.FullName",
                    medicalRecord.VetId
                );
                return View(medicalRecord);
            }

            try
            {
                _context.Update(medicalRecord);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicalRecordExists(medicalRecord.Id))
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

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MedicalRecords == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Pet)
                .Include(m => m.Vet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicalRecordExists(int id)
        {
            return (_context.MedicalRecords?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
