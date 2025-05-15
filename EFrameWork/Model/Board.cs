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
<<<<<<< HEAD
        public Team? Team { get; set; }

        public ICollection<Task> Tasks { get; set; } = new List<Task>();
=======
        public Team Team { get; set; }
        
        public ICollection<Task>? Tasks { get; set; }
>>>>>>> 0b5e9ead5066def673f81cd42c143fb250a4f860

    }
}