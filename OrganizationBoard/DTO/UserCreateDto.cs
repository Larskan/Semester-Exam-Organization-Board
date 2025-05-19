using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrganizationBoard.DTO
{
    public class UserCreateDto
    {
        public int? UserID { get; set; } = 0;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = "";
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one number, and one special character.")]
        public string Password { get; set; } = "";
        [Required]
        public int RoleID { get; set; } = 0;
        [Required]
        public int OrganizationID { get; set; } = 0;
        public int? TeamID { get; set; } = 0;
    }
}