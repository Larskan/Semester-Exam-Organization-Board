using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EFrameWork.Model
{
    public class Board
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BoardID { get; set; }
        [Required]
        public string BoardName { get; set; }
        public int? TeamID { get; set; }
        [ForeignKey("TeamID")]
        public Team Team { get; set; }
        
        public ICollection<Task>? Tasks { get; set; }

    }
}