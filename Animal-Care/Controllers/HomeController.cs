using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Animal_Care.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Animal_Care.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AnimalCare2Context _context;

        public HomeController(ILogger<HomeController> logger, AnimalCare2Context context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeDashboardViewModel
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            };

            if (!model.IsAuthenticated)
            {
                // Anonymous: simple marketing/description section only
                return View(model);
            }

            if (User.IsInRole("Veterinarian"))
            {
                await PopulateVeterinarianDashboardAsync(model);
            }
            else if (User.IsInRole("Receptionist"))
            {
                await PopulateReceptionistDashboardAsync(model);
            }
            else if (User.IsInRole("Admin"))
            {
                await PopulateAdminDashboardAsync(model);
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // ========== Helpers ==========

        private int? GetCurrentUserId()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idString, out var id))
            {
                return id;
            }
            return null;
        }

        private async Task PopulateVeterinarianDashboardAsync(HomeDashboardViewModel model)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                model.VetUpcomingAppointments = new();
                return;
            }

            var today = DateTime.Today;

            model.VetUpcomingAppointments = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p.Owner)
                .Include(a => a.AppointmentType)
                .Where(a =>
                    a.Status != "Canceled" &&
                    a.VetId == currentUserId.Value &&
                    a.StartTime >= today)
                .OrderBy(a => a.StartTime)
                .Select(a => new AppointmentListItem
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    PetName = a.Pet.Name,
                    OwnerName = a.Pet.Owner.Name,
                    TypeName = a.AppointmentType.Name,
                    Status = a.Status
                })
                .Take(10)
                .ToListAsync();
        }

        private async Task PopulateReceptionistDashboardAsync(HomeDashboardViewModel model)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            model.TodayAppointments = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p.Owner)
                .Include(a => a.Vet)
                    .ThenInclude(v => v.User)
                .Include(a => a.AppointmentType)
                .Where(a =>
                    a.Status != "Canceled" &&
                    a.StartTime >= today &&
                    a.StartTime < tomorrow)
                .OrderBy(a => a.StartTime)
                .Select(a => new AppointmentListItem
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    PetName = a.Pet.Name,
                    OwnerName = a.Pet.Owner.Name,
                    VetName = a.Vet.User.FullName,
                    TypeName = a.AppointmentType.Name,
                    Status = a.Status
                })
                .ToListAsync();
        }

        private async Task PopulateAdminDashboardAsync(HomeDashboardViewModel model)
        {
            // Overview counts
            model.TotalOwners = await _context.Owners.CountAsync();
            model.TotalPets = await _context.Pets.CountAsync();
            model.TotalUsers = await _context.Users.CountAsync();

            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, 1);
            var end = start.AddMonths(1);

            var monthAppointments = await _context.Appointments
                .Where(a => a.StartTime >= start && a.StartTime < end)
                .ToListAsync();

            model.TotalAppointmentsThisMonth = monthAppointments.Count;
            model.ScheduledThisMonth = monthAppointments.Count(a => a.Status == "Scheduled");
            model.CompletedThisMonth = monthAppointments.Count(a => a.Status == "Complete");
            model.CanceledThisMonth = monthAppointments.Count(a => a.Status == "Canceled");
        }
    }
}
