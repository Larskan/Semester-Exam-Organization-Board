using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class Status
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatusID {get; set;}
        [Required]
        public TaskStatus StatusOption {get; set;}

        public ICollection<EFrameWork.Model.Task> Tasks { get; set; }
    }
    public enum TaskStatus
    {
        NotStarted,
        Ongoing,
        Done
        // TODO: Need NotStarted -> Ongoing -> Done -> Confirmed
    }

}

