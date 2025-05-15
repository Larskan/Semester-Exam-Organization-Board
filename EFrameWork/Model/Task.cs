using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Model;

namespace EFrameWork.Model
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }

        [Required]
        public int BoardID { get; set; }
        [ForeignKey("BoardID")]
        public Board Board { get; set; }

        [Required]
        public int StatusID { get; set; }

        [ForeignKey("StatusID")]
        public Status Status { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public float? Estimation { get; set; }
        public float? NumUser { get; set; }

        public ICollection<UserToTask>? UserAssignments { get; set; } = new HashSet<UserToTask>();
    }
}