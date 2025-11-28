using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class ClinicHour
{
    public int Id { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeSpan OpenTime { get; set; }

    public TimeSpan CloseTime { get; set; }
}
