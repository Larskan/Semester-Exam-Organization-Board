using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Model;

namespace EFrameWork.Model
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role Role { get; set; }
        [Required]
        public int OrganizationID { get; set; }
        [ForeignKey("OrganizationID")]
        public Organization Organization { get; set; }

        [Required]
        public int TeamID { get; set; }

        [ForeignKey("TeamID")]
        public Team Team { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public ICollection<UserToTask> TaskAssignments { get; set; }
    }
}