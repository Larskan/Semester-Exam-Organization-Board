using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Status
    {
        public int StatusId { get; set; }
        public required TaskStatus StatusOption { get; set; }
    }

    public enum TaskStatus{
        NotStarted,
        Ongoing,
        Done
    }
}
    
