using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.DTO
{
    public class BoardDto
    {
        public int? BoardID { get; set; }
        public string BoardName { get; set; }
        public int? TeamID { get; set; }
    }
}