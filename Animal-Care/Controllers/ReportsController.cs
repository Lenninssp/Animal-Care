using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Animal_Care.Models;

namespace Animal_Care.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly AnimalCare2Context _context;

        public ReportsController(AnimalCare2Context context)
        {
            _context = context;
        }

        // /Reports/Monthly?year=2025&month=3
        public async Task<IActionResult> Monthly(int? year, int? month)
        {
            var now = DateTime.Now;

            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;

            var start = new DateTime(selectedYear, selectedMonth, 1);
            var end = start.AddMonths(1);

            // Load appointments for that month, including vet + user for name
            var appointments = await _context.Appointments
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
                .Where(a => a.StartTime >= start && a.StartTime < end)
                .ToListAsync();

            var model = new MonthlyReportViewModel
            {
                Year = selectedYear,
                Month = selectedMonth,
                TotalAppointments = appointments.Count,
                ScheduledCount = appointments.Count(a => a.Status == "Scheduled"),
                CanceledCount = appointments.Count(a => a.Status == "Canceled"),
                CompletedCount = appointments.Count(a => a.Status == "Complete")
            };

            model.VetWorkloads = appointments
                .GroupBy(a => new { a.VetId, VetName = a.Vet.User.FullName })
                .Select(g => new VetWorkloadItem
                {
                    VetId = g.Key.VetId,
                    VetName = g.Key.VetName,
                    TotalAppointments = g.Count(),
                    ScheduledCount = g.Count(a => a.Status == "Scheduled"),
                    CanceledCount = g.Count(a => a.Status == "Canceled"),
                    CompletedCount = g.Count(a => a.Status == "Complete")
                })
                .OrderBy(v => v.VetName)
                .ToList();

            return View(model);
        }
    }
}
