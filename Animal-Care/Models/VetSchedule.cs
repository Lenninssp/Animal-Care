using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class VetSchedule
{
    public int Id { get; set; }

    public int VetId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public virtual Veterinarian Vet { get; set; } = null!;
}
