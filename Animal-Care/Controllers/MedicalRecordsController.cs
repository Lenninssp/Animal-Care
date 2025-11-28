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
            var animalCare2Context = _context.MedicalRecords.Include(m => m.Appointment).Include(m => m.IdNavigation).Include(m => m.Pet).Include(m => m.Vet);
            return View(await animalCare2Context.ToListAsync());
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
                .Include(m => m.IdNavigation)
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
            ViewData["Id"] = new SelectList(_context.Appointments, "Id", "Id");
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id");
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId");
            return View();
        }

        // POST: MedicalRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VisitDate,Diagnosis,Treatment,PetId,VetId,AppointmentId")] MedicalRecord medicalRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicalRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.AppointmentId);
            ViewData["Id"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.Id);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id", medicalRecord.PetId);
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", medicalRecord.VetId);
            return View(medicalRecord);
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
            ViewData["Id"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.Id);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id", medicalRecord.PetId);
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", medicalRecord.VetId);
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VisitDate,Diagnosis,Treatment,PetId,VetId,AppointmentId")] MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.AppointmentId);
            ViewData["Id"] = new SelectList(_context.Appointments, "Id", "Id", medicalRecord.Id);
            ViewData["PetId"] = new SelectList(_context.Pets, "Id", "Id", medicalRecord.PetId);
            ViewData["VetId"] = new SelectList(_context.Veterinarians, "UserId", "UserId", medicalRecord.VetId);
            return View(medicalRecord);
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
                .Include(m => m.IdNavigation)
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
                return Problem("Entity set 'AnimalCare2Context.MedicalRecords'  is null.");
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
