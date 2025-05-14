using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class CardTask
    {
        public int CardTaskId { get; set; }
        public int BoardId { get; set; }
        public int StatusId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public float EstimaredTime { get; set; }
        public int NumberOfUsers { get; set; }
    }
}