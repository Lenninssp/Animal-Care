using System;
using System.Collections.Generic;

namespace Animal_Care.Models;

public partial class Owner
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
