using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Status
    {
        public int StatusId { get; set; }
        public required string StatusName { get; set; }
    }
}