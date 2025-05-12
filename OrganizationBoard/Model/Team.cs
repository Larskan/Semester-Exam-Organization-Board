using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Team
    {
        public int TeamId { get; set; }
        public required string TeamName { get; set; }
    }
}