using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class UserToTask
    {
        public int UserToTaskId { get; set; }
        public int UserId { get; set; }
        public int CardTaskId { get; set; }
    }
}