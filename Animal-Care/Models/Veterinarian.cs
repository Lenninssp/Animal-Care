using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class Veterinarian
{
    public int UserId { get; set; }

    public string? Specialty { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VetSchedule> VetSchedules { get; set; } = new List<VetSchedule>();
}
