using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;

namespace Model
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }
        [Required]
        public RoleStatus RoleName { get; set; }

        public ICollection<User> Users { get; set; }
    }

    public enum RoleStatus
    {
        Admin,
        TeamLeader,
        TeamMember
    }

}

