using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Role
    {
        public int RoleId { get; set; }
        public required RoleStatus RoleName { get; set; }
    }

    public enum RoleStatus
    {
        Admin,
        TeamLeader,
        TeamMember
    }
}