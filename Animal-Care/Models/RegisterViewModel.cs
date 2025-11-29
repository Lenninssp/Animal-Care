using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Animal_Care.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }

        // MUST be nullable so [Required] can work correctly
        [Required(ErrorMessage = "Please select a role.")]
        public int? RoleId { get; set; }

        // For the dropdown
        public IEnumerable<SelectListItem>? Roles { get; set; }

    }
}
