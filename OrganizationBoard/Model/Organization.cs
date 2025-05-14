using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public required string OrganizationName { get; set; }
    }
}