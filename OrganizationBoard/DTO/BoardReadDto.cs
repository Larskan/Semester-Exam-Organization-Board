using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class BoardReadDto
    {
        public int BoardID { get; set; }
        public string BoardName { get; set; }
        public int TeamID { get; set; }
        public string? TeamName { get; set; } // Optional: Include Team name for convenience
        public ICollection<TaskReadDto>? Tasks { get; set; } // Include a simplified Task DTO
    }
}