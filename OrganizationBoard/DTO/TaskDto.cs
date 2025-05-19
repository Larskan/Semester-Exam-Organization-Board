using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class TaskDto
    {
        public int? TaskID { get; set; }
        public required int BoardID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public float? Estimation { get; set; }
        public float? NumUser { get; set; }
        // You might include a list of user IDs to assign upon creation
    }
}