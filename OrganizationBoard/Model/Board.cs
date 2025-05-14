using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.Model
{
    public class Board
    {
        public int BoardID { get; set; }
        public required string BoardName { get; set; }
        public int TeamID { get; set; }
    }
}