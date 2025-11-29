using System;
using System.Collections.Generic;

namespace Animal_Care.Models
{
    public partial class User
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string? Phone { get; set; }

        public int RoleId { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public virtual Role Role { get; set; } = null!;

        public virtual Veterinarian? Veterinarian { get; set; }
    }
}
