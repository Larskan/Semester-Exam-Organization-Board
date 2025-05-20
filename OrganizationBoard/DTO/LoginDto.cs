using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email or password")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Invalid email or password")]
        public string Password { get; set; } = "";
    }
}