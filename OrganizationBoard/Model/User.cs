using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class User
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int RoleId { get; set; }
        public int OrganizationId { get; set; }
        public int TeamId { get; set; }
    }
}