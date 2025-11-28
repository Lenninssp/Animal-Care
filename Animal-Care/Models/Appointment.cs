using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = null!;

    public int PetId { get; set; }

    public int VetId { get; set; }

    public int RecepcionistUserId { get; set; }

    public int AppointmentTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CanceledAt { get; set; }

    public virtual AppointmentType AppointmentType { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecordAppointments { get; set; } = new List<MedicalRecord>();

    public virtual MedicalRecord? MedicalRecordIdNavigation { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual User RecepcionistUser { get; set; } = null!;

    public virtual Veterinarian Vet { get; set; } = null!;
}
