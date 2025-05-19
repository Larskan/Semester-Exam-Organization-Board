using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class UserDto
    {
        public int UserID { get; set; } = 0;
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public int RoleID { get; set; } = 0;
        public int OrganizationID { get; set; } = 0;
        public int? TeamID { get; set; } = 0;
    }
}