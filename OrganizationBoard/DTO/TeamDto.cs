using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrganizationBoard.DTO
{
    public class TeamDto
    {
        [Required]
        public int TeamID { get; set; }
        [Required]
        public string TeamName { get; set; }
    }
}