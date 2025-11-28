using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class Pet
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Species { get; set; } = null!;

    public int? Age { get; set; }

    public int OwnerId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual Owner Owner { get; set; } = null!;
}
