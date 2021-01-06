using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSchoolWebApp.Models
{
    public class Class : EntityBase
    {
        [Required]
        [StringLength(8)]
        public string Name { get; set; }

        public Teacher Teacher { get; set; }

        public List<Student> Students { get; set; }
        public List<Parent> Parents { get; set; }
        public List<Assignment> Assignments { get; set; }
    }
}
