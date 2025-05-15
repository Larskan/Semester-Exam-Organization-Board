using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class TaskReadDto
    {
        public int TaskID { get; set; }
        public int BoardID { get; set; }
        public string? BoardName { get; set; } // Optional: Include Board name
        public int StatusID { get; set; }
        public int StatusOption { get; set; } // Assuming Status has an Option property
        public string Title { get; set; }
        public string? Description { get; set; }
        public float? Estimation { get; set; }
        public float? NumUser { get; set; }
        // You might include a simplified list of assigned user IDs or names here
    }
}