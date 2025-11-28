using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class MedicalRecord
{
    public int Id { get; set; }

    public DateTime VisitDate { get; set; }

    public string? Diagnosis { get; set; }

    public string? Treatment { get; set; }

    public int PetId { get; set; }

    public int VetId { get; set; }

    public int AppointmentId { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Appointment IdNavigation { get; set; } = null!;

    public virtual Pet Pet { get; set; } = null!;

    public virtual Veterinarian Vet { get; set; } = null!;
}
