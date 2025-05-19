using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class AccountAndOrgDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string OrgName { get; set; } = "";    
    }
}