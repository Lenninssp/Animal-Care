namespace Animal_Care.Models
{
    public class VetWorkloadItem
    {
        public int VetId { get; set; }
        public string VetName { get; set; } = string.Empty;

        public int TotalAppointments { get; set; }
        public int ScheduledCount { get; set; }
        public int CanceledCount { get; set; }
        public int CompletedCount { get; set; }
    }

    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public int TotalAppointments { get; set; }
        public int ScheduledCount { get; set; }
        public int CanceledCount { get; set; }
        public int CompletedCount { get; set; }

        public List<VetWorkloadItem> VetWorkloads { get; set; } = new();
    }
}
