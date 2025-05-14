using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Role
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
    }
}