using System;
using System.Collections.Generic;

namespace Animal_Care.Models
{
    public class AppointmentListItem
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }

        public string PetName { get; set; } = string.Empty;
        public string? OwnerName { get; set; }
        public string? VetName { get; set; }
        public string? TypeName { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class HomeDashboardViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public string? Role { get; set; }

        // Admin overview
        public int? TotalOwners { get; set; }
        public int? TotalPets { get; set; }
        public int? TotalUsers { get; set; }
        public int? TotalAppointmentsThisMonth { get; set; }
        public int? ScheduledThisMonth { get; set; }
        public int? CompletedThisMonth { get; set; }
        public int? CanceledThisMonth { get; set; }

        // Veterinarian
        public List<AppointmentListItem>? VetUpcomingAppointments { get; set; }

        // Receptionist
        public List<AppointmentListItem>? TodayAppointments { get; set; }

        public int TodayAppointmentsCount =>
            TodayAppointments?.Count ?? 0;
    }
}
