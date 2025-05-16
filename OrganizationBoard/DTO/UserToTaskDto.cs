using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class UserToTaskDto
    {
        public int? UserID { get; set; }
        public int? TaskID { get; set; }
    }
}