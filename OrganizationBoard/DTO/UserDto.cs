using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrganizationBoard.DTO
{
    public class UserDto
    {
        [Required]
        public int UserID { get; set; } = 0;
        // [Required]
        // [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; } = "";
        [Required]
        public int RoleID { get; set; } = 0;
        // [Required]
        public int? OrganizationID { get; set; } = 0;
        public int? TeamID { get; set; } = 0;
    }
}