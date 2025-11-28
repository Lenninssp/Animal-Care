using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class AppointmentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DurationMinutes { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
